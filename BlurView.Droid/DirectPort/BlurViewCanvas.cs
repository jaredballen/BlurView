using Android.Graphics;

namespace BlurView.Droid.DirectPort;

public class BlurViewCanvas : Canvas
{
    internal EightBitLab.Com.BlurViewLibrary.BlurViewRenderer BlurViewRenderer { get; }
        
    public BlurViewCanvas(Bitmap bitmap, EightBitLab.Com.BlurViewLibrary.BlurViewRenderer blurViewRenderer)
        : base(bitmap) => BlurViewRenderer = blurViewRenderer;
}