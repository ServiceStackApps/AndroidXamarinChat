using Android.App;
using Android.Widget;
using Android.OS;
using ServiceStack;
using Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Views;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Preferences;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Xamarin.Auth;

namespace AndroidXamarinChat
{
	[Activity (Label = "Chat (Xamarin)", Icon = "@mipmap/ic_launcher",
		Theme="@style/ChatApp", ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : AppCompatActivity
	{
		private ChatActionBarDrawerToggle mDrawerToggle;
		private DrawerLayout mDrawerLayout;
		private ListView mRightDrawer;
	    private NavigationView navigationView;

		private ListView messageHistoryList;

		private ArrayAdapter mRightAdapter;
		private ArrayAdapter messageHistoryAdapter;
		private List<string> mRightDataSet;
		private EditText messageBox;

		private List<string> messageHistoryDataSet;
	    private readonly List<Exception> errors = new List<Exception> ();

	    private ChatClient client;
		private ChatCmdReciever cmdReceiver;

        private Dictionary<string,string> commands = new Dictionary<string, string>
        {
            {"Announce Hello","/cmd.announce Hello from Android"},
            { "Play YouTube", "/tv.watch https://youtu.be/u5CVsCnxyXg" }
        }; 

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

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
			messageHistoryList = FindViewById<ListView>(Resource.Id.messageHistory);
            var chatBackground = FindViewById<ImageView>(Resource.Id.chat_background);
            "https://servicestack.net/img/slide/image01.jpg".GetImageBitmap().ContinueWith(t =>
            {
                t.Wait();
                var bitmap = t.Result;
                Application.SynchronizationContext.Post(_ =>
                {
                    chatBackground.SetImageBitmap(bitmap);
                },null);
            });

            navigationView.Tag = 0;
			mRightDrawer.Tag = 1;

			messageHistoryDataSet = new List<string> ();
			messageHistoryAdapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleListItem1, messageHistoryDataSet);
			messageHistoryList.Adapter = messageHistoryAdapter;

		    var txtUser = FindViewById<TextView>(Resource.Id.txtUserName);
		    var imgProfile = FindViewById<ImageView>(Resource.Id.imgProfile);

			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var channels = prefs.GetString ("Channels", null);
			var lastChannel = prefs.GetString ("LastChannel", null);
			cmdReceiver = new ChatCmdReciever (this, messageHistoryAdapter, lastChannel ?? "home");
		    var chanArray = channels != null ? channels.Split (',') : new[] {"home"};

			client = new ChatClient(chanArray)
		    {
		        OnConnect = connectMsg =>
		        {
		            Task.Run(() =>
		            {
		                client.UpdateChatHistory(cmdReceiver).ConfigureAwait(false);
		            }).ConfigureAwait(false);
					
		            Application.SynchronizationContext.Post(_ =>
		            {
                        txtUser.Text = connectMsg.DisplayName;
                    },null);
		            connectMsg.ProfileUrl.GetImageBitmap().ContinueWith(bitmap =>
		            {
                        Application.SynchronizationContext.Post(_ =>
                        {
                            imgProfile.SetImageBitmap(bitmap.Result);
                        },null);
		            });
		        },
		        OnException = error => { 
					errors.Add(error); 
				}
		    };
		    try
		    {
                var accountDetailsResponseTask = client.ServiceClient.GetAsync(new GetUserDetails());
                accountDetailsResponseTask.ConfigureAwait(false);
                accountDetailsResponseTask.ContinueWith(response =>
                {
                    // Do nothing, cookie is still valid.
                });
            }
		    catch (Exception e)
		    {
		        if (!(e is WebServiceException)) throw;
		        var intent = new Intent(this.BaseContext,typeof(LoginActivity));
		        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
		        StartActivity(intent);
		        Finish();
		        throw;
		    }
		    

		    SetSupportActionBar(mToolbar);

		    mRightDataSet = new List<string>(commands.Keys);
		    mRightAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mRightDataSet);
			mRightDrawer.Adapter = mRightAdapter;
		    mRightDrawer.ItemClick += (sender, args) =>
		    {
               Application.SynchronizationContext.Post(_ =>
               {
                   messageBox.Text = commands[mRightDataSet[args.Position]];
                   mDrawerLayout.CloseDrawer(mRightDrawer);
               },null);
		    };

			mDrawerToggle = new ChatActionBarDrawerToggle(
				this,							//Host Activity
				mDrawerLayout,					//DrawerLayout
				mToolbar,						// Instance of toolbar, if you use other ctor, the hamburger icon/arrow animation won't work..
				Resource.String.openDrawer,		//Opened Message
				Resource.String.closeDrawer		//Closed Message
			);

			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowTitleEnabled(true);

			mDrawerLayout.SetDrawerListener(mDrawerToggle);
			mDrawerToggle.SyncState();

		    navigationView.NavigationItemSelected += this.OnChannelClick;
            sendButton.Click += OnSendClick;
		}


