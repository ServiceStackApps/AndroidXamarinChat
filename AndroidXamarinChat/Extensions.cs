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
			await Task.Run (async () => {
				var httpClient = new HttpClient();
				string chatHistoryJson = await httpClient.GetStringAsync(
						"http://chat.servicestack.net/chathistory?Channels={0}&format=json".Fmt(client.Channels.Join(",")));
				var chatHistory = chatHistoryJson.FromJson<GetChatHistoryResponse>();
					
				//var chatHistory = "http://chat.servicestack.net/chathistory?Channels={0}".Fmt(client.Channels.Join(",")).GetJsonFromUrl().FromJson<GetChatHistoryResponse>();
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