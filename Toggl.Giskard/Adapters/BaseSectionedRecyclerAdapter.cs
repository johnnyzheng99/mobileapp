using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Util;
using Android.Support.V7.Widget;
using Android.Views;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Interfaces;
using Toggl.Giskard.ViewHolders;
using Toggl.Multivac;

namespace Toggl.Giskard.Adapters
{
    public abstract class BaseSectionedRecyclerAdapter<TSection, TItem, TSectionViewHolder, TItemViewHolder> : RecyclerView.Adapter
        where TItemViewHolder : BaseRecyclerViewHolder<TItem>
        where TSectionViewHolder : BaseRecyclerViewHolder<TSection>
        where TItem : IDiffable<TItem>
        where TSection : IDiffable<TSection>
    {
        public const int SectionViewType = 0;
        public const int ItemViewType = 1;

        private readonly object updateLock = new object();
        private bool isUpdateRunning;

        private IList<Either<TSection, TItem>> currentItems = new List<Either<TSection, TItem>>();
        private IList<Either<TSection, TItem>> nextUpdate;

        private Subject<TItem> itemTapSubject = new Subject<TItem>();

        private IList<CollectionSection<TSection, TItem>> items;
        public IList<CollectionSection<TSection, TItem>> Items
        {
            get => Items;
            set => setItems(value ?? new List<CollectionSection<TSection, TItem>>());
        }

        public IObservable<TItem> ItemTapObservable => itemTapSubject.AsObservable();

        public override int ItemCount => currentItems.Count;

        protected BaseSectionedRecyclerAdapter()
        {
            HasStableIds = true;
        }

        protected BaseSectionedRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int GetItemViewType(int position)
            => currentItems[position].IsLeft ? SectionViewType : ItemViewType;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(parent.Context);
            if (viewType == ItemViewType)
            {
                var viewHolder = CreateItemViewHolder(inflater, parent);
                viewHolder.TappedSubject = itemTapSubject;
                return viewHolder;
            }

            return CreateHeaderViewHolder(inflater, parent);
        }

        protected abstract TSectionViewHolder CreateHeaderViewHolder(LayoutInflater inflater, ViewGroup parent);

        protected abstract TItemViewHolder CreateItemViewHolder(LayoutInflater inflater, ViewGroup parent);

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            switch (holder)
            {
                case TItemViewHolder itemViewHolder:
                    itemViewHolder.Item = currentItems[position].Right;
                    break;
                case TSectionViewHolder sectionViewHolder:
                    sectionViewHolder.Item = currentItems[position].Left;
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected view holder of type {holder.GetType().Name}");
            }
        }

        private void setItems(IList<CollectionSection<TSection, TItem>> newItems)
        {
            lock (updateLock)
            {
                var flatNewItems = flattenItems(newItems);
                if (!isUpdateRunning)
                {
                    isUpdateRunning = true;
                    processUpdate(flatNewItems);
                }
                else
                {
                    nextUpdate = flatNewItems;
                }
            }
        }

        private IList<Either<TSection, TItem>> flattenItems(IList<CollectionSection<TSection, TItem>> newItems)
        {
            return newItems.Aggregate(new List<Either<TSection, TItem>>(), (flatten, section) =>
                {
                    flatten.Add(Either<TSection, TItem>.WithLeft(section.Header));
                    section
                        .Items
                        .ToList()
                        .ForEach(item => flatten.Add(Either<TSection, TItem>.WithRight(item)));
                    return flatten;
                })
                .ToList();
        }

        private void processUpdate(IList<Either<TSection, TItem>> newItems)
        {
            var oldItems = currentItems;
            var handler = new Handler();
            Task.Run(() =>
            {
                var diffResult = DiffUtil.CalculateDiff(new BaseSectionedDiffCallBack(oldItems, newItems));
                handler.Post(() =>
                {
                    dispatchUpdates(newItems, diffResult);
                });
            });
        }

        private void dispatchUpdates(IList<Either<TSection, TItem>> newItems, DiffUtil.DiffResult diffResult)
        {
            currentItems = newItems;
            diffResult.DispatchUpdatesTo(this);
            lock (updateLock)
            {
                if (nextUpdate != null)
                {
                    processUpdate(nextUpdate);
                    nextUpdate = null;
                }
                else
                {
                    isUpdateRunning = false;
                }
            }
        }

        private sealed class BaseSectionedDiffCallBack : DiffUtil.Callback
        {
            private IList<Either<TSection, TItem>> oldItems;
            private IList<Either<TSection, TItem>> newItems;

            public BaseSectionedDiffCallBack(IList<Either<TSection, TItem>> oldItems, IList<Either<TSection, TItem>> newItems)
            {
                this.oldItems = oldItems;
                this.newItems = newItems;
            }

            public override bool AreContentsTheSame(int oldItemPosition, int newItemPosition)
            {
                var oldItem = oldItems[oldItemPosition];
                var newItem = newItems[newItemPosition];

                if (oldItem.IsLeft && newItem.IsLeft)
                    return oldItem.Left.Equals(newItem.Left);
                if (oldItem.IsRight && newItem.IsRight)
                    return oldItem.Right.Equals(newItem.Right);
                return false;
            }

            public override bool AreItemsTheSame(int oldItemPosition, int newItemPosition)
            {
                var oldItem = oldItems[oldItemPosition];
                var newItem = newItems[newItemPosition];

                if (oldItem.IsLeft && newItem.IsLeft)
                    return oldItem.Left.Identifier == newItem.Left.Identifier;
                if (oldItem.IsRight && newItem.IsRight)
                    return oldItem.Right.Identifier == newItem.Right.Identifier;
                return false;
            }

            public override int NewListSize => newItems.Count;
            public override int OldListSize => oldItems.Count;
        }
    }
}
