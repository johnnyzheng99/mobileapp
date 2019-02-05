using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CoreGraphics;
using Toggl.Daneel.Cells;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Collections.Changes;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public interface IObservableScroll
    {
        IObservable<CGPoint> ScrollOffset { get; }
        IObservable<bool> IsDragging { get; }
    }

    public abstract class ReactiveSectionedListTableViewSource<TModel, TCell>
        : MutableSectionedListTableViewSource<TModel, TCell>, IObservableScroll
        where TCell : BaseTableViewCell<TModel>
    {
        public IObservable<ICollectionChange> CollectionChange { get; }

        public IObservable<CGPoint> ScrollOffset
            => scrolledSubject.AsObservable();

        public IObservable<bool> IsDragging
            => isDraggingSubject.AsObservable();

        private Subject<CGPoint> scrolledSubject = new Subject<CGPoint>();
        private Subject<bool> isDraggingSubject = new Subject<bool>();

        public ReactiveSectionedListTableViewSource(ObservableGroupedOrderedCollection<TModel> collection, string cellIdentifier)
            : base (collection, cellIdentifier)
        {
            CollectionChange = collection.CollectionChange;
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            scrolledSubject.OnNext(scrollView.ContentOffset);
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            isDraggingSubject.OnNext(true);
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            isDraggingSubject.OnNext(false);
        }

        public abstract void RefreshHeader(UITableView tableView, int section);
    }
}
