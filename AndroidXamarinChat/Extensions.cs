using System;
using Chat;
using System.Threading.Tasks;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Android.App;
using Android.Widget;

namespace AndroidXamarinChat
{
	public static class Extensions
	{
		public static string DisplayMessage(this ChatMessage chatMessage)
		{
			return chatMessage.FromName + ": " + chatMessage.Message + "\n";
		}

	    public static Task UpdateChatHistory(this ServerEventsClient client, ChatCommandHandler cmdReceiver)
	    {
	        return Task.Run(() =>
	        {
	            var chatHistory = client.ServiceClient.Get(new GetChatHistory
	            {
	                Channels = client.Channels
	            });
	            cmdReceiver.FullHistory = new Dictionary<string, List<ChatMessage>>();
	            try
	            {
	                foreach (var channel in client.Channels)
	                {
	                    var currentChannel = channel;
	                    cmdReceiver.FullHistory.Add(channel,
	                        chatHistory.Results
	                            .Where(x => x.Channel == currentChannel)
	                            .Select(x => x).ToList());
	                }
	            }
	            catch (Exception e)
	            {
	                Console.WriteLine(e);
	            }
	            cmdReceiver.SyncAdapter();
	        });
	    }

        public static void ChangeChannel(this ServerEventsClient client, string channel, ChatCommandHandler cmdReceiver)
        {
            var currentChannels = new List<string>(client.Channels);

            if (cmdReceiver.FullHistory.ContainsKey(channel) && currentChannels.Contains(channel))
            {
                cmdReceiver.ChangeChannel(channel);
            }
            else {

                if (!currentChannels.Contains(channel))
                    currentChannels.Add(channel);

                client.SubscribeToChannels(currentChannels.ToArray());
                cmdReceiver.CurrentChannel = channel;
                client.UpdateChatHistory(cmdReceiver).ConfigureAwait(false);
            }
        }

        public static void SendMessage(this ServerEventsClient client, PostRawToChannel request)
        {
            client.ServiceClient.Post(request);
        }

        public static void UpdateUserProfile(this ServerEventConnect connectMsg, MainActivity activity)
        {
            var txtUser = activity.FindViewById<TextView>(Resource.Id.txtUserName);
            var imgProfile = activity.FindViewById<ImageView>(Resource.Id.imgProfile);

            Application.SynchronizationContext.Post(_ => { txtUser.Text = connectMsg.DisplayName; }, null);
            connectMsg.ProfileUrl.GetImageBitmap()
                .ContinueWith(
                    bitmap =>
                    {
                        Application.SynchronizationContext.Post(_ => { imgProfile.SetImageBitmap(bitmap.Result); }, null);
                    });
        }

        public static void UpdateCookiesFromIntent(this MainActivity mainActivity, ServerEventsClient client)
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

		public static Task<List<ServerEventCommand>> GetSubscribers(this ServerEventsClient client)
		{
			var task = client.ServiceClient.GetAsync<List<ServerEventCommand>>(
				"/event-subscribers?{0}".Fmt(client.Channels.Join(",")));
				task.ConfigureAwait(false);
			return task;
		}
    }
}