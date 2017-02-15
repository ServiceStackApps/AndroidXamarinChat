using Android.App;
using Android.Widget;
using Android.OS;
using ServiceStack;
using Chat;
using System;
using System.Collections.Generic;
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
        private ChatActionBarDrawerToggle mDrawerToggle;
        private DrawerLayout mDrawerLayout;
        private ListView mRightDrawer;
        private NavigationView navigationView;

        private EditText messageBox;

        private readonly List<Exception> errors = new List<Exception>();

        private ServerEventsClient client;
        private ChatCommandHandler cmdReceiver;

        private List<ServerEventCommand> subscriberList = new List<ServerEventCommand>();

        private readonly Dictionary<string, string> commands = new Dictionary<string, string>
        {
            {"Announce Hello","/cmd.announce Hello from Android"},
            { "Play YouTube", "/tv.watch https://youtu.be/u5CVsCnxyXg" },
            { "Set background color", "/css.background$#top #0091ea"},
            { "Reset background color", "/css.background$#top #ffffff"},
            {"Logout","/logout" }
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
            SupportToolbar mToolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            mRightDrawer = FindViewById<ListView>(Resource.Id.right_drawer);
            var messageHistoryList = FindViewById<ListView>(Resource.Id.messageHistory);
            var chatBackground = FindViewById<ImageView>(Resource.Id.chat_background);
            InitDefaultBackground(chatBackground);

            navigationView.Tag = 0;
            mRightDrawer.Tag = 1;

            var messageHistoryAdapter = new MessageListViewAdapter(this, new List<ChatMessage>(), () => this.subscriberList);
            messageHistoryList.Adapter = messageHistoryAdapter;

            var channels = new[] { "home" };
            cmdReceiver = new ChatCommandHandler(this, messageHistoryAdapter, "home");
            var activity = this;

            client = new ServerEventsClient(BaseUrl, channels)
            {
                OnConnect = connectMsg =>
                {
                    client.UpdateChatHistory(cmdReceiver).ConfigureAwait(false);
                    connectMsg.UpdateUserProfile(activity);
                },
                OnCommand = command =>
                {
                    if (command is ServerEventJoin)
                    {
                        client.GetSubscribers().ContinueWith(result =>
                        {
                            result.Wait();
                            subscriberList = result.Result;
                            Application.SynchronizationContext.Post(_ =>
                            {
                                // Refresh profile icons when users join
                                messageHistoryAdapter.NotifyDataSetChanged();
                            }, null);
                        });
                    }
                },
                OnException =
                    error =>
                    {
                        Application.SynchronizationContext.Post(
                            _ => { Toast.MakeText(this, "Error : " + error.Message, ToastLength.Long); }, null);
                    },
                //ServiceClient = new JsonHttpClient(BaseUrl),
                Resolver = new MessageResolver(cmdReceiver)
            };
            client.RegisterNamedReceiver<ChatReceiver>("cmd");
            client.RegisterNamedReceiver<TvReciever>("tv");
            client.RegisterNamedReceiver<CssReceiver>("css");

            SetSupportActionBar(mToolbar);

            var mRightDataSet = new List<string>(commands.Keys);
            var mRightAdapter = new ActionListViewAdapter(this, mRightDataSet);
            mRightDrawer.Adapter = mRightAdapter;
            mRightDrawer.ItemClick += (sender, args) =>
            {
                Application.SynchronizationContext.Post(_ =>
                {
                    messageBox.Text = commands[mRightDataSet[args.Position]];
                    mDrawerLayout.CloseDrawer(mRightDrawer);
                }, null);
            };

            mDrawerToggle = new ChatActionBarDrawerToggle(
                this,                           //Host Activity
                mDrawerLayout,                  //DrawerLayout
                mToolbar,                       // Instance of toolbar, if you use other ctor, the hamburger icon/arrow animation won't work..
                Resource.String.openDrawer,     //Opened Message
                Resource.String.closeDrawer     //Closed Message
            );

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);

            mDrawerLayout.SetDrawerListener(mDrawerToggle);
            mDrawerToggle.SyncState();

            navigationView.NavigationItemSelected += OnChannelClick;
            sendButton.Click += OnSendClick;
        }

        private static void InitDefaultBackground(ImageView chatBackground)
        {
            "https://servicestack.net/img/slide/image01.jpg".GetImageBitmap().ContinueWith(t =>
            {
                t.Wait();
                var bitmap = t.Result;
                Application.SynchronizationContext.Post(_ => { chatBackground.SetImageBitmap(bitmap); }, null);
            });
        }

        public void OnChannelClick(object sender, NavigationView.NavigationItemSelectedEventArgs navigationItemSelectedEventArgs)
        {
            string itemText = navigationItemSelectedEventArgs.MenuItem.TitleFormatted.ToString();
            if (itemText == UiHelpers.CreateChannelLabel)
            {
                var result = UiHelpers.ShowChannelDialog(this);
                result.ContinueWith(ta =>
                {
                    ta.Wait();
                    try
                    {
                        string nChannel = ta.Result;
                        var nChannels = new List<string>(client.Channels);
                        nChannels.Add(nChannel);
                        UiHelpers.ResetChannelDrawer(this, navigationView, nChannels.ToArray());
                        client.ChangeChannel(ta.Result, cmdReceiver);
                        cmdReceiver.SyncAdapter();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                });
            }
            else
            {
                //Change channel
                client.ChangeChannel(itemText, cmdReceiver);
            }
            mDrawerLayout.CloseDrawer(navigationView);
        }

        public void OnSendClick(object sender, EventArgs e)
        {
            var hasSelector = messageBox.Text.StartsWith("/");
            string selector = hasSelector ? messageBox.Text.Substring(1).SplitOnFirst(" ")[0] : "cmd.chat";
            string message = hasSelector && messageBox.Text.Contains(" ") ? messageBox.Text.Substring(1).SplitOnFirst(" ")[1] : messageBox.Text;

            if (selector == "cmd.chat")
            {
                client.ServiceClient.PostAsync(new PostChatToChannel
                {
                    Channel = cmdReceiver.CurrentChannel,
                    From = client.SubscriptionId,
                    Message = message,
                    Selector = selector
                }).ConfigureAwait(false);
            }
            else if (selector == "logout")
            {
                PerformLogout();
            }
            else
            {
                client.ServiceClient.PostAsync(new PostRawToChannel
                {
                    Channel = cmdReceiver.CurrentChannel,
                    From = client.SubscriptionId,
                    Message = message,
                    Selector = selector
                }).ConfigureAwait(false);
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
                    if (mDrawerLayout.IsDrawerOpen(mRightDrawer))
                    {
                        //Right Drawer is already open, close it
                        mDrawerLayout.CloseDrawer(mRightDrawer);
                    }
                    else
                    {
                        //Right Drawer is closed, open it and just in case close left drawer
                        mDrawerLayout.OpenDrawer(mRightDrawer);
                        mDrawerLayout.CloseDrawer(navigationView);
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

            mDrawerToggle.SyncState();
            UiHelpers.ResetChannelDrawer(this, navigationView, client.Channels);
            client.Connect().ConfigureAwait(false);
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            mDrawerToggle.OnConfigurationChanged(newConfig);
        }

        protected override void OnDestroy()
        {
            client.Stop();
            cmdReceiver = null;
            base.OnDestroy();
        }
    }
}
