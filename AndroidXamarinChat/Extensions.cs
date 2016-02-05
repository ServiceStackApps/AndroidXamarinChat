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
	}
}