using System;
using Android.App;
using Android.Widget;
using Chat;
using ServiceStack;
using ServiceStack.Configuration;
using Android.Content;
using System.Collections.Generic;

namespace AndroidXamarinChat
{
	public class ChatCmdReciever
	{
		private Activity parentActivity;
		private ArrayAdapter messageAdapter;
		public Dictionary<string,List<string>> FullHistory { get; set; }

		public string CurrentChannel  { get; private set; }

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
				parentActivity.RunOnUiThread (() => {
					messageAdapter.Add (chatMessage.DisplayMessage ());
					messageAdapter.NotifyDataSetChanged ();
				});
			}
		}

		public void ChangeChannel(string channel)
		{
			this.CurrentChannel = channel;
			parentActivity.RunOnUiThread (() => {
				messageAdapter.Clear ();
				if (FullHistory.ContainsKey (channel)) {
					FullHistory [channel].ForEach (msg => {
						messageAdapter.Add (msg);
					});
				}
			});
		}

		public void ShowVideo(string videoUrl)
		{
			parentActivity.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(videoUrl)));
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
			} else if (typeof(T) == typeof(TvReciever)) {
				return (T)(new TvReciever (this.messageHandler) as object);
			}
			else
				return typeof(T).CreateInstance<T> ();
		}
	}
}