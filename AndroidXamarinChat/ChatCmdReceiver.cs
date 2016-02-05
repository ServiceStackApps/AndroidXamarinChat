using System;
using Android.App;
using Android.Widget;
using Chat;
using ServiceStack;
using ServiceStack.Configuration;
using Android.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;

namespace AndroidXamarinChat
{
	public class ChatCmdReciever
	{
		private Activity parentActivity;
		private ArrayAdapter messageAdapter;
		public Dictionary<string,List<string>> FullHistory { get; set; }

		public string CurrentChannel  { get; set; }

		public ChatCmdReciever(Activity parentActivity, ArrayAdapter messageAdapter, string initialChannel)
		{
			this.parentActivity = parentActivity;
			this.messageAdapter = messageAdapter;
			this.FullHistory = new Dictionary<string, List<string>> ();
			this.CurrentChannel = initialChannel;
		}

		public void AppendMessage(ChatMessage chatMessage)
		{
			if (!FullHistory.ContainsKey (chatMessage.Channel)) {
				FullHistory.Add (chatMessage.Channel, new List<string> ());
			}
			FullHistory [chatMessage.Channel].Add (chatMessage.DisplayMessage ());
			if (chatMessage.Channel == this.CurrentChannel) {
				Application.SynchronizationContext.Post(_ =>
				{
                    messageAdapter.Add(chatMessage.DisplayMessage());
                    messageAdapter.NotifyDataSetChanged();
                },null);
			}
		}

		public void ChangeChannel(string channel)
		{
			this.CurrentChannel = channel;
			Application.SynchronizationContext.Post(_ =>
			{
                messageAdapter.Clear();
                if (FullHistory.ContainsKey(channel))
                {
                    FullHistory[channel].ForEach(msg => {
                        messageAdapter.Add(msg);
                    });
                }
            },null);
		}

		public void SyncAdapter()
		{
			ChangeChannel (this.CurrentChannel);
		}

		public void ShowVideo(string videoUrl)
		{
			parentActivity.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(videoUrl)));
		}

		public void Announce(string message)
		{
            Notification.Builder builder = new Notification.Builder(parentActivity)
                                .SetLocalOnly(true)
                                .SetAutoCancel(true)
                                .SetContentTitle("Chat (Xamarin)")
                                .SetContentText(message)
                                .SetSmallIcon(Resource.Drawable.ic_stat_icon);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                parentActivity.GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);

            Vibrator vibrator = (Vibrator)parentActivity.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(1000);
		    CancelNotification(notificationManager).ConfigureAwait(false);
		}

	    private async Task CancelNotification(NotificationManager notificationManager)
	    {
	        await Task.Delay(5000);
            notificationManager.CancelAll();
	    }

	    public void ChangeBackground(string message)
	    {
	        var url = message.StartsWith("url(") ? message.Substring(4, message.Length - 5) : message;
	        url.GetImageBitmap().ContinueWith(t =>
             {
                 var bitmap = t.Result;
                 var chatBackground = parentActivity.FindViewById<ImageView>(Resource.Id.chat_background);
                 Application.SynchronizationContext.Post(_ =>
                 {
                     chatBackground.SetImageBitmap(bitmap);
                 },null);
             });
        }
	}

	public class MessageResolver : IResolver
	{
		ChatCmdReciever messageHandler;
		public MessageResolver(ChatCmdReciever messageHandler)
		{
			this.messageHandler = messageHandler;
		}

		public T TryResolve<T> ()
		{
			if (typeof(T) == typeof(ChatReceiver)) {
				return (T)(new ChatReceiver (this.messageHandler) as object);
			}
            if (typeof(T) == typeof(TvReciever)) {
				return (T)(new TvReciever (this.messageHandler) as object);
			}
		    if (typeof (T) == typeof (CssReceiver)) {
		        return (T)(new CssReceiver(this.messageHandler) as object);
		    }
			
			return typeof(T).CreateInstance<T> ();
		}
	}
}