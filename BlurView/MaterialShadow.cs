using System;

namespace BlurView;

public class MaterialShadow
{
    private static readonly double[] _elevation = { 1, 2, 3, 4, 6, 8, 9, 12, 16, 24 };
    
    private static readonly double _opacityPenumbra = 0.26;
    private static readonly double _dxPenumbra = 0;
    private static readonly double[] _dyPenumbra = { 2, 3, 4, 4, 1, 3, 3, 5, 6, 9 };
    private static readonly double[] _radiusPenumbra = { 2, 4, 5, 5, 18, 16, 16, 22, 30, 46 };
    private static readonly double[] _spreadPenumbra = { 0, 0, 0, 0, 0, 2, 2, 4, 5, 8 };

    public static (double opacity, double dx, double dy, double radius, double spread) GetPenumbra(double elevation)
    {
        var index = Array.BinarySearch(_elevation, elevation);
        
        if (index >= 0)
            return (_opacityPenumbra, _dxPenumbra, _dyPenumbra[index], _radiusPenumbra[index], _spreadPenumbra[index]);
            
        index = ~index;
        index = index == _elevation.Length ? index - 1 : index;
        
        var dy = LookupLerp(elevation, index, _elevation, _dyPenumbra);
        var radius = LookupLerp(elevation, index, _elevation, _radiusPenumbra);
        var spread = LookupLerp(elevation, index, _elevation, _spreadPenumbra);
                
        return (_opacityPenumbra, _dxPenumbra, dy, radius, spread);
    }
    
    private static readonly double _opacityAmbient = 0.08;
    private static readonly double _dxAmbient = 0;
    private static readonly double[] _dyAmbient = { 1, 1, 1, 1, 3, 4, 5, 7, 8, 11 };
    private static readonly double[] _radiusAmbient = { 3, 5, 8, 10, 5, 15, 6, 8, 10, 15 };
    private static readonly double _spreadAmbient = 0;
    
    public static (double opacity, double dx, double dy, double radius, double spread) GetAmbient(double elevation)
    {
        var index = Array.BinarySearch(_elevation, elevation);
        
        if (index >= 0)
            return (_opacityAmbient, _dxAmbient, _dyAmbient[index], _radiusAmbient[index], _spreadAmbient);
        
        index = ~index;
        index = index == _elevation.Length ? index - 1 : index;

        var dy = LookupLerp(elevation, index, _elevation, _dyAmbient);
        var radius = LookupLerp(elevation, index, _elevation, _radiusAmbient);
        
        return (_opacityAmbient, _dxAmbient, dy, radius, _spreadAmbient);
    }

    private static double LookupLerp(double x, int index, double[] xs, double[] ys)
    {
        var (x0, y0, x1, y1) = index switch
        {
            0 => (xs[index], ys[index], xs[index + 1], ys[index + 1]),
            _ => (xs[index - 1], ys[index - 1], xs[index], ys[index])
        };
            
        return Lerp(x, x0, y0, x1, y1);
    }

    private static double Lerp(double x, double x0, double y0, double x1, double y1)
    {
        var dx = x1 - x0;
        dx = dx == 0 ? double.Epsilon : dx;
        return (((y1 - y0) / dx) * (x - x0)) + y1;
    }
}