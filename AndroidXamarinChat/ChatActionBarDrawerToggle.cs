using System;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;

namespace AndroidXamarinChat
{
	public class ChatActionBarDrawerToggle : ActionBarDrawerToggle
	{
		private AppCompatActivity mHostActivity;
	    private readonly DrawerLayout drawerLayout;
	    private int mOpenedResource;
		private int mClosedResource;

	    private bool rightIsClosed = true;
	    private bool leftIsClosed = true;

	    public ChatActionBarDrawerToggle (AppCompatActivity host, 
			DrawerLayout drawerLayout,
			Android.Support.V7.Widget.Toolbar toolbar,
			int openedResource, 
			int closedResource) 
			: base(host, drawerLayout,toolbar,openedResource,closedResource)
		{
			mHostActivity = host;
	        this.drawerLayout = drawerLayout;
	        mOpenedResource = openedResource;
			mClosedResource = closedResource;
        }

	    public override void OnDrawerOpened (View drawerView)
		{
			int drawerType = (int)drawerView.Tag;

            if (drawerType == 0)
			{
				base.OnDrawerOpened (drawerView);
				mHostActivity.SupportActionBar.SetTitle(mOpenedResource);
			}

	        if (drawerView.Id == Resource.Id.nav_view)
	            leftIsClosed = false;

	        if (drawerView.Id == Resource.Id.right_drawer)
	            rightIsClosed = false;
		}

		public override void OnDrawerClosed (View drawerView)
		{
			int drawerType = (int)drawerView.Tag;

			if (drawerType == 0)
			{
				base.OnDrawerClosed (drawerView);
				mHostActivity.SupportActionBar.SetTitle(mClosedResource);
			}

            if (drawerView.Id == Resource.Id.nav_view)
                leftIsClosed = true;

            if (drawerView.Id == Resource.Id.right_drawer)
                rightIsClosed = true;
        }

		public override void OnDrawerSlide (View drawerView, float slideOffset)
		{
			int drawerType = (int)drawerView.Tag;

            var leftDrawer = mHostActivity.FindViewById<NavigationView>(Resource.Id.nav_view);
            var rightDrawer = mHostActivity.FindViewById<ListView>(Resource.Id.right_drawer);
            switch (drawerView.Id)
            {
                case Resource.Id.right_drawer:
                    if (drawerLayout.IsDrawerOpen(leftDrawer) && !leftIsClosed)
                    {
                        drawerLayout.CloseDrawer(leftDrawer);
                        leftIsClosed = true;
                    }
                    break;
                case Resource.Id.nav_view:

                    if (drawerLayout.IsDrawerOpen(rightDrawer) && !rightIsClosed)
                    {
                        drawerLayout.CloseDrawer(rightDrawer);
                        rightIsClosed = true;
                    }
                    break;
            }

            if (drawerType == 0)
			{
				base.OnDrawerSlide (drawerView, slideOffset);
			}
		}
	}
}

