using Android.Content;
using Android.Graphics;
using BlurView;
using BlurView.Droid.Renderers;
using Xamarin.Forms;
using Rect = Android.Graphics.Rect;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(SKCanvasView), typeof(SKCanvasViewRenderer))]
namespace BlurView.Droid.Renderers
{
    public class SKCanvasViewRenderer : SkiaSharp.Views.Forms.SKCanvasViewRenderer
    {
        public SKCanvasViewRenderer(Context context) : base(context)
        {
            ClipToOutline = false;
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
        }

        protected override bool DrawChild(Canvas? canvas, View? child, long drawingTime)
        {
            if (canvas is BlurViewCanvas blurViewCanvas && child is not null)
            {
                var blurViewCanvasLocation = new int[2];
                blurViewCanvas.GetLocationOnScreen(blurViewCanvasLocation);

                var childLocation = new int[2];
                child.GetLocationOnScreen(childLocation);

                var sx = (float)child.Width / canvas.Width;
                var sy = (float)child.Height / canvas.Height;
                
                var dx = (childLocation[0] - blurViewCanvasLocation[0]) * sx;
                var dy = (childLocation[1] - blurViewCanvasLocation[1]) * sy;
                
                canvas.Save();

                //canvas.ClipBounds = new Rect(0, 0, canvas.Width, canvas.Height);
                
                canvas.Matrix = new Matrix(Matrix.IdentityMatrix);
                canvas.Density = 0;
                                
                canvas.Scale(sx, sy);
                //canvas.Translate(dx, dy);
                
                var baseResult = base.DrawChild(canvas, child, drawingTime);
                
                canvas.Restore();

                return baseResult;
            }
            else
                return base.DrawChild(canvas, child, drawingTime);
        }
    }
}