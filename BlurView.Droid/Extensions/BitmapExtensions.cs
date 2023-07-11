using Android.Graphics;
using BlurView.Extensions;

namespace BlurView.Droid.Extensions
{
    public static class BitmapExtensions
    {
        public static void TryCleanup(this Bitmap? bitmap)
        {
            try { bitmap?.Recycle(); } catch { /* ignored */ }
            bitmap.TryDispose();
        }
    }
}