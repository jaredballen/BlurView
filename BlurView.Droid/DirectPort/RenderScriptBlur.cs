using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Renderscripts;
using AndroidX.Annotations;

namespace EightBitLab.Com.BlurView
{
    [Obsolete("RenderScript is deprecated and its hardware acceleration is not guaranteed. RenderEffectBlur is the best alternative at the moment.")]
    internal class RenderScriptBlur : IBlurAlgorithm
    {
        private Paint paint = new Paint(PaintFlags.FilterBitmap);
        private RenderScript renderScript;
        private ScriptIntrinsicBlur blurScript;
        private Allocation outAllocation;

        private int lastBitmapWidth = -1;
        private int lastBitmapHeight = -1;

        [RequiresApi(Value = (int)BuildVersionCodes.JellyBeanMr1)]
        public RenderScriptBlur(Context context)
        {
            renderScript = RenderScript.Create(context);
            blurScript = ScriptIntrinsicBlur.Create(renderScript, Element.U8_4(renderScript));
        }

        private bool CanReuseAllocation(Bitmap bitmap)
        {
            return bitmap.Height == lastBitmapHeight && bitmap.Width == lastBitmapWidth;
        }

        [RequiresApi(Value = (int)BuildVersionCodes.JellyBeanMr1)]
        public Bitmap Blur(Bitmap bitmap, float blurRadius)
        {
            Allocation inAllocation = Allocation.CreateFromBitmap(renderScript, bitmap);

            if (!CanReuseAllocation(bitmap))
            {
                outAllocation?.Destroy();
                outAllocation = Allocation.CreateTyped(renderScript, inAllocation.Type);
                lastBitmapWidth = bitmap.Width;
                lastBitmapHeight = bitmap.Height;
            }

            blurScript.SetRadius(blurRadius);
            blurScript.SetInput(inAllocation);
            // Do not use inAllocation in ForEach. It will cause visual artifacts on the blurred Bitmap
            blurScript.ForEach(outAllocation);
            outAllocation.CopyTo(bitmap);

            inAllocation.Destroy();
            return bitmap;
        }

        public void Destroy()
        {
            blurScript.Destroy();
            renderScript.Destroy();
            outAllocation?.Destroy();
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
            return IBlurController.DEFAULT_SCALE_FACTOR;
        }

        public void Render(Canvas canvas, Bitmap bitmap)
        {
            canvas.DrawBitmap(bitmap, 0f, 0f, paint);
        }
    }
}
