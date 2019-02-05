using System;
using System.Reactive.Linq;
using CoreGraphics;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Reactive;

namespace Toggl.Daneel.Extensions.Reactive
{
    public static class BaseTableViewSourceExtensions
    {
        public static IObservable<TModel> ModelSelected<THeader, TModel>(
            this IReactive<BaseTableViewSource<THeader, TModel>> reactive)
            => Observable
                .FromEventPattern<TModel>(e => reactive.Base.OnItemTapped += e, e => reactive.Base.OnItemTapped -= e)
                .Select(e => e.EventArgs);

        public static IObservable<CGPoint> Scrolled<THeader, TModel>(
            this IReactive<BaseTableViewSource<THeader, TModel>> reactive)
            => Observable
                .FromEventPattern<CGPoint>(e => reactive.Base.OnScrolled += e, e => reactive.Base.OnScrolled -= e)
                .Select(e => e.EventArgs);
    }
}
