using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using BlurView.Droid.Extensions;
using BlurView.Extensions;
using EightBitLab.Com.BlurView;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(BlurView.BlurView), typeof(EightBitLab.Com.BlurViewLibrary.BlurViewRenderer))]
namespace EightBitLab.Com.BlurViewLibrary
{
    public class BlurViewRenderer : ViewRenderer
    {
        public static readonly float UltraThinScaleFactor = 5;
        public static readonly float ThinScaleFactor = 10;
        public static readonly float SystemScaleFactor = 15;
        public static readonly float ThickScaleFactor = 20;
        public static readonly float ChromeScaleFactor = 15;

        private static readonly Color UltraThinLight = Color.White.WithAlpha(0.50);
        private static readonly Color UltraThinDark = Color.DarkGray.WithAlpha(0.50);
        private static readonly Color ThinLight = Color.White.WithAlpha(0.633);
        private static readonly Color ThinDark = Color.DarkGray.WithAlpha(0.633);
        private static readonly Color SystemLight = Color.White.WithAlpha(0.766);
        private static readonly Color SystemDark = Color.DarkGray.WithAlpha(0.766);
        private static readonly Color ThickLight = Color.White.WithAlpha(0.90);
        private static readonly Color ThickDark = Color.DarkGray.WithAlpha(0.90);
        private static readonly Color ChromeLight = Color.White.WithAlpha(0.80);
        private static readonly Color ChromeDark = Color.DarkGray.WithAlpha(0.80);
        
        private DateTime t0 = DateTime.Now;
        private List<double> samples = new List<double>();
        private static readonly string TAG = typeof(BlurViewRenderer).Name;

        private IBlurController _blurController = new NoOpController();
        private IDisposable? _themeChangedDisposable;
        
        private global::BlurView.BlurView.Materials Material => (Element as global::BlurView.BlurView)?.Material ?? global::BlurView.BlurView.Materials.System;

        public float BlurRadius => 25;

        public float ScaleFactor => Material.GetMaterialThickness() switch
        {
            global::BlurView.BlurView.MaterialThicknesses.UltraThin => UltraThinScaleFactor,
            global::BlurView.BlurView.MaterialThicknesses.Thin => ThinScaleFactor,
            global::BlurView.BlurView.MaterialThicknesses.System => SystemScaleFactor,
            global::BlurView.BlurView.MaterialThicknesses.Thick => ThickScaleFactor,
            global::BlurView.BlurView.MaterialThicknesses.Chrome => ChromeScaleFactor,
            _ => 15
        };
        
        public Color OverlayColor => (Material.GetMaterialThickness(), Material.IsDark()) switch
        {
            (global::BlurView.BlurView.MaterialThicknesses.UltraThin, true) => UltraThinDark,  
            (global::BlurView.BlurView.MaterialThicknesses.UltraThin, false) => UltraThinLight,  
            (global::BlurView.BlurView.MaterialThicknesses.Thin, true) => ThinDark,
            (global::BlurView.BlurView.MaterialThicknesses.Thin, false) => ThinLight,
            (global::BlurView.BlurView.MaterialThicknesses.System, true) => SystemDark,
            (global::BlurView.BlurView.MaterialThicknesses.System, false) => SystemLight,
            (global::BlurView.BlurView.MaterialThicknesses.Thick, true) => ThickDark,
            (global::BlurView.BlurView.MaterialThicknesses.Thick, false) => ThickLight,
            (global::BlurView.BlurView.MaterialThicknesses.Chrome, true) => ChromeDark,
            (global::BlurView.BlurView.MaterialThicknesses.Chrome, false) => ChromeLight,
            (_, true) => SystemDark,
            (_, false) => SystemLight,
        };
        
        protected override void OnDraw(Canvas? canvas)
        {
            var t1 = DateTime.Now;
            samples.Add((t1 - t0).TotalSeconds);
            t0 = t1;
            Log.Debug("BlurView", "{0}: rate = {1:F0} Hz, location = {2}", ContentDescription,
                samples.Count / samples.Sum(), $"({this.Left}, {this.Top}, {this.Right}, {this.Bottom})");
            if (samples.Count > 5) samples.RemoveAt(0);

            base.OnDraw(canvas);
        }

        public BlurViewRenderer(Context context) : base(context) { }

        public override void Draw(Canvas canvas)
        {
            if (!_blurController.Draw(canvas)) return;
            base.Draw(canvas);
        }
        
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (!string.Equals(e.PropertyName, global::BlurView.BlurView.MaterialProperty.PropertyName)) return;
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
           
