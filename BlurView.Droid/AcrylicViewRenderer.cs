#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using BlurView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(BlurView.AcrylicView), typeof(AcrylicViewRenderer))]
namespace BlurView.Droid
{
    public class AcrylicViewRenderer : ViewRenderer, ViewTreeObserver.IOnPreDrawListener
    {
        private BlurController? _blurController;

        private Color BackgroundColor => ((Element as AcrylicView)?.BackgroundColor ?? AcrylicView.DefaultBackgroundColor).ToAndroid();
        
        private float BlurRadius => (float)((Element as AcrylicView)?.BlurRadius ?? AcrylicView.DefaultBlurRadius);
        
        private float Elevation => (float)((Element as AcrylicView)?.Elevation ?? AcrylicView.DefaultElevation);

        public AcrylicViewRenderer(IntPtr javaReference, JniHandleOwnership transfer) : base(Xamarin.Essentials.Platform.AppContext) { }

        public AcrylicViewRenderer(Context context) : base(context) { }
        
        public bool OnPreDraw()
        {
            _blurController?.OnPreDraw();
            return true;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if (_blurController is not null && Element is not null && Control is not null)
            {
                _blurController.BackgroundColor = BackgroundColor;
                _blurController.BlurRadius = BlurRadius;
                _blurController.Elevation = Elevation;
            }
        }

        protected override void OnDraw(Canvas? canvas)
        {
            if (Visibility is not ViewStates.Visible)
                throw new Exception();
            
            base.OnDraw(canvas);
        }

        public override void Draw(Canvas canvas)
        {
            try
            {
                ViewTreeObserver.RemoveOnPreDrawListener(this);

                if (!(_blurController?.Draw(canvas) ?? false)) return;
                base.Draw(canvas);
                SetBackgroundColor(Color.Transparent);
            }
            finally
            {
                ViewTreeObserver.AddOnPreDrawListener(this);
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            var currentPage = Application.Current?.MainPage.Navigation.ModalStack.LastOrDefault() ??
                              Application.Current?.MainPage.Navigation.NavigationStack.LastOrDefault() ??
                              Application.Current?.MainPage ??
                              Shell.Current.CurrentPage;

            _blurController = new BlurController(this, currentPage?.GetRenderer()?.View)
            {
                BackgroundColor = BackgroundColor,
                BlurRadius = BlurRadius,
                Elevation = Elevation,
            };
            
            ViewTreeObserver.AddOnPreDrawListener(this);
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            
            ViewTreeObserver.RemoveOnPreDrawListener(this);
            
            try { _blurController?.Dispose(); } catch { /* do nothing */ }
            _blurController = null;
        }
        
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (string.Equals(e.PropertyName, AcrylicView.BackgroundColorProperty.PropertyName) ||
                string.Equals(e.PropertyName, AcrylicView.BlurRadiusProperty.PropertyName) ||
                string.Equals(e.PropertyName, AcrylicView.ElevationProperty.PropertyName))
            {
                _blurController.BackgroundColor = BackgroundColor;
                _blurController.BlurRadius = BlurRadius;
                _blurController.Elevation = Elevation;
            }
        }
    }
}