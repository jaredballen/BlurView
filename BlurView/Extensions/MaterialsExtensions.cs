using Xamarin.Forms;

namespace BlurView.Extensions;

public static class MaterialsExtensions
{
    public static BlurView.MaterialThicknesses GetMaterialThickness(this BlurView.Materials material)
        => (BlurView.MaterialThicknesses)((byte)material & 0xF0b);
    
    public static BlurView.MaterialColors GetMaterialColor(this BlurView.Materials material)
        => (BlurView.MaterialColors)((byte)material & 0x0Fb);

    public static bool IsDark(this BlurView.Materials material)
        => (material.GetMaterialColor(), Application.Current.UserAppTheme) switch
        {
            (BlurView.MaterialColors.Light, _) => false,
            (BlurView.MaterialColors.Dark, _) => true,
            (BlurView.MaterialColors.Dynamic, OSAppTheme.Dark) => true,
            (_, _) => false
        };
}