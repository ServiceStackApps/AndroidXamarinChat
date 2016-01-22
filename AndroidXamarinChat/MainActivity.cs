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

namespace AndroidXamarinChat
{
	[Activity (Label = "AndroidXamarinChat", MainLauncher = true, Icon = "@mipmap/icon",
		Theme="@style/ChatApp")]
	public class MainActivity : AppCompatActivity
	{
		string[] channels = { "home" };
		string currentChannel = "home";

		private ChatActionBarDrawerToggle mDrawerToggle;
		private DrawerLayout mDrawerLayout;
		private ListView mLeftDrawer;
		private ListView mRightDrawer;

		private ListView messageHistoryList;

		private ArrayAdapter mLeftAdapter;
		private ArrayAdapter mRightAdapter;
		private ArrayAdapter messageHistoryAdapter;

		private List<string> mRightDataSet;

		private List<string> messageHistoryDataSet;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button sendButton = FindViewById<Button>(Resource.Id.sendMessageButton);
			EditText messageBox = FindViewById<EditText>(Resource.Id.message);
			SupportToolbar mToolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
			mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			mLeftDrawer = FindViewById<ListView>(Resource.Id.left_drawer);
			mRightDrawer = FindViewById<ListView>(Resource.Id.right_drawer);
			messageHistoryList = FindViewById<ListView>(Resource.Id.messageHistory);

			mLeftDrawer.Tag = 0;
			mRightDrawer.Tag = 1;

			SetSupportActionBar(mToolbar);
			var leftMenuData = new List<string> (channels);
			leftMenuData.Add ("Create Channel+");
			mLeftAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, leftMenuData);
			mLeftDrawer.Adapter = mLeftAdapter;

			messageHistoryDataSet = new List<string> ();
			messageHistoryAdapter = new ArrayAdapter (this, Android.Resource.Layout.SimpleListItem1, messageHistoryDataSet);
			messageHistoryList.Adapter = messageHistoryAdapter;


			mRightDataSet = new List<string>();
			mRightDataSet.Add ("Right Item 1");
			mRightDataSet.Add ("Right Item 2");
			mRightAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mRightDataSet);
			mRightDrawer.Adapter = mRightAdapter;

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

			if (bundle != null)
			{
				if (bundle.GetString("DrawerState") == "Opened")
				{
					SupportActionBar.SetTitle(Resource.String.openDrawer);
				}

				else
				{
					SupportActionBar.SetTitle(Resource.String.closeDrawer);
				}
			}

			else
			{
				//This is the first the time the activity is ran
				SupportActionBar.SetTitle(Resource.String.closeDrawer);
			}

			ServerEventConnect connectMsg = null;
			var errors = new List<Exception>();

			var client = new ServerEventsClient("http://chat.servicestack.net",channels:channels.Join())
			{
				OnConnect = e =>
				{
					connectMsg = e;
				},
				OnCommand = message =>
				{

				},
				OnMessage = message =>
				{

				},
				OnException = errors.Add,
			}.Start();

			client.RegisterReceiver<ChatReceiver>();
			MessageView messageView = new MessageView (this, this.messageHistoryAdapter);

			var chatHistory = client.ServiceClient.Get(new GetChatHistory { Channels = channels});
			chatHistory.Results.ForEach ((cm) => {
				if(cm.Channel == currentChannel)
					messageView.AppendMessage(cm);
			});
			sendButton.Click += delegate
			{
				try
				{
					Task.Run(() => {
						var response = client.ServiceClient.Post<ChatMessage>(new PostChatToChannel
							{
								Channel = currentChannel,
								From = connectMsg.Id,
								Message = messageBox.Text,
								Selector = "cmd.chat"
							});

						messageView.AppendMessage(response);
						this.RunOnUiThread(() => {
							messageBox.Text = "";
						});
					});
				}
				catch (Exception exception)
				{

					throw;
				}

			};
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{		
			switch (item.ItemId)
			{

			case Android.Resource.Id.Home:
				//The hamburger icon was clicked which means the drawer toggle will handle the event
				//all we need to do is ensure the right drawer is closed so the don't overlap
				mDrawerLayout.CloseDrawer (mRightDrawer);
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
					mDrawerLayout.OpenDrawer (mRightDrawer);
					mDrawerLayout.CloseDrawer (mLeftDrawer);
				}

				return true;

			default:
				return base.OnOptionsItemSelected (item);
			}
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.action_menu, menu);
			return base.OnCreateOptionsMenu (menu);
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			if (mDrawerLayout.IsDrawerOpen((int)GravityFlags.Left))
			{
				outState.PutString("DrawerState", "Opened");
			}

			else
			{
				outState.PutString("DrawerState", "Closed");
			}

			base.OnSaveInstanceState (outState);
		}

		protected override void OnPostCreate (Bundle savedInstanceState)
		{
			base.OnPostCreate (savedInstanceState);
			mDrawerToggle.SyncState();
		}

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);
			mDrawerToggle.OnConfigurationChanged(newConfig);
		}
	}

	public class ChatReceiver : ServerEventReceiver
	{
		private readonly Activity activity;
		private readonly EditText clientMessageBox;
		private readonly EditText messageView;

		public ChatReceiver(Activity activity)
		{
			this.activity = activity;
			clientMessageBox = activity.FindViewById<EditText>(Resource.Id.sendMessageButton);
			messageView = activity.FindViewById<EditText>(Resource.Id.messageHistory);
		}

		public void Chat(ChatMessage chatMessage)
		{
			activity.RunOnUiThread(() =>
				{
					messageView.Text += chatMessage.DisplayMessage();
				});
		}
	}

	public class MessageView
	{
		private Activity parentActivity;
		private ArrayAdapter messageAdapter;
		public MessageView(Activity parentActivity, ArrayAdapter messageAdapter)
		{
			this.parentActivity = parentActivity;
			this.messageAdapter = messageAdapter;
		}

		public void AppendMessage(ChatMessage chatMessage)
		{
			parentActivity.RunOnUiThread (() => {
				messageAdapter.Add (chatMessage.DisplayMessage ());
				messageAdapter.NotifyDataSetChanged ();
			});
		}
	}
}
