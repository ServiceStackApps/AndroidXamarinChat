using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Chat;
using ServiceStack;

namespace AndroidXamarinChat
{
	public class MessageListViewAdapter : BaseAdapter<ChatMessage>
	{
	    private readonly Context context;
	    private readonly List<ChatMessage> items;
	    private readonly Func<List<ServerEventCommand>> subscribers;

	    public MessageListViewAdapter (Context context, List<ChatMessage> items, Func<List<ServerEventCommand>> subscribers)
	    {
	        this.context = context;
	        this.items = items;
	        this.subscribers = subscribers;
	    }

	    public override long GetItemId(int position)
	    {
	        return position;
	    }

	    public override View GetView(int position, View convertView, ViewGroup parent)
	    {
            View row = convertView;
            if (row == null)
                row = LayoutInflater.From(context).Inflate(Resource.Layout.chat_message_item, null, false);
            var label = row.FindViewById<TextView>(Resource.Id.txtMessage);
	        var message = items[position];
	        string profileUrl = null;
	        var subs = new List<ServerEventCommand>(subscribers());
            foreach (var subscriber in subs)
	        {
	            if (message.FromUserId == subscriber.UserId)
	            {
	                profileUrl = subscriber.ProfileUrl;
	            }
	        }
	        profileUrl = profileUrl ??
	                     "https://raw.githubusercontent.com/ServiceStack/Assets/master/img/apps/no-profile64.png";
            label.Text = items[position].DisplayMessage();
	        var image = row.FindViewById<ImageView>(Resource.Id.imgUser);
	        profileUrl.GetImageBitmap().ContinueWith(bitmap =>
	        {
                Application.SynchronizationContext.Post(_ =>
                {
                    image.SetImageBitmap(bitmap.Result);
                }, null);
	        }).ConfigureAwait(false);
            return row;
        }

	    public override int Count
	    {
	        get { return items.Count; }
	    }

	    public override ChatMessage this[int position]
        {
            get { return items[position]; }
        }

	    public void Add(ChatMessage chatMessage)
	    {
	        items.Add(chatMessage);
            this.NotifyDataSetChanged();
	    }

	    public void Clear()
	    {
	        items.Clear();
            this.NotifyDataSetChanged();
	    }
	}
}