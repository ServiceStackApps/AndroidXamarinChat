﻿using System;
using Chat;
using System.Collections.Generic;
using ServiceStack;

namespace AndroidXamarinChat
{
	public class ChatClient : ServerEventsClient
	{
		public const string BaseUrl = "http://chat.layoric.org/";

        public string ProfileUrl { get; set; }
        
		public ChatClient (string[] channels)
			: base (BaseUrl, channels)
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
                if(!currentChannels.Contains(channel))
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