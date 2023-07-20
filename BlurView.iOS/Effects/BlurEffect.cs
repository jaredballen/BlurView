using System;
using System.ComponentModel;
using BlurView.iOS.Effects;
using BlurView.iOS.Extensions;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportEffect (typeof(BlurEffect), nameof(BlurView.Effects.BlurEffect))]
namespace BlurView.iOS.Effects;

public class BlurEffect : PlatformEffect
{
    private UIBlurEffect? _blurEffect;
    private UIVisualEffectView? _blurEffectView;

    private Views.BlurView View => (Views.BlurView)Element;

    private UIView NativeView => Container ?? Control;
    
    protected override void OnAttached()
    {
        _blurEffect = UIBlurEffect.FromStyle(View.Material.ToNative());
        _blurEffectView = new UIVisualEffectView
        {
            Effect = _blurEffect,
            Frame = View.Bounds.Offset(-View.Bounds.X, -View.Bounds.Y).ToRectangleF()
        };

        if (NativeView.Subviews.Length > 0)
            NativeView.InsertSubview(_blurEffectView, 0);
        else
            NativeView.AddSubview(_blurEffectView);
    }

    protected override void OnDetached()
    {
        _blurEffectView?.RemoveFromSuperview();
        _blurEffectView?.Dispose();
        _blurEffect?.Dispose();
    }

    protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
    {
        base.OnElementPropertyChanged(args);

        if (_blurEffectView is null) return;
        
        if (string.Equals(args.PropertyName, VisualElement.WidthProperty.PropertyName) ||
            string.Equals(args.PropertyName, VisualElement.HeightProperty.PropertyName))
        {
            _blurEffectView.Frame = View.Bounds.Offset(-View.Bounds.X, -View.Bounds.Y).ToRectangleF();
            return;
        }

        if (string.Equals(args.PropertyName, Views.BlurView.MaterialProperty.PropertyName))
        {
            var blurEffect = UIBlurEffect.FromStyle(View.Material.ToNative());
            _blurEffectView.Effect = blurEffect;
            _blurEffect?.Dispose();
            _blurEffect = blurEffect;
            return;
        }
    }
}