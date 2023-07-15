using Android.Graphics;

namespace EightBitLab.Com.BlurView
{
    internal class NoOpController : IBlurController
    {
        public float BlurRadius { get; set; }
        public float ScaleFactor { get; set; }
        public Color OverlayColor { get; set; }
        public void Resize() { /* nothing to do */ }
        public bool Draw(Canvas canvas) => true;
        public void Dispose() { /* nothing to do */ }
    }
}