using System;
using System.Collections;
using System.Collections.Generic;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Interfaces;
using Toggl.Foundation.MvvmCross.Reactive;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.ViewHolders;

namespace Toggl.Giskard.Extensions.Reactive
{
    public static class RecyclerAdapterExtensions
    {
        public static Action<IList<T>> Items<T>(this IReactive<BaseRecyclerAdapter<T>> reactive) where T : IDiffable<T>
            => collection => reactive.Base.Items = collection;

        public static Action<IList<CollectionSection<TSection, TItem>>> Items<TSection, TItem, TSectionViewHolder, TItemViewHolder>
        (this IReactive<BaseSectionedRecyclerAdapter<TSection, TItem, TSectionViewHolder, TItemViewHolder>> reactive)
        where TSection : IDiffable<TSection>
        where TItem : IDiffable<TItem>
        where TSectionViewHolder : BaseRecyclerViewHolder<TSection>
        where TItemViewHolder : BaseRecyclerViewHolder<TItem>
            => collection => reactive.Base.Items = collection;
    }
}
