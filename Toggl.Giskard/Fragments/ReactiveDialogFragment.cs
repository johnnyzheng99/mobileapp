using System.Reactive.Disposables;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.ViewModels;

namespace Toggl.Giskard.Fragments
{
    public abstract class ReactiveDialogFragment<TViewModel> : MvxDialogFragment<TViewModel>
        where TViewModel : class, IMvxViewModel
    {
        protected CompositeDisposable DisposeBag = new CompositeDisposable();

        protected abstract void InitializeViews(View view);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            DisposeBag?.Dispose();
        }
    }
}
