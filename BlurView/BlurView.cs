using System;
using Xamarin.Forms;

namespace BlurView
{
    public class BlurView : ContentView
    {
        public const double DefaultBlurRadius = 8;
        public static readonly Color DefaultBackgroundColor = new Color(0x00, 0x00, 0x00, 0x73);
        
        #region RootView Property
        public static readonly BindableProperty RootViewProperty = BindableProperty.Create(
            propertyName: nameof(RootView),
            returnType: typeof(VisualElement),
            declaringType: typeof(BlurView),
            defaultValue: default(VisualElement),
            defaultBindingMode: BindingMode.OneWay);

        public VisualElement RootView
        {
            get => ( VisualElement )GetValue(RootViewProperty);
            set => SetValue(RootViewProperty, value);
        }
        #endregion

        #region BlurRadius Property
        public static readonly BindableProperty BlurRadiusProperty = BindableProperty.Create(
            propertyName: nameof(BlurRadius),
            returnType: typeof(double),
            declaringType: typeof(BlurView),
            defaultValue: DefaultBlurRadius,
            defaultBindingMode: BindingMode.OneWay,
            coerceValue: (_, value) => Math.Min(Math.Max(1, (double) value), 25));

        public double BlurRadius
        {
            get => ( double )GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }
        #endregion

        public BlurView()
        {
            BackgroundColor = DefaultBackgroundColor;
        }
    }
}