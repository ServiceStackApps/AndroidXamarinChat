using Android.App;
using Android.Widget;
using Android.OS;
using ServiceStack;
using Chat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Content.PM;
using Android.Content;
using Android.Support.Design.Widget;
using ServiceStack.Logging;
using Xamarin.Auth;
using LogManager = ServiceStack.Logging.LogManager;

namespace AndroidXamarinChat
{
    [Activity(Label = "Chat (Xamarin)", Icon = "@mipmap/ic_launcher",
        Theme = "@style/ChatApp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        public const string BaseUrl = "http://chat.servicestack.net/";
        private ChatActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private ListView rightDrawer;
        private NavigationView navigationView;

        private EditText messageBox;

        private readonly List<Exception> errors = new List<Exception>();

        private ServerEventsClient client;
        private ChatCommandHandler cmdReceiver;

        private List<ServerEventUser> subscriberList = new List<ServerEventUser>();

        private readonly Dictionary<string, string> commands = new Dictionary<string, string>
        {
            { "Announce Hello", "/cmd.announce Hello from Xamarin.Android" },
            { "Play YouTube",   "/tv.watch https://youtu.be/u5CVsCnxyXg" },
            { "Background Image", "/css.background-image url(http://bit.ly/2lxT0gh)" },
            { "Background Top", "/css.background$#top #2c3e50" },
            { "Background Color", "/css.background #ecf0f1" },
            { "Background Bottom", "/css.background$#bottom #2c3e50" },
            { "Logout", "/logout" }
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            LogManager.LogFactory = new GenericLogFactory(Console.WriteLine);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            // Get our button from the layout resource,
            // and attach an event to it
            Button sendButton = FindViewById<Button>(Resource.Id.sendMessageButton);
            messageBox = FindViewById<EditText>(Resource.Id.message);
            SupportToolbar toolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            rightDrawer = FindViewById<ListView>(Resource.Id.right_drawer);
            var messageHistoryList = FindViewById<ListView>(Resource.Id.messageHistory);
            var chatBackground = FindViewById<ImageView>(Resource.Id.chat_background);
            InitDefaultBackground(chatBackground);

            navigationView.Tag = 0;
            rightDrawer.Tag = 1;

            var messageHistoryAdapter = new MessageListViewAdapter(this, new List<ChatMessage>(), () => this.subscriberList);
            messageHistoryList.Adapter = messageHistoryAdapter;

            var channels = new[] { "home" };
            cmdReceiver = new ChatCommandHandler(this, messageHistoryAdapter, "home");
            var activity = this;

            client = new ServerEventsClient(BaseUrl, channels)
            {
                OnConnect = connectMsg =>
                {
                    client.UpdateChatHistory(cmdReceiver)
                    .ContinueWith(t => 
                        connectMsg.UpdateUserProfile(activity));
                },
                OnCommand = command =>
                {
                    if (command is ServerEventJoin)
                    {
                        client.GetChannelSubscribersAsync()
                            .ContinueWith(t => {
                                subscriberList = t.Result;
                                Application.SynchronizationContext.Post(_ => {
                                    // Refresh profile icons when users join
                                    messageHistoryAdapter.NotifyDataSetChanged();
                                }, null);
                            });
                    }
                },
                OnException = error => {
                    Application.SynchronizationContext.Post(
                        _ => { Toast.MakeText(this, "Error : " + error.Message, ToastLength.Long); }, null);
                },
                //ServiceClient = new JsonHttpClient(BaseUrl),
                Resolver = new MessageResolver(cmdReceiver)
            };
            client.RegisterNamedReceiver<ChatReceiver>("cmd");
            client.RegisterNamedReceiver<TvReciever>("tv");
            client.RegisterNamedReceiver<CssReceiver>("css");

            SetSupportActionBar(toolbar);

            var rightDataSet = new List<string>(commands.Keys);
            var rightAdapter = new ActionListViewAdapter(this, rightDataSet);
            rightDrawer.Adapter = rightAdapter;
            rightDrawer.ItemClick += (sender, args) =>
            {
                Application.SynchronizationContext.Post(_ =>
                {
                    messageBox.Text = commands[rightDataSet[args.Position]];
                    drawerLayout.CloseDrawer(rightDrawer);
                }, null);
            };

            drawerToggle = new ChatActionBarDrawerToggle(
                this,                           //Host Activity
                drawerLayout,                  //DrawerLayout
                toolbar,                       // Instance of toolbar, if you use other ctor, the hamburger icon/arrow animation won't work..
                Resource.String.openDrawer,     //Opened Message
                Resource.String.closeDrawer     //Closed Message
            );

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);

            drawerLayout.SetDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            navigationView.NavigationItemSelected += OnChannelClick;
            sendButton.Click += async (e,args) => { await OnSendClick(e, args); };
        }

        private static void InitDefaultBackground(ImageView chatBackground)
        {
            "https://servicestack.net/img/slide/image01.jpg".GetImageBitmap().ContinueWith(t =>
            {
                var bitmap = t.Result;
                Application.SynchronizationContext.Post(_ => { chatBackground.SetImageBitmap(bitmap); }, null);
            });
        }

        public void OnChannelClick(object sender, NavigationView.NavigationItemSelectedEventArgs navigationItemSelectedEventArgs)
        {
            string itemText = navigationItemSelectedEventArgs.MenuItem.TitleFormatted.ToString();
            if (itemText == UiHelpers.CreateChannelLabel)
            {
                UiHelpers.ShowChannelDialog(this)
                    .ContinueWith(ta =>
                    {
                        string nChannel = ta.Result;
                        var nChannels = new List<string>(client.Channels) { nChannel };
                        UiHelpers.ResetChannelDrawer(this, navigationView, nChannels.ToArray());
                        client.ChangeChannel(ta.Result, cmdReceiver)
                            .Error(ex => errors.Add(ex));
                    });
            }
            else
            {
                //Change channel
                client.ChangeChannel(itemText, cmdReceiver);
            }
            drawerLayout.CloseDrawer(navigationView);
        }

        public async Task OnSendClick(object sender, EventArgs e)
        {
            var hasSelector = messageBox.Text.StartsWith("/");
            string selector = hasSelector ? messageBox.Text.Substring(1).SplitOnFirst(" ")[0] : "cmd.chat";
            string message = hasSelector && messageBox.Text.Contains(" ") ? messageBox.Text.Substring(1).SplitOnFirst(" ")[1] : messageBox.Text;

            if (selector == "cmd.chat")
            {
                await client.ServiceClient.PostAsync(new PostChatToChannel
                {
                    Channel = cmdReceiver.CurrentChannel,
                    From = client.SubscriptionId,
                    Message = message,
                    Selector = selector
                });
            }
            else if (selector == "logout")
            {
                PerformLogout();
            }
            else
            {
                await client.ServiceClient.PostAsync(new PostRawToChannel
                {
                    Channel = cmdReceiver.CurrentChannel,
                    From = client.SubscriptionId,
                    Message = message,
                    Selector = selector
                });
            }

            messageBox.Text = "";
        }

        private void PerformLogout()
        {
            var txtUser = FindViewById<TextView>(Resource.Id.txtUserName);
            AccountStore.Create(this).Delete(new Account(txtUser.Text), "Twitter");
            client.ServiceClient.ClearCookies();
            var intent = new Intent(BaseContext, typeof(LoginActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

            StartActivity(intent);
            Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_help:
                    if (drawerLayout.IsDrawerOpen(rightDrawer))
                    {
                        //Right Drawer is already open, close it
                        drawerLayout.CloseDrawer(rightDrawer);
                    }
                    else
                    {
                        //Right Drawer is closed, open it and just in case close left drawer
                        drawerLayout.OpenDrawer(rightDrawer);
                        drawerLayout.CloseDrawer(navigationView);
                    }
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            this.UpdateCookiesFromIntent(client);

            drawerToggle.SyncState();
            UiHelpers.ResetChannelDrawer(this, navigationView, client.Channels);
            client.Connect().ConfigureAwait(false);
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            drawerToggle.OnConfigurationChanged(newConfig);
        }

        protected override void OnDestroy()
        {
            client.Stop();
            cmdReceiver = null;
            base.OnDestroy();
        }
    }
}
