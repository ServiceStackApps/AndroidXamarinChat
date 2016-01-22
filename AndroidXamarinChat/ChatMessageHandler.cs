using System;
using Android.App;
using Android.Widget;
using Chat;
using ServiceStack;
using ServiceStack.Configuration;

namespace AndroidXamarinChat
{
	public class ChatMessageHandler
	{
		private Activity parentActivity;
		private ArrayAdapter messageAdapter;
		public ChatMessageHandler(Activity parentActivity, ArrayAdapter messageAdapter)
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
			
		}

	}

	public class MessageResolver : IResolver
	{
		ChatMessageHandler messageHandler;
		public MessageResolver(ChatMessageHandler messageHandler)
		{
			this.messageHandler = messageHandler;
		}

		public T TryResolve<T> ()
		{
			if (typeof(T) == typeof(ChatReceiver)) {
				return (T)(new ChatReceiver (this.messageHandler) as object);
			}
			else
				return typeof(T).CreateInstance<T> ();
		}
	}
}