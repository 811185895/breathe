using System.Drawing;
using System.Runtime.InteropServices;

namespace BreatheWidget.App;

internal static class ScreenMetrics
{
    private const uint MonitorDefaultToNearest = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(NativePoint pt, uint dwFlags);

    [DllImport("Shcore.dll", SetLastError = true)]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    /// <summary>
    /// Converts GDI pixel bounds to WPF device-independent units for the monitor that contains the bounds center.
    /// </summary>
    public static (double Left, double Top, double Width, double Height) BoundsToDIP(Rectangle bounds)
    {
        var dpiX = 96u;
        var dpiY = 96u;

        var center = new NativePoint
        {
            X = bounds.Left + bounds.Width / 2,
            Y = bounds.Top + bounds.Height / 2
        };

        var hMonitor = MonitorFromPoint(center, MonitorDefaultToNearest);
        if (hMonitor != IntPtr.Zero)
        {
            try
            {
                if (GetDpiForMonitor(hMonitor, 0, out dpiX, out dpiY) != 0 || dpiX == 0 || dpiY == 0)
                {
                    dpiX = dpiY = 96;
                }
            }
            catch (DllNotFoundException)
            {
                dpiX = dpiY = 96;
            }
        }

        var scaleX = 96.0 / dpiX;
        var scaleY = 96.0 / dpiY;

        return (
            bounds.Left * scaleX,
            bounds.Top * scaleY,
            bounds.Width * scaleX,
            bounds.Height * scaleY);
    }
}
