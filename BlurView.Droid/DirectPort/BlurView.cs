using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.Annotations;
using EightBitLab.Com.BlurView;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Color = Android.Graphics.Color;
using Resource = BlurView.Droid.Resource;

[assembly: ExportRenderer(typeof(BlurView.BlurView), typeof(EightBitLab.Com.BlurViewLibrary.BlurView))]
namespace EightBitLab.Com.BlurViewLibrary
{
    public class BlurView : ViewRenderer
    {
        private static readonly string TAG = typeof(BlurView).Name;

        private IBlurController blurController = new NoOpController();

        private int overlayColor;
        
        private float BlurRadius => (float)((Element as global::BlurView.BlurView)?.BlurRadius ?? global::BlurView.BlurView.DefaultBlurRadius);

        private double RedrawsPerSecond
        {
            get => (Element as global::BlurView.BlurView)?.RedrawsPerSecond ?? 0;
            set
            {
                if (Element is not global::BlurView.BlurView blurView) return;
                blurView.RedrawsPerSecond = value;
            }
        }
        
        public BlurView(Context context) : base(context)
        {
            Init(null, 0);
        }
        
        private void Init(IAttributeSet attrs, int defStyleAttr)
        {
            TypedArray a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.BlurView, defStyleAttr, 0);
            overlayColor = a.GetColor(Resource.Styleable.BlurView_blurOverlayColor, Color.Transparent);
            a.Recycle();
        }

        private DateTime t0 = DateTime.Now;
        private List<double> samples = new List<double>();
        
        public override void Draw(Canvas canvas)
        {
            bool shouldDraw = blurController.Draw(canvas);
            if (shouldDraw)
            {
                base.Draw(canvas);
            }
        }

        protected override void OnDraw(Canvas? canvas)
        {
            var t1 = DateTime.Now;
            samples.Add((t1 - t0).TotalSeconds);
            t0 = t1;
            Log.Debug("BlurView", "{0}: rate = {1:F0} Hz", ContentDescription, samples.Count / samples.Sum());
            if (samples.Count > 5) samples.RemoveAt(0);
            
            base.OnDraw(canvas);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            blurController.UpdateBlurViewSize();
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            blurController.SetBlurAutoUpdate(false);
        }
        
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            
            var currentPage = Application.Current?.MainPage.Navigation.ModalStack.LastOrDefault() ??
                              Application.Current?.MainPage.Navigation.NavigationStack.LastOrDefault() ??
                              Application.Current?.MainPage ??
                              Shell.Current.CurrentPage;

            this.SetupWith(currentPage?.GetRenderer()?.View as ViewGroup)
            .SetBlurRadius(BlurRadius);
                
            if (!IsHardwareAccelerated)
            {
                Log.Error(TAG, "BlurView can't be used in not hardware-accelerated window!");
            }
            else
            {
                blurController.SetBlurAutoUpdate(true);
            }
        }

        public IBlurViewFacade SetupWith([NonNull] ViewGroup rootView, IBlurAlgorithm algorithm)
        {
            this.blurController.Destroy();
            this.blurController = new PreDrawBlurController(this, rootView, overlayColor, algorithm);

            return blurController;
        }

        [RequiresApi(Value = (int)BuildVersionCodes.JellyBeanMr1)]
        public IBlurViewFacade SetupWith([NonNull] ViewGroup rootView)
        {
            return SetupWith(rootView, GetBlurAlgorithm());
        }

        public IBlurViewFacade SetBlurRadius(float radius)
        {
            return blurController.SetBlurRadius(radius);
        }

        public IBlurViewFacade SetOverlayColor(int overlayColor)
        {
            this.overlayColor = overlayColor;
            return blurController.SetOverlayColor(overlayColor);
        }

        public IBlurViewFacade SetBlurAutoUpdate(bool enabled)
        {
            return blurController.SetBlurAutoUpdate(enabled);
        }

        public IBlurViewFacade SetBlurEnabled(bool enabled)
        {
            return blurController.SetBlurEnabled(enabled);
        }

        [NonNull]
        [RequiresApi(Value = (int)BuildVersionCodes.JellyBeanMr1)]
        private IBlurAlgorithm GetBlurAlgorithm()
        {
            IBlurAlgorithm algorithm;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                algorithm = new RenderEffectBlur();
            }
            else
            {
                algorithm = new RenderScriptBlur(Context);
            }

            return algorithm;
        }
    }
}
           
