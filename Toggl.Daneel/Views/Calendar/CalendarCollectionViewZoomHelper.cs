using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CoreGraphics;
using Foundation;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.Helper;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using UIKit;
using Math = System.Math;

namespace Toggl.Daneel.Views.Calendar
{
    public sealed class CalendarCollectionViewZoomHelper : CalendarCollectionViewAutoScrollHelper, IUIGestureRecognizerDelegate
    {
        private UIPinchGestureRecognizer pinchGestureRecognizer;
        private CalendarCollectionViewLayout layout;

        //private CGPoint firstPoint;

        public CalendarCollectionViewZoomHelper(
            UICollectionView collectionView,
            CalendarCollectionViewLayout layout) : base(collectionView, layout)
        {
            Ensure.Argument.IsNotNull(layout, nameof(layout));
            this.layout = layout;

            pinchGestureRecognizer = new UIPinchGestureRecognizer(onPinchUpdated);
            pinchGestureRecognizer.Delegate = this;
            collectionView.AddGestureRecognizer(pinchGestureRecognizer);
        }

        public CalendarCollectionViewZoomHelper(IntPtr handle) : base(handle)
        {
        }

        void onPinchUpdated(UIPinchGestureRecognizer gesture)
        {
            var pinchCenter = gesture.LocationInView(gesture.View);

            switch (gesture.State)
            {
                case UIGestureRecognizerState.Began:
                    layout.ScaleHourHeight(gesture.Scale, pinchCenter);
                    break;

                case UIGestureRecognizerState.Changed:
                    layout.ScaleHourHeight(gesture.Scale, pinchCenter);
                    gesture.Scale = 1;
                    break;

                case UIGestureRecognizerState.Ended:
                    layout.ScaleHourHeight(gesture.Scale, pinchCenter);
                    break;

                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    break;
            }
        }
    }
}