		public void OnChannelClick(object sender, NavigationView.NavigationItemSelectedEventArgs navigationItemSelectedEventArgs)
		{
		    string itemText = navigationItemSelectedEventArgs.MenuItem.TitleFormatted.ToString();
			if(itemText == UiHelpers.CreateChannelLabel) {
				var result = UiHelpers.ShowChannelDialog(this);
				messageHistoryAdapter.Clear();
				messageHistoryAdapter.NotifyDataSetChanged();
				result.ContinueWith(ta => {
					ta.Wait();
					try{
						string nChannel = ta.Result;
                        var nChannels = new List<string>(client.Channels);
                        nChannels.Add(nChannel);
                        UiHelpers.ResetChannelDrawer(this,navigationView,nChannels.ToArray());
						client.ChangeChannel(ta.Result,cmdReceiver);
                        SaveChannelInfo();
                    } catch (Exception ex) 
					{
						errors.Add(ex);
					}					
				});
			} else {
				//Change channel
				client.ChangeChannel(itemText, cmdReceiver);
                SaveChannelInfo();
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
                JsonHttpClient httpClient = new JsonHttpClient(ChatClient.BaseUrl);
		        httpClient.CookieContainer = (client.ServiceClient as ServiceClientBase).CookieContainer;
		        var dto = new PostChatToChannel
		        {
		            Channel = cmdReceiver.CurrentChannel,
		            From = client.SubscriptionId,
		            Message = message,
		            Selector = selector
		        };

		        httpClient.PostAsync(dto).ConfigureAwait(false);
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

		    Application.SynchronizationContext.Post(_ =>
		    {
                messageBox.Text = "";
            }, null);
		}

	    private void PerformLogout()
	    {
	        var txtUser = this.FindViewById<TextView>(Resource.Id.txtUserName);
	        AccountStore.Create(this).Delete(new Account(txtUser.Text), "Twitter");
	        client.ServiceClient.ClearCookies();
	        var intent = new Intent(this.BaseContext, typeof (LoginActivity));
	        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

	        StartActivity(intent);
	        Finish();
	    }

	    public override void Finish()
	    {
	        base.Finish();
	        cmdReceiver = null;
	    }

	    public override bool OnOptionsItemSelected (IMenuItem item)
		{
		    switch (item.ItemId)
		    {
		        case Android.Resource.Id.Home:
		            //The hamburger icon was clicked which means the drawer toggle will handle the event
		            //all we need to do is ensure the right drawer is closed so the don't overlap
		            mDrawerLayout.CloseDrawer(mRightDrawer);
		            mDrawerToggle.OnOptionsItemSelected(item);
		            return true;

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

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.action_menu, menu);
			return base.OnCreateOptionsMenu (menu);
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
		    SaveChannelInfo();
            base.OnSaveInstanceState (outState);
		}

	    private void SaveChannelInfo()
	    {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("Channels", client.Channels.Join(","));
            editor.PutString("LastChannel", cmdReceiver.CurrentChannel);
            editor.Apply();
        }
			
	    protected override void OnPostCreate (Bundle savedInstanceState)
		{
			base.OnPostCreate (savedInstanceState);

	        UpdateCookiesFromIntent(this, client);

            mDrawerToggle.SyncState();
            UiHelpers.ResetChannelDrawer (this, navigationView, client.Channels);
			client.Resolver = new MessageResolver (cmdReceiver);
	        client.Connect().ConfigureAwait(false);
		}

	    private static void UpdateCookiesFromIntent(MainActivity mainActivity, ServerEventsClient client)
	    {
	        if (mainActivity.Intent == null)
                return;
	        string cookieStr = mainActivity.Intent.GetStringExtra("SSCookie");
	        if (string.IsNullOrEmpty(cookieStr) || !cookieStr.Contains(';'))
                return;
	        var cookies = cookieStr.Split(';');
	        foreach (var c in cookies)
	        {
	            var key = c.Split('=')[0].Trim();
	            var val = c.Split('=')[1].Trim();
	            client.ServiceClient.SetCookie(key, val);
	        }
	    }

	    public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);
			mDrawerToggle.OnConfigurationChanged(newConfig);
		}
	}
}
