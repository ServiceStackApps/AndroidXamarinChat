using System;
using Chat;
using System.Threading.Tasks;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;

namespace AndroidXamarinChat
{
	public static class Extensions
	{
		public static string DisplayMessage(this ChatMessage chatMessage)
		{
			return chatMessage.FromName + ": " + chatMessage.Message + "\n";
		}

		public static Task UpdateChatHistory(this ChatClient client, string[] channels, ChatCmdReciever cmdReceiver)
		{
			return Task.Run (() => {
				var chatHistory = client.ServiceClient.Get(new GetChatHistory { Channels = channels});
				cmdReceiver.FullHistory = new Dictionary<string, List<string>>();
				chatHistory.Results.ForEach(message =>  {
					if(!cmdReceiver.FullHistory.ContainsKey(message.Channel)) {
						cmdReceiver.FullHistory.Add(message.Channel,new List<string>());
					}
					cmdReceiver.FullHistory[message.Channel].Add(message.DisplayMessage());
				});
			});
		}

		public static void ChangeChannel(this ChatClient client, string channel, ChatCmdReciever cmdReceiver)
		{
			var currentChannels = new List<string> (client.Channels);
			if (currentChannels.Contains (channel)) {
				cmdReceiver.ChangeChannel (channel);
			} else {
				var updatedChannels = new List<string> (client.Channels);
				updatedChannels.Add (channel);
				client.Channels = updatedChannels.ToArray ();
				client.Restart ();
				client.UpdateChatHistory (client.Channels, cmdReceiver);
				cmdReceiver.ChangeChannel (channel);
			}
		}
	}
}