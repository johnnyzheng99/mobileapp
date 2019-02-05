using Android.Graphics;
using MultivacPoint = Toggl.Multivac.Point;

namespace Toggl.Giskard.Extensions
{
    public static class PointExtensions
    {
        public static PointF ToPointF(this MultivacPoint point)
            => new PointF((float)point.X, (float)point.Y);

        public static MultivacPoint ToPoint(this PointF point)
            => new MultivacPoint { X = point.X, Y = point.Y };
    }
}
