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
            return (chatMessage.FromName + ": " + chatMessage.Message + "\n").UnescapeHtml();
        }

        public static Task UpdateChatHistory(this ServerEventsClient client, ChatCommandHandler cmdReceiver)
        {
            return client.ServiceClient.GetAsync(new GetChatHistory {
                Channels = client.Channels
            }).Success(chatHistory => 
            {
                cmdReceiver.FullHistory = new Dictionary<string, List<ChatMessage>>();
                foreach (var channel in client.Channels)
                {
                    var currentChannel = channel;
                    cmdReceiver.FullHistory.Add(channel,
                        chatHistory.Results
                            .Where(x => x.Channel == currentChannel)
                            .Select(x => x).ToList());
                }
                cmdReceiver.SyncAdapter();
            });
        }

        public static Task ChangeChannel(this ServerEventsClient client, string channel, ChatCommandHandler cmdReceiver)
        {
            if (cmdReceiver.FullHistory.ContainsKey(channel) && client.Channels.Contains(channel))
            {
                cmdReceiver.ChangeChannel(channel);
                cmdReceiver.SyncAdapter();
                return Task.CompletedTask;
            }

            return client.SubscribeToChannelsAsync(channel)
                .Then(t =>
                {
                    cmdReceiver.CurrentChannel = channel;
                    return client.UpdateChatHistory(cmdReceiver);
                });
        }

        public static void SendMessage(this ServerEventsClient client, PostRawToChannel request)
        {
            client.ServiceClient.Post(request);
        }

        public static async Task UpdateUserProfile(this ServerEventConnect connectMsg, Activity activity)
        {
            Application.SynchronizationContext.Post(_ =>
            {
                var txtUser = activity.FindViewById<TextView>(Resource.Id.txtUserName);
                txtUser.Text = connectMsg.DisplayName;
            }, null);

            var bitmap = await connectMsg.ProfileUrl.GetImageBitmap();
            Application.SynchronizationContext.Post(_ =>
            {
                var imgProfile = activity.FindViewById<ImageView>(Resource.Id.imgProfile);
                imgProfile.SetImageBitmap(bitmap);
            }, null);
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

        public static string UnescapeHtml(this string html)
        {
            return html
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&#39;", "\'")
                .Replace("&quot;", "\"");
        }
    }
}