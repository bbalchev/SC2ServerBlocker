using System.Windows.Media;

namespace SC2ServerBlocker
{
    internal static class UIColors
    {
        public static readonly Color StatusError = Color.FromRgb(153, 0, 0);
        public static readonly Color StatusNormal = Color.FromRgb(34, 34, 34);

        public static readonly Color BannerErrorBackground = Color.FromRgb(255, 224, 224);
        public static readonly Color BannerErrorBorder = Color.FromRgb(204, 0, 0);
        public static readonly Color BannerErrorForeground = Color.FromRgb(153, 0, 0);

        public static readonly Color BannerWarningBackground = Color.FromRgb(255, 244, 214);
        public static readonly Color BannerWarningBorder = Color.FromRgb(204, 153, 0);
        public static readonly Color BannerWarningForeground = Color.FromRgb(102, 76, 0);
    }
}
