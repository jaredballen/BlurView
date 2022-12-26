using System.Collections.Generic;
using Android.Views;
using FFImageLoading.Cache;
using Java.Lang;
using Xamarin.Forms;
using Rect = Android.Graphics.Rect;
using View = Android.Views.View;

namespace BlurView.Droid.Extensions;

public static class ViewExtensions
{
    public static bool Intersects(this View firstView, View secondView)
    {
        if (firstView.Visibility is not ViewStates.Visible || secondView.Visibility is not ViewStates.Visible)
            return false;
        
        var firstPosition = new int[2];
        var secondPosition = new int[2];

        firstView.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
        firstView.GetLocationOnScreen(firstPosition);
        
        secondView.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
        secondView.GetLocationOnScreen(secondPosition);

        var firstRect = new Rect(firstPosition[0], firstPosition[1], firstPosition[0] + firstView.Width, firstPosition[1] + firstView.Height);
        var secondRect = new Rect(secondPosition[0], secondPosition[1], secondPosition[0] + secondView.Width, secondPosition[1] + secondView.Height);
        
        var intersectsMethod = Rect.Intersects(firstRect, secondRect);

        var measuredIntersects = firstPosition[0] < secondPosition[0] + secondView.MeasuredWidth
               && firstPosition[0] + firstView.MeasuredWidth > secondPosition[0]
               && firstPosition[1] < secondPosition[1] + secondView.MeasuredHeight
               && firstPosition[1] + firstView.MeasuredHeight > secondPosition[1];
        
        var intersects = firstPosition[0] < secondPosition[0] + secondView.Width
               && firstPosition[0] + firstView.Width > secondPosition[0]
               && firstPosition[1] < secondPosition[1] + secondView.Height
               && firstPosition[1] + firstView.Height > secondPosition[1];

        return intersectsMethod;
    }
    
    public static bool Above(this Xamarin.Forms.View firstView, Xamarin.Forms.View secondView)
    {
        var parent1 = firstView.Parent;
        var parents1 = new Stack<Element>();
        while (parent1 is not null)
        {
            parents1.Push(parent1);
            parent1 = parent1.Parent;
        }
        var parent2 = secondView.Parent;
        var parents2 = new Stack<Element>();
        while (parent2 is not null)
        {
            parents2.Push(parent2);
            parent2 = parent2.Parent;
        }

        if (!parents1.TryPeek(out parent1) || !parents2.TryPeek(out parent2) || !ReferenceEquals(parent1, parent2))
            throw new Exception("View do NOT share root parent!?");
                
        while (parents1.TryPop(out parent1) && parents2.TryPop(out parent2))
        {
            parents1.TryPeek(out var peekParent1);
            parents2.TryPeek(out var peekParent2);

            peekParent1 ??= firstView;
            peekParent2 ??= secondView;
                    
            var index1 = parent1?.LogicalChildren.IndexOf(peekParent1) ?? int.MaxValue;
            var index2 = parent2?.LogicalChildren.IndexOf(peekParent2) ?? int.MaxValue;

            if (index1 > index2)
                return true;
        }

        return false;
    }
}