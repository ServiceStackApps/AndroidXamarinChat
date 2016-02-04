using System;
using System.Collections.Generic;
using Android.Support.V7.App;
using Android.Widget;
using Android.App;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Views.InputMethods;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Views;
using Java.Nio;
using ServiceStack;

namespace AndroidXamarinChat
{
	public static class UiHelpers
	{
		public const string CreateChannelLabel = "      Join";

		public static Task<string> ShowChannelDialog(Activity activity)
		{
			var tcs = new TaskCompletionSource<string>();
			var inputDialog = new Android.Support.V7.App.AlertDialog.Builder(activity);
			var userInput = new EditText (inputDialog.Context);
			userInput.Hint = "New Channel";
			string selectedInput = "Create new channel";
			userInput.Text = "general";
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
			        chanMenuItem.SetCheckable(true);
			        chanMenuItem.SetEnabled(true);
			    }
                var createChanMenuItem = subMenu.Add(Resource.Id.channelsGroup, Menu.None, Menu.None, CreateChannelLabel);
                createChanMenuItem.SetIcon(Resource.Drawable.ic_plus_circle_white_24dp);
                navigationView.RefreshDrawableState();
            });
		}

	    public static void SelectChannel(Activity parentActivity, NavigationView navigationView, string channel)
	    {
            parentActivity.RunOnUiThread(() =>
            {
                var subMenu = navigationView.Menu.GetItem(0).SubMenu;
                for (int i = 0; i < subMenu.Size(); i++)
                {
                    var item = subMenu.GetItem(i);
                    if (item.TitleFormatted.ToString() == channel)
                    {
                        navigationView.SetCheckedItem(item.ItemId);
                    }
                    else
                    {
                        item.SetChecked(false);
                    }
                }
                
                navigationView.RefreshDrawableState();
            });
        }

	    public static void UpdateImageViewSrc(Activity activity, int imageViewRsc, string url)
	    {

	        url.GetImageBitmap().ContinueWith(t =>
	        {
	            activity.RunOnUiThread(() =>
	            {
	                var bitmap = t.Result;
	                var imageView = activity.FindViewById<ImageView>(imageViewRsc);
	                imageView.SetImageBitmap(bitmap);
	            });
	        });
	    }

        private static readonly Dictionary<string,byte[]> BackgroundCache = new Dictionary<string, byte[]>(); 

	    public static Task<Bitmap> GetImageBitmap(this string url)
	    {
	        var task = new Task<Bitmap>(() =>
	        {
                byte[] bytes;
                if (BackgroundCache.ContainsKey(url))
                {
                    bytes = BackgroundCache[url];
                }
                else
                {
                    bytes = url.GetBytesFromUrl();
                    BackgroundCache.Add(url, bytes);
                }
                var bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                return bitmap;
            });
	        task.ConfigureAwait(false);
            task.Start();
            return task;
	    }
	}

    public static class UserImageHandler
    {
        public static Task<Bitmap> GetProfileImage(string url)
        {
            string imagePath = GetProfileImagePath(url);
            bool getFromCache = !File.Exists(imagePath);
            if (getFromCache)
            {
                return ReadImageFromCache(url);
            }

            var task = url.GetImageBitmap();
            task.ContinueWith(t =>
            {
                CacheImage(url, t.Result);
            });
            return task;
        }

        private static string FileNameRegex = "[|\\\\?*<\\\\\":>\\[\\]/#\']";

        public static string GetProfileImagePath(string url)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string fileName = Regex.Replace(url, FileNameRegex, "", RegexOptions.IgnoreCase);
            return System.IO.Path.Combine(path, fileName);
        }

        private static void CacheImage(string url, Bitmap bitmap)
        {
            string imagePath = GetProfileImagePath(url);
            if (!File.Exists(imagePath))
            {
                ByteBuffer byteBuffer = ByteBuffer.Allocate(bitmap.ByteCount);
                bitmap.CopyPixelsToBuffer(byteBuffer);
                MemoryStream ms = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
                byte[] bytes = ms.ToBytes();
                using (var fs = File.Create(imagePath))
                {
                    fs.Write(bytes,0,bytes.Length);
                }
            }
        }

        private static Task<Bitmap> ReadImageFromCache(string url)
        {
            string imagePath = GetProfileImagePath(url);
            return BitmapFactory.DecodeFileAsync(imagePath);
        }
    }
}

