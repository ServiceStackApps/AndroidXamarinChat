using System;
using Chat;
using System.Threading.Tasks;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace AndroidXamarinChat
{
	public static class Extensions
	{
		public static string DisplayMessage(this ChatMessage chatMessage)
		{
			return chatMessage.FromName + ": " + chatMessage.Message + "\n";
		}

		public static async Task UpdateChatHistory(this ChatClient client,ChatCmdReciever cmdReceiver)
		{
			await Task.Run (() => {
				var chatHistory = client.JsonHttpClient.Get(new GetChatHistory { 
					Channels = client.Channels
				});
				cmdReceiver.FullHistory = new Dictionary<string, List<string>> ();
				chatHistory.Results.ForEach (message => {
					if (!cmdReceiver.FullHistory.ContainsKey (message.Channel)) {
						cmdReceiver.FullHistory.Add (message.Channel, new List<string> ());
					}
					cmdReceiver.FullHistory [message.Channel].Add (message.DisplayMessage ());
				});
				cmdReceiver.SyncAdapter ();
			});
		}
	}
}