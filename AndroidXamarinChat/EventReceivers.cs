using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chat;
using ServiceStack;

namespace AndroidXamarinChat
{
    public class ChatReceiver : ServerEventReceiver
    {
        private readonly ChatCommandHandler chatMessageHandler;

        public ChatReceiver(ChatCommandHandler chatMessageHandler)
        {
            this.chatMessageHandler = chatMessageHandler;
        }

        public void Chat(ChatMessage chatMessage)
        {
            chatMessageHandler.AppendMessage(chatMessage);
        }

        public void Announce(string message)
        {
            chatMessageHandler.Announce(message);
        }
    }

    public class TvReciever : ServerEventReceiver
    {
        private readonly ChatCommandHandler chatMessageHandler;

        public TvReciever(ChatCommandHandler chatMessageHandler)
        {
            this.chatMessageHandler = chatMessageHandler;
        }

        public void Watch(string videoUrl)
        {
            chatMessageHandler.ShowVideo(videoUrl);
        }
    }

    public class CssReceiver : ServerEventReceiver
    {
        private readonly ChatCommandHandler chatMessageHandler;

        public CssReceiver(ChatCommandHandler chatMessageHandler)
        {
            this.chatMessageHandler = chatMessageHandler;
        }

        public void BackgroundImage(string message)
        {
            chatMessageHandler.ChangeBackground(message);
        }
    }
}