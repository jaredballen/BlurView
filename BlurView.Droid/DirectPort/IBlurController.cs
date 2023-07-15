using System;
using Android.Graphics;

namespace EightBitLab.Com.BlurView
{
    public interface IBlurController : IDisposable
    {
        const float DefaultBlurRadius = 16f;
        const float DefaultScaleFactor = 1f;
        
        public float BlurRadius { get; set; }
        public float ScaleFactor { get; set; }
        public Color OverlayColor { get; set; }
        public void Resize();
        
        /**
         * Draws blurred content on given canvas
         *
         * @return true if BlurView should proceed with drawing itself and its children
         */
        bool Draw(Canvas canvas);
    }
}