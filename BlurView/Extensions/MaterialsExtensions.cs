using Xamarin.Forms;

namespace BlurView.Extensions;

public static class MaterialsExtensions
{
    public static Views.BlurView.MaterialThicknesses GetMaterialThickness(this Views.BlurView.Materials material)
        => (Views.BlurView.MaterialThicknesses)((byte)material & 0xF0);

    public static Views.BlurView.MaterialColors GetMaterialColor(this Views.BlurView.Materials material)
        => (Views.BlurView.MaterialColors)((byte)material & 0x0F);

    public static bool IsDark(this Views.BlurView.Materials material)
        => (material.GetMaterialColor(), Application.Current.RequestedTheme) switch
        {
            (Views.BlurView.MaterialColors.Light, _) => false,
            (Views.BlurView.MaterialColors.Dark, _) => true,
            (Views.BlurView.MaterialColors.Dynamic, OSAppTheme.Dark) => true,
            (_, _) => false
        };
}