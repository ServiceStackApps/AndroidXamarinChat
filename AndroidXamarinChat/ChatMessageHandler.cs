using System;
using Android.App;
using Android.Widget;
using Chat;
using ServiceStack;
using ServiceStack.Configuration;
using Android.Content;

namespace AndroidXamarinChat
{
	public class ChatCmdReciever
	{
		private Activity parentActivity;
		private ArrayAdapter messageAdapter;
		public ChatCmdReciever(Activity parentActivity, ArrayAdapter messageAdapter)
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