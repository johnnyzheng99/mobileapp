using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;

namespace Toggl.Giskard.Extensions
{
    public static class ContextExtensions
    {
        public static VectorDrawable GetVectorDrawable(this Context context, int resourceId)
        {
            return ContextCompat.GetDrawable(context, resourceId) as VectorDrawable;
        }
    }
}
