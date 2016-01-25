using System;
using Android.Support.V7.App;
using Android.Widget;
using Android.App;
using System.Globalization;
using System.Threading.Tasks;
using Android.Views.InputMethods;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;

namespace AndroidXamarinChat
{
	public class UIHelpers
	{
		public const string CreateChannelLabel = "Create Channel";

		public static Task<string> ShowChannelDialog(Activity activity)
		{
			var tcs = new TaskCompletionSource<string>();
			var inputDialog = new Android.Support.V7.App.AlertDialog.Builder(activity);
			var userInput = new EditText (inputDialog.Context);
			userInput.Hint = "New Channel";
			string selectedInput = "Create new channel";
			userInput.Text = "";
			//SetEditTextStylings(userInput);
			userInput.InputType = Android.Text.InputTypes.ClassText;
			inputDialog.SetTitle(selectedInput);
			inputDialog.SetView(userInput);
			inputDialog.SetPositiveButton(
				"Ok",
				(see, ess) => 
				{
					if (userInput.Text != string.Empty)
					{
						tcs.SetResult(userInput.Text);
					}
					else
					{
						tcs.SetResult("");
					}
					//HideKeyboard(userInput);
				});
			inputDialog.SetNegativeButton("Cancel", (afk, kfa) => { 
				//HideKeyboard(userInput); 
			});
			inputDialog.Show();
			return tcs.Task;
		}

		private static void ShowKeyboard(Activity parentActivity, EditText userInput)
		{
			parentActivity.RunOnUiThread (() => {
				userInput.RequestFocus();
				InputMethodManager imm = (InputMethodManager)parentActivity.GetSystemService(Context.InputMethodService);
				imm.ToggleSoftInput(ShowFlags.Forced, 0);
			});
		}

		private static void HideKeyboard(Activity parentActivity,EditText userInput)
		{
			parentActivity.RunOnUiThread (() => {
				InputMethodManager imm = (InputMethodManager)parentActivity.GetSystemService(Context.InputMethodService);
				imm.HideSoftInputFromWindow(userInput.WindowToken, 0);
			});
		}

		public static void ResetChannelDrawer(Activity parentActivity, NavigationView navigationView, string[] channels)
		{
			parentActivity.RunOnUiThread(() => {
                var subMenu = navigationView.Menu.GetItem(0).SubMenu;
                subMenu.Clear();
                for (int i = 0; i < channels.Length; i++)
			    {
                    var chanMenuItem = subMenu.Add(Resource.Id.channelsGroup, Menu.None, Menu.None, channels[i]);
                    chanMenuItem.SetIcon(Resource.Drawable.ic_discuss);
                }
                var createChanMenuItem = subMenu.Add(Resource.Id.channelsGroup, Menu.None, Menu.None, CreateChannelLabel);
                createChanMenuItem.SetIcon(Resource.Drawable.ic_plus_circle_white_24dp);
                navigationView.RefreshDrawableState();
            });
		}
	}
}

