using System;
using System.ComponentModel;
using System.Reactive.Linq;
using Android.Content;
using Android.Graphics;
using BlurView.Droid.Extensions;
using BlurView.Extensions;
using EightBitLab.Com.BlurView;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(BlurView.Views.BlurView), typeof(EightBitLab.Com.BlurViewLibrary.BlurViewRenderer))]
namespace EightBitLab.Com.BlurViewLibrary
{
    public class BlurViewRenderer : ViewRenderer
    {
        private const float UltraThinBlurRadius = 11;
        private const float ThinBlurRadius = 12;
        private const float SystemBlurRadius = 15;
        private const float ThickBlurRadius = 25;
        private const float ChromeBlurRadius = 13;
        
        private const float UltraThinScaleFactor = 11;
        private const float ThinScaleFactor = 12;
        private const float SystemScaleFactor = 15;
        private const float ThickScaleFactor = 20;
        private const float ChromeScaleFactor = 13;

        private static readonly Color DarkColor = Color.Black;
        
        private static readonly Color UltraThinLight = Color.White.WithAlpha(0.30);
        private static readonly Color UltraThinDark = DarkColor.WithAlpha(0.30);
        private static readonly Color ThinLight = Color.White.WithAlpha(0.60);
        private static readonly Color ThinDark = DarkColor.WithAlpha(0.60);
        private static readonly Color SystemLight = Color.White.WithAlpha(0.75);
        private static readonly Color SystemDark = DarkColor.WithAlpha(0.73);
        private static readonly Color ThickLight = Color.White.WithAlpha(0.90);
        private static readonly Color ThickDark = DarkColor.WithAlpha(0.90);
        private static readonly Color ChromeLight = Color.White.WithAlpha(0.80);
        private static readonly Color ChromeDark = DarkColor.WithAlpha(0.70);
        
        private IBlurController _blurController = new NoOpController();
        private IDisposable? _themeChangedDisposable;
        
        private global::BlurView.Views.BlurView.Materials Material => (Element as global::BlurView.Views.BlurView)?.Material ?? global::BlurView.Views.BlurView.Materials.System;

        public float BlurRadius => Material.GetMaterialThickness() switch
        {
            global::BlurView.Views.BlurView.MaterialThicknesses.UltraThin => UltraThinBlurRadius,
            global::BlurView.Views.BlurView.MaterialThicknesses.Thin => ThinBlurRadius,
            global::BlurView.Views.BlurView.MaterialThicknesses.System => SystemBlurRadius,
            global::BlurView.Views.BlurView.MaterialThicknesses.Thick => ThickBlurRadius,
            global::BlurView.Views.BlurView.MaterialThicknesses.Chrome => ChromeBlurRadius,
            _ => SystemBlurRadius
        };

        public float ScaleFactor => Material.GetMaterialThickness() switch
        {
            global::BlurView.Views.BlurView.MaterialThicknesses.UltraThin => UltraThinScaleFactor,
            global::BlurView.Views.BlurView.MaterialThicknesses.Thin => ThinScaleFactor,
            global::BlurView.Views.BlurView.MaterialThicknesses.System => SystemScaleFactor,
            global::BlurView.Views.BlurView.MaterialThicknesses.Thick => ThickScaleFactor,
            global::BlurView.Views.BlurView.MaterialThicknesses.Chrome => ChromeScaleFactor,
            _ => SystemScaleFactor
        };
        
        public Color OverlayColor => (Material.GetMaterialThickness(), Material.IsDark()) switch
        {
            (global::BlurView.Views.BlurView.MaterialThicknesses.UltraThin, true) => UltraThinDark,  
            (global::BlurView.Views.BlurView.MaterialThicknesses.UltraThin, false) => UltraThinLight,  
            (global::BlurView.Views.BlurView.MaterialThicknesses.Thin, true) => ThinDark,
            (global::BlurView.Views.BlurView.MaterialThicknesses.Thin, false) => ThinLight,
            (global::BlurView.Views.BlurView.MaterialThicknesses.System, true) => SystemDark,
            (global::BlurView.Views.BlurView.MaterialThicknesses.System, false) => SystemLight,
            (global::BlurView.Views.BlurView.MaterialThicknesses.Thick, true) => ThickDark,
            (global::BlurView.Views.BlurView.MaterialThicknesses.Thick, false) => ThickLight,
            (global::BlurView.Views.BlurView.MaterialThicknesses.Chrome, true) => ChromeDark,
            (global::BlurView.Views.BlurView.MaterialThicknesses.Chrome, false) => ChromeLight,
            (_, true) => SystemDark,
            (_, false) => SystemLight,
        };

        public BlurViewRenderer(Context context) : base(context) { }

        public override void Draw(Canvas canvas)
        {
            if (!_blurController.Draw(canvas)) return;
            base.Draw(canvas);
        }
        
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (!string.Equals(e.PropertyName, global::BlurView.Views.BlurView.MaterialProperty.PropertyName)) return;
            _blurController.BlurRadius = BlurRadius;
            _blurController.ScaleFactor = ScaleFactor;
            _blurController.OverlayColor = OverlayColor;
        }
        
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            _blurController.Resize();
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            _blurController.TryDispose();
            _blurController = IsHardwareAccelerated
                ? new PreDrawBlurController(this)
                : new NoOpController();

            _themeChangedDisposable?.TryDispose();
            _themeChangedDisposable = Observable.FromEventPattern<AppThemeChangedEventArgs>(
                    h => Application.Current.RequestedThemeChanged += h,
                    h => Application.Current.RequestedThemeChanged -= h)
                .Subscribe(_ => _blurController.OverlayColor = OverlayColor);
            
            _blurController.BlurRadius = BlurRadius;
            _blurController.ScaleFactor = ScaleFactor;
            _blurController.OverlayColor = OverlayColor;
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            _themeChangedDisposable?.TryDispose();
            _blurController.TryDispose();
        }
    }
}
           
