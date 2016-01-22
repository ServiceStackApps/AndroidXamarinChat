using System;
using Chat;
using System.Threading.Tasks;
using ServiceStack;
using System.Collections.Generic;

namespace AndroidXamarinChat
{
	public static class Extensions
	{
		public static string DisplayMessage(this ChatMessage chatMessage)
		{
			return chatMessage.FromName + ": " + chatMessage.Message + "\n";
		}

		public static void UpdateChatHistory(this ChatClient client, string[] channels, string currentChannel, ChatCmdReciever cmdReceiver)
		{
			Task.Run (() => {
				var chatHistory = client.ServiceClient.Get(new GetChatHistory { Channels = channels});
				client.HistoryCache = chatHistory.Results;
				chatHistory.Results.ForEach ((cm) => {
					if(cm.Channel == currentChannel)
						cmdReceiver.AppendMessage(cm);
				});
			});
		}

		public static void ChangeChannel(this ChatClient client, string channel, ChatCmdReciever cmdReceiver)
		{
			var currentChannels = new List<string> (client.Channels);
			if (currentChannels.Contains (channel)) {
				cmdReceiver.ChangeChannel (channel, client.HistoryCache);
			} else {
				var updatedChannels = new List<string> (client.Channels);
				updatedChannels.Add (channel);
				client.Channels = updatedChannels.ToArray ();
				client.Restart ();
				client.UpdateChatHistory (client.Channels, channel, cmdReceiver);
			}
		}
	}
}