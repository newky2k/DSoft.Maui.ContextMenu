using Android.Content.Res;

namespace The49.Maui.ContextMenu;

internal class ViewUtils
{
    public static int DpToPx(int dp)
    {
        return (int)Math.Round(dp * Resources.System.DisplayMetrics.Density);
    }
}
