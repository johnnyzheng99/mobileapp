﻿using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Toggl.Giskard.ViewHolders
{
    public class UserCalendarHeaderViewHolder : BaseRecyclerViewHolder<string>
    {
        private TextView sourceName;

        public static UserCalendarViewHolder Create(View itemView)
            => new UserCalendarViewHolder(itemView);

        public UserCalendarHeaderViewHolder(View itemView)
            : base(itemView)
        {
        }

        public UserCalendarHeaderViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            sourceName = ItemView.FindViewById<TextView>(Resource.Id.CalendarSource);
        }

        protected override void UpdateView()
        {
            sourceName.Text = Item;
        }

    }
}
