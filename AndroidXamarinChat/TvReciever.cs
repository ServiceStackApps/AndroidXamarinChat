﻿using System;
using ServiceStack;

namespace AndroidXamarinChat
{
	public class TvReciever : ServerEventReceiver
	{
		private readonly ChatCmdReciever chatMessageHandler;

		public TvReciever(ChatCmdReciever chatMessageHandler)
		{
			this.chatMessageHandler = chatMessageHandler;
		}

		public void Watch(string videoUrl)
		{
			chatMessageHandler.ShowVideo (videoUrl);
		}
	}
}
