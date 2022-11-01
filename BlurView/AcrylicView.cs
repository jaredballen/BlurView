using System;
using Xamarin.Forms;

namespace BlurView
{
    public class AcrylicView : ContentView
    {
        public const double DefaultBlurRadius = 8;
        public const double DefaultElevation = 2;
        public static readonly Color DefaultBackgroundColor = new Color(0x00, 0x00, 0x00, 0x73);

        #region BlurRadius Property
        public static readonly BindableProperty BlurRadiusProperty = BindableProperty.Create(
            propertyName: nameof(BlurRadius),
            returnType: typeof(double),
            declaringType: typeof(AcrylicView),
            defaultValue: DefaultBlurRadius,
            defaultBindingMode: BindingMode.OneWay,
            coerceValue: (_, value) => Math.Min(Math.Max(1, (double) value), 25));

        public double BlurRadius
        {
            get => ( double )GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }
        #endregion
        
        #region Elevation Property
        public static readonly BindableProperty ElevationProperty = BindableProperty.Create(
            propertyName: nameof(Elevation),
            returnType: typeof(double),
            declaringType: typeof(AcrylicView),
            defaultValue: DefaultElevation,
            defaultBindingMode: BindingMode.OneWay,
            coerceValue: (_, value) => (double) value < 0 ? 0 : value);

        public double Elevation
        {
            get => ( double )GetValue(ElevationProperty);
            set => SetValue(ElevationProperty, value);
        }
        #endregion

        public AcrylicView()
        {
            BackgroundColor = DefaultBackgroundColor;
        }
    }
}