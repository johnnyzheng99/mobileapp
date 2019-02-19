using System;
using Android.Runtime;
using Android.Views;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using Toggl.Giskard.ViewHolders;

namespace Toggl.Giskard.Adapters
{
    public sealed class UserCalendarsRecyclerAdapter : BaseRecyclerAdapter<SelectableUserCalendarViewModel>
    {
        protected override BaseRecyclerViewHolder<SelectableUserCalendarViewModel> CreateViewHolder(ViewGroup parent, LayoutInflater inflater, int viewType)
        {
            var inflatedView = inflater.Inflate(Resource.Layout.UserCalendarItem, parent, false);
            return new UserCalendarViewHolder(inflatedView);
        }
    }
}
