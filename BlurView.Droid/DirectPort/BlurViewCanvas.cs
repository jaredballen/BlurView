using Android.Graphics;
using Android.Views;
using AndroidX.Annotations;

namespace EightBitLab.Com.BlurView
{
    public class BlurViewCanvas : Canvas
    {
        internal readonly BlurViewLibrary.BlurView _view;

        public BlurViewCanvas([NonNull] Bitmap bitmap, BlurViewLibrary.BlurView view) : base(bitmap)
        {
            _view = view;
        }
    }
}