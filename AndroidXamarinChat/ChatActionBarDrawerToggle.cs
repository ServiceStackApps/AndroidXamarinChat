using System;
using Android.Support.V7.App;
using Android.Support.V4.Widget;

namespace AndroidXamarinChat
{
	public class ChatActionBarDrawerToggle : ActionBarDrawerToggle
	{
		private AppCompatActivity mHostActivity;
		private int mOpenedResource;
		private int mClosedResource;

		public ChatActionBarDrawerToggle (AppCompatActivity host, 
			DrawerLayout drawerLayout,
			Android.Support.V7.Widget.Toolbar toolbar,
			int openedResource, 
			int closedResource) 
			: base(host, drawerLayout,toolbar,openedResource,closedResource)
		{
			mHostActivity = host;
			mOpenedResource = openedResource;
			mClosedResource = closedResource;
		}

		public override void OnDrawerOpened (Android.Views.View drawerView)
		{	
			int drawerType = (int)drawerView.Tag;

			if (drawerType == 0)
			{
				base.OnDrawerOpened (drawerView);
				mHostActivity.SupportActionBar.SetTitle(mOpenedResource);
			}
		}

		public override void OnDrawerClosed (Android.Views.View drawerView)
		{
			int drawerType = (int)drawerView.Tag;

			if (drawerType == 0)
			{
				base.OnDrawerClosed (drawerView);
				mHostActivity.SupportActionBar.SetTitle(mClosedResource);
			}				
		}

		public override void OnDrawerSlide (Android.Views.View drawerView, float slideOffset)
		{
			int drawerType = (int)drawerView.Tag;

			if (drawerType == 0)
			{
				base.OnDrawerSlide (drawerView, slideOffset);
			}
		}
	}
}

