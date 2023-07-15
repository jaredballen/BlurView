using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Annotations;

namespace EightBitLab.Com.BlurView
{
    [RequiresApi(Value = (int)BuildVersionCodes.S)]
    internal class RenderEffectBlur : IBlurAlgorithm
    {
        private RenderNode node = new RenderNode("BlurViewNode");
        private int height;
        private int width;
        private float lastBlurRadius = 1f;

        [Nullable]
        public IBlurAlgorithm fallbackAlgorithm;
        private Context _context;

        public RenderEffectBlur(Context context)
        {
            _context = context;
        }

        public Bitmap Blur(Bitmap bitmap, float blurRadius)
        {
            lastBlurRadius = blurRadius;

            if (bitmap.Height != height || bitmap.Width != width)
            {
                height = bitmap.Height;
                width = bitmap.Width;
                node.SetPosition(0, 0, width, height);
            }

            Canvas canvas = node.BeginRecording();
            canvas.DrawBitmap(bitmap, 0, 0, null);
            node.EndRecording();
            node.SetRenderEffect(RenderEffect.CreateBlurEffect(blurRadius, blurRadius, Shader.TileMode.Mirror));
            // Returning not blurred bitmap because the rendering relies on the RenderNode
            return bitmap;
        }

        public void Destroy()
        {
            node.DiscardDisplayList();
            fallbackAlgorithm?.Destroy();
        }

        public bool CanModifyBitmap()
        {
            return true;
        }

        public Bitmap.Config GetSupportedBitmapConfig()
        {
            return Bitmap.Config.Argb8888;
        }

        public float ScaleFactor()
        {
            return IBlurController.DefaultScaleFactor;
        }

        public void Render(Canvas canvas, Bitmap bitmap)
        {
            if (canvas.IsHardwareAccelerated)
            {
                canvas.DrawRenderNode(node);
            }
            else
            {
                if (fallbackAlgorithm == null)
                {
                    fallbackAlgorithm = new RenderScriptBlur(_context);
                }
                fallbackAlgorithm.Blur(bitmap, lastBlurRadius);
                fallbackAlgorithm.Render(canvas, bitmap);
            }
        }
    }
}
