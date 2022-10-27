using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using BlurView;
using BlurView.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(SKCanvasView), typeof(SKCanvasViewRenderer))]
namespace BlurView.Droid.Renderers
{
    public class SKCanvasViewRenderer : SkiaSharp.Views.Forms.SKCanvasViewRenderer
    {
        public SKCanvasViewRenderer(Context context) : base(context) { }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        protected override bool DrawChild(Canvas? canvas, View? child, long drawingTime)
        {
            if (canvas is BlurViewCanvas blurViewCanvas && child is not null)
            {
                using var vBitmap = Bitmap.CreateBitmap(child.Width, child.Height, Bitmap.Config.Argb8888);
                using var vCanvas = new Canvas(vBitmap);
                
                var baseResult = base.DrawChild(vCanvas, child, drawingTime);

                canvas.DrawBitmap(vBitmap, 0, 0, null);

                return baseResult;
            }
            else
                return base.DrawChild(canvas, child, drawingTime);
        }
    }
}