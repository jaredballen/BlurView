using System;
using Xamarin.Forms;

namespace BlurView
{
    public class BlurView : ContentView
    {
        public const double DefaultBlurRadius = 8;
        public static readonly Color DefaultBackgroundColor = new Color(0x00, 0x00, 0x00, 0x73);

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