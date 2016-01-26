using System;
using Chat;
using System.Collections.Generic;
using ServiceStack;

namespace AndroidXamarinChat
{
	public class ChatClient : ServerEventsClient
	{
		private const string baseUrl = "http://chat.servicestack.net/";

		public ChatClient (string[] channels)
			: base (baseUrl, channels)
		{
			this.RegisterNamedReceiver<ChatReceiver> ("cmd");
			this.RegisterNamedReceiver<TvReciever> ("tv");
		    this.RegisterNamedReceiver<CssReceiver>("css");
		}
			
		public void SendMessage(PostRawToChannel request)
		{
			this.ServiceClient.Post (request);
		}

		public void ChangeChannel(string channel, ChatCmdReciever cmdReceiver)
		{
			var currentChannels = new List<string> (this.Channels);
			if (cmdReceiver.FullHistory.ContainsKey (channel) && currentChannels.Contains (channel)) {
				cmdReceiver.ChangeChannel (channel);
			} else {
				currentChannels.Add (channel);
				this.Channels = currentChannels.ToArray ();
				if (Channels != null && Channels.Length > 0)
					this.EventStreamUri = this.EventStreamUri
						.AddQueryParam("channel", string.Join(",", Channels));
				cmdReceiver.CurrentChannel = channel;
                cmdReceiver.FullHistory.Add(channel,new List<string>());
                this.Restart ();
			}
		}
	}
}