using System;
using ServiceStack;
using Chat;
using System.Collections.Generic;

namespace AndroidXamarinChat
{
	public class ChatReceiver : ServerEventReceiver
	{
		private readonly ChatCmdReciever chatMessageHandler;

		public ChatReceiver(ChatCmdReciever chatMessageHandler)
		{
			this.chatMessageHandler = chatMessageHandler;
		}

		public void Chat(ChatMessage chatMessage)
		{
			chatMessageHandler.AppendMessage (chatMessage);
		}

		public void Announce(string message)
		{

		}

		public void Toggle(string message) 
		{ 

		}

		public void BackgroundImage(string cssRule) 
		{

		}
	}

	public class ChatClient : ServerEventsClient
	{
		public List<ChatMessage> HistoryCache { get; set; }
		public string CurrentChannel  { get; set; }

		public ChatClient (string[] channels)
			: base ("http://chat.servicestack.net/", channels)
		{
			CurrentChannel = channels [0];
		}

		public void SendMessage(PostChatToChannel request)
		{
			this.ServiceClient.Post (request);
		}

		public void ChangeChannel(string channel, ChatCmdReciever cmdReceiver)
		{
			this.CurrentChannel = channel;
			var currentChannels = new List<string> (this.Channels);

			if (currentChannels.Contains (channel)) {
				cmdReceiver.ChangeChannel (channel, this.HistoryCache);
			} else {
				currentChannels.Add (channel);
				this.Channels = currentChannels.ToArray ();
				if (Channels != null && Channels.Length > 0)
					this.EventStreamUri = this.EventStreamUri
						.AddQueryParam("channel", string.Join(",", Channels));
				this.Restart ();
				this.UpdateChatHistory (this.Channels, channel, cmdReceiver);
			}
		}
	}
}

