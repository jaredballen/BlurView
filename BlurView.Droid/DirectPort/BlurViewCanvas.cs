using Android.Graphics;
using AndroidX.Annotations;

namespace EightBitLab.Com.BlurView
{
    public class BlurViewCanvas : Canvas
    {
        public BlurViewCanvas([NonNull] Bitmap bitmap) : base(bitmap)
        {
        }
    }
}