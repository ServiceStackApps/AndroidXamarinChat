using System;
using Chat;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceStack;

namespace AndroidXamarinChat
{
	public class ChatClient : ServerEventsClient, IDisposable
	{
		public const string BaseUrl = "http://chat.servicestack.net/";

        public string ProfileUrl { get; set; }

	    public bool updatingFullHistory = false;
        
		public ChatClient (string[] channels)
			: base (BaseUrl, channels)
		{
			this.RegisterNamedReceiver<ChatReceiver> ("cmd");
			this.RegisterNamedReceiver<TvReciever> ("tv");
		    this.RegisterNamedReceiver<CssReceiver>("css");
		    (this.ServiceClient as ServiceClientBase).HandleCallbackOnUiThread = false;
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
                if (updatingFullHistory)
                {
                    var task = this.UpdateChatHistory(cmdReceiver);
                    task.ConfigureAwait(false);
                    task.ContinueWith(t =>
                    {
                        this.Restart();
                    });
                }
                else
                {
                    this.Restart();
                }
			}
		}

        public async Task UpdateChatHistory(ChatCmdReciever cmdReceiver)
        {
            updatingFullHistory = true;
            await Task.Run(() => {
                var chatHistory = this.ServiceClient.Get(new GetChatHistory
                {
                    Channels = this.Channels
                });
                cmdReceiver.FullHistory = new Dictionary<string, List<string>>();
                chatHistory.Results.ForEach(message => {
                    if (!cmdReceiver.FullHistory.ContainsKey(message.Channel))
                    {
                        cmdReceiver.FullHistory.Add(message.Channel, new List<string>());
                    }
                    cmdReceiver.FullHistory[message.Channel].Add(message.DisplayMessage());
                });
                cmdReceiver.SyncAdapter();
                updatingFullHistory = false;
            });
        }

	    public void Dispose()
	    {
	        base.Dispose();
            this.ServiceClient.CancelAsync();
            this.ServiceClient.Dispose();
	        this.ServiceClient = null;
	    }

    }
}