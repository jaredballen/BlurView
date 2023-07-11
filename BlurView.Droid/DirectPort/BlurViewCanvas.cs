using Android.Graphics;

namespace BlurView.Droid.DirectPort;

public class BlurViewCanvas : Canvas
{
    internal EightBitLab.Com.BlurViewLibrary.BlurView BlurView { get; }
        
    public BlurViewCanvas(Bitmap bitmap, EightBitLab.Com.BlurViewLibrary.BlurView blurView)
        : base(bitmap) => BlurView = blurView;
}