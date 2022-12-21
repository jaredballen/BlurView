using Android.Views;

namespace BlurView.Droid.Extensions;

public static class ViewExtensions
{
    public static bool Overlaps(this View firstView, View secondView)
    {
        if (firstView.Visibility is not ViewStates.Visible || secondView.Visibility is not ViewStates.Visible)
            return false;
        
        var firstPosition = new int[2];
        var secondPosition = new int[2];

        firstView.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
        firstView.GetLocationOnScreen(firstPosition);
        
        secondView.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
        secondView.GetLocationOnScreen(secondPosition);

        return firstPosition[0] < secondPosition[0] + secondView.MeasuredWidth
               && firstPosition[0] + firstView.MeasuredWidth > secondPosition[0]
               && firstPosition[1] < secondPosition[1] + secondView.MeasuredHeight
               && firstPosition[1] + firstView.MeasuredHeight > secondPosition[1];
    }
}