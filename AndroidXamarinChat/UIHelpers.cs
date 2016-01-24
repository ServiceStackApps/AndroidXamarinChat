using System;
using Android.Support.V7.App;
using Android.Widget;
using Android.App;
using System.Globalization;
using System.Threading.Tasks;
using Android.Views.InputMethods;
using Android.Content;

namespace AndroidXamarinChat
{
	public class UIHelpers
	{
		public static Task<string> ShowChannelDialog(Activity activity)
		{
			var tcs = new TaskCompletionSource<string>();
			var inputDialog = new Android.Support.V7.App.AlertDialog.Builder(activity);
			var userInput = new EditText (activity);
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

		public static void AddChannelToDrawer(Activity parentActivity, ArrayAdapter drawerAdapter, string channelName)
		{
			parentActivity.RunOnUiThread (() => {
				drawerAdapter.Insert (channelName, drawerAdapter.Count - 1);
				drawerAdapter.NotifyDataSetChanged ();
			});
		}
	}
}

