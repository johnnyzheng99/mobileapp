using Android.OS;
using Android.Views;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar;

namespace Toggl.Giskard.Fragments
{
    public sealed partial class SelectUserCalendarsFragment : ReactiveFragment<SelectUserCalendarsViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.SelectUserCalendarsFragment, container, false);
            InitializeViews(view);

            return view;
        }
    }
}
