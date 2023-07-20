using System;
using UIKit;

namespace BlurView.iOS.Extensions;

public static class MaterialExtensions
{
    public static UIBlurEffectStyle ToNative(this BlurView.Views.BlurView.Materials material)
        => material switch
        {
            Views.BlurView.Materials.UltraThin => UIBlurEffectStyle.SystemUltraThinMaterial,
            Views.BlurView.Materials.UltraThinLight => UIBlurEffectStyle.SystemUltraThinMaterialLight,
            Views.BlurView.Materials.UltraThinDark => UIBlurEffectStyle.SystemUltraThinMaterialDark,
            Views.BlurView.Materials.Thin => UIBlurEffectStyle.SystemThinMaterial,
            Views.BlurView.Materials.ThinLight => UIBlurEffectStyle.SystemThinMaterialLight,
            Views.BlurView.Materials.ThinDark => UIBlurEffectStyle.SystemThinMaterialDark,
            Views.BlurView.Materials.System => UIBlurEffectStyle.SystemMaterial,
            Views.BlurView.Materials.SystemLight => UIBlurEffectStyle.SystemMaterialLight,
            Views.BlurView.Materials.SystemDark => UIBlurEffectStyle.SystemMaterialDark,
            Views.BlurView.Materials.Thick => UIBlurEffectStyle.SystemThickMaterial,
            Views.BlurView.Materials.ThickLight => UIBlurEffectStyle.SystemThickMaterialLight,
            Views.BlurView.Materials.ThickDark => UIBlurEffectStyle.SystemThickMaterialDark,
            Views.BlurView.Materials.Chrome => UIBlurEffectStyle.SystemChromeMaterial,
            Views.BlurView.Materials.ChromeLight => UIBlurEffectStyle.SystemChromeMaterialLight,
            Views.BlurView.Materials.ChromeDark => UIBlurEffectStyle.SystemChromeMaterialDark,
            _ => UIBlurEffectStyle.SystemMaterial
        };
}