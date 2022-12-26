using Android.Graphics;

namespace BlurView.Droid;

internal class AcrylicCanvas : Canvas
{
    public AcrylicViewRenderer View { get; }
        
    internal AcrylicCanvas(AcrylicViewRenderer view, Bitmap bitmap) : base(bitmap)
    {
        View = view;
    }
}