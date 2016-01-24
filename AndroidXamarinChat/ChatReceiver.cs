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
			chatMessageHandler.Announce (message);
		}

		public void Toggle(string message) 
		{ 

		}

		public void BackgroundImage(string cssRule) 
		{

		}
	}
}

