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

        public void Background(string message)
        {
            chatMessageHandler.ChangeBackgroundColor(message);
        }
    }
}