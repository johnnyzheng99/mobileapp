using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions.Reactive;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxFragmentPresentation(AddToBackStack = true)]
    public sealed partial class SelectUserCalendarsFragment : ReactiveFragment<SelectUserCalendarsViewModel>
    {
        private UserCalendarsRecyclerAdapter userCalendarsAdapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.SelectUserCalendarsFragment, container, false);
            InitializeViews(view);

            setupRecyclerView();

            cancelButton
                .Rx()
                .BindAction(ViewModel.Close)
                .DisposedBy(DisposeBag);

            doneButton
                .Rx()
                .BindAction(ViewModel.Done)
                .DisposedBy(DisposeBag);

            userCalendarsAdapter
                .ItemTapObservable
                .Subscribe(ViewModel.SelectCalendar.Inputs)
                .DisposedBy(DisposeBag);

            return view;
        }

        private void setupRecyclerView()
        {
            userCalendarsAdapter = new UserCalendarsRecyclerAdapter();
            recyclerView.SetAdapter(userCalendarsAdapter);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));
        }
    }
}
