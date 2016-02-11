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

namespace AndroidXamarinChat
{
    public class ActionListViewAdapter : BaseAdapter<string>
    {
        private readonly Context context;
        private readonly List<string> items;

        public ActionListViewAdapter(Context context, List<string> items)
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if (row == null)
                row = LayoutInflater.From(context).Inflate(Resource.Layout.action_row_item, null, false);
            var label = row.FindViewById<TextView>(Resource.Id.actionLabel);
            label.Text = items[position];
            return row;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override string this[int position]
        {
            get { return items[position]; }
        }
    }
}