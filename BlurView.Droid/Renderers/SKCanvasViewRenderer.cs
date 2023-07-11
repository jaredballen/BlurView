using Android.Content;
using Android.Graphics;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;

namespace BlurView.Droid.Renderers
{
    public class SKCanvasViewRenderer : SkiaSharp.Views.Forms.SKCanvasViewRenderer
    {
        private Bitmap? _bitmapBuffer;
        private Canvas? _canvasBuffer;
        
        public SKCanvasViewRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<SkiaSharp.Views.Forms.SKCanvasView> e)
        {
            base.OnElementChanged(e);
            InitializeBuffers(Control?.Width ?? 0, Control?.Height ?? 0);
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            InitializeBuffers(Control?.Width ?? 0, Control?.Height ?? 0);
        }


        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            InitializeBuffers(Control?.Width ?? 0, Control?.Height ?? 0);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            InitializeBuffers(w, h);
            base.OnSizeChanged(w, h, oldw, oldh);
        }

        protected override bool DrawChild(Canvas? canvas, View? child, long drawingTime)
        {
            if (_canvasBuffer is null || (canvas.Width == child.Width && canvas.Height == child.Height))
                return base.DrawChild(canvas, child, drawingTime);

            var invalidated = base.DrawChild(_canvasBuffer, child, drawingTime);
            canvas.DrawBitmap(_bitmapBuffer, 0, 0, null);
            return invalidated;
        }

        private void InitializeBuffers(int width, int height)
        {
            try { _bitmapBuffer?.Dispose(); } catch { /* ignored */ }
            _bitmapBuffer = null;
            
            try { _canvasBuffer?.Dispose(); } catch { /* ignored */ }
            _canvasBuffer = null;
            
            if (Element is null || Control is null || width <= 0 || height <= 0) return;
            
            _bitmapBuffer = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            _canvasBuffer = new Canvas(_bitmapBuffer);
        }
    }
}