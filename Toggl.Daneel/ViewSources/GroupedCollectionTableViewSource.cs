using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Foundation;
using MvvmCross.Binding.Extensions;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.ViewModels;
using Toggl.Daneel.Views.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public abstract class GroupedCollectionTableViewSource<TCollection, TItem> : MvxTableViewSource
        where TCollection : MvxObservableCollection<TItem>, new()
        where TItem : class
    {
        private readonly string cellIdentifier;
        private readonly string headerCellIdentifier;
        private readonly object animationLock = new object();

        private MvxObservableCollection<TCollection> observableCollection;

        private readonly List<TCollection> displayedGroupedItems = new List<TCollection>();

        protected IReadOnlyCollection<TCollection> GroupedItems { get; }

        public override IEnumerable ItemsSource
        {
            get => GroupedItems;
            set { throw new InvalidOperationException($"You must bind to the {nameof(ObservableCollection)} and not the {nameof(ItemsSource)}"); }
        }

        public MvxObservableCollection<TCollection> ObservableCollection
        {
            get => observableCollection;
            set
            {
                if (observableCollection != null)
                {
                    observableCollection.CollectionChanged -= OnCollectionChanged;
                }

                observableCollection = value;
                cloneCollection();
                base.ItemsSource = GroupedItems;

                if (observableCollection != null)
                {
                    observableCollection.CollectionChanged += OnCollectionChanged;
                }
            }
        }

        protected GroupedCollectionTableViewSource(UITableView tableView, string cellIdentifier, string headerCellIdentifier)
            : base(tableView)
        {
            this.cellIdentifier = cellIdentifier;
            this.headerCellIdentifier = headerCellIdentifier;

            UseAnimations = true;

            GroupedItems = displayedGroupedItems.AsReadOnly();
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var grouping = GetGroupAt(section);

            var cell = GetOrCreateHeaderViewFor(tableView);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = grouping;

            if (section == 0 && cell is IHeaderViewCellWithHideableTopSeparator headerCell)
                headerCell.TopSeparatorHidden = true;

            return cell;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);

            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
            => ItemsSource.Count();

        public override nint RowsInSection(UITableView tableview, nint section)
            => GetGroupAt(section).Count();

        protected virtual IEnumerable<TItem> GetGroupAt(nint section)
            => displayedGroupedItems.ElementAtOrDefault((int)section) ?? new TCollection();

        protected override object GetItemAt(NSIndexPath indexPath)
            => displayedGroupedItems.ElementAtOrDefault(indexPath.Section)?.ElementAtOrDefault((int)indexPath.Item);

        public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
        {
            var firstVisible = TableView.IndexPathsForVisibleRows.FirstOrDefault();
            if (firstVisible == null) return;

            if (firstVisible.Section != section + 1) return;

            var nextHeader = TableView.GetHeaderView(firstVisible.Section) as IHeaderViewCellWithHideableTopSeparator;
            if (nextHeader == null) return;

            nextHeader.TopSeparatorHidden = true;
        }

        public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
        {
            var headerViewCell = headerView as IHeaderViewCellWithHideableTopSeparator;
            if (headerViewCell == null) return;

            var firstVisibleIndexPath = TableView.IndexPathsForVisibleRows.FirstOrDefault();
            if (firstVisibleIndexPath == null) return;

            if (firstVisibleIndexPath.Section == section)
            {
                var nextHeader = TableView.GetHeaderView(section + 1) as IHeaderViewCellWithHideableTopSeparator;
                if (nextHeader == null) return;
                nextHeader.TopSeparatorHidden = false;
                headerViewCell.TopSeparatorHidden = true;
            }
            else
            {
                headerViewCell.TopSeparatorHidden = false;
            }
        }

        protected virtual UITableViewHeaderFooterView GetOrCreateHeaderViewFor(UITableView tableView)
            => tableView.DequeueReusableHeaderFooterView(headerCellIdentifier);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            tryAnimateOnMainThread(() => animateSectionChangesIfPossible(args));
        }

        private void tryAnimateOnMainThread(Action animate)
        {
            InvokeOnMainThread(() =>
            {
                if (!UseAnimations)
                {
                    reloadTable();
                    return;
                }

                try
                {
                    animate();
                }
                catch
                {
                    reloadTable();
                }
            });
        }

        private void animateSectionChangesIfPossible(NotifyCollectionChangedEventArgs args)
        {
            lock (animationLock)
            {
                TableView.BeginUpdates();

                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        var addedSection = CloneCollection(observableCollection[args.NewStartingIndex]);
                        displayedGroupedItems.Insert(args.NewStartingIndex, addedSection);

                        var indexToAdd = NSIndexSet.FromIndex(args.NewStartingIndex);
                        TableView.InsertSections(indexToAdd, UITableViewRowAnimation.Automatic);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        displayedGroupedItems.RemoveAt(args.OldStartingIndex);

                        var indexToRemove = NSIndexSet.FromIndex(args.OldStartingIndex);
                        TableView.DeleteSections(indexToRemove, UITableViewRowAnimation.Automatic);
                        break;

                    case NotifyCollectionChangedAction.Move when args.NewItems.Count == 1 && args.OldItems.Count == 1:
                        var movedSection = CloneCollection(observableCollection[args.NewStartingIndex]);
                        displayedGroupedItems.RemoveAt(args.OldStartingIndex);
                        displayedGroupedItems.Insert(args.NewStartingIndex, movedSection);

                        TableView.MoveSection(args.OldStartingIndex, args.NewStartingIndex);
                        break;

                    case NotifyCollectionChangedAction.Replace when args.NewItems.Count == args.OldItems.Count:
                        var replacedSection = CloneCollection(observableCollection[args.NewStartingIndex]);
                        displayedGroupedItems[args.NewStartingIndex] = replacedSection;

                        var indexSet = NSIndexSet.FromIndex(args.NewStartingIndex);
                        TableView.ReloadSections(indexSet, ReplaceAnimation);
                        break;

                    default:
                        reloadTable();
                        break;
                }

                TableView.EndUpdates();
            }
        }

        private void reloadTable()
        {
            cloneCollection();
            ReloadTableData();
        }

        private void cloneCollection()
        {
            displayedGroupedItems.Clear();
            displayedGroupedItems.AddRange(observableCollection.Select(CloneCollection));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing || ObservableCollection == null) return;

            ObservableCollection.CollectionChanged -= OnCollectionChanged;
        }

        protected abstract TCollection CloneCollection(TCollection collection);
    }
}
