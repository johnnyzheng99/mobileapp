using Android.Graphics;
using Android.Views;

namespace Toggl.Giskard.Extensions
{
    public static class MotionEventExtensions
    {
        public static PointF ToPointF(this MotionEvent motionEvent)
            => new PointF(motionEvent.GetX(), motionEvent.GetY());
    }
}
