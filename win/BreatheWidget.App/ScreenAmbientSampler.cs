using BreatheWidget.Core;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace BreatheWidget.App;

internal sealed class ScreenAmbientSampler : IDisposable
{
    private readonly int _sampleSize;
    private readonly Drawing.Bitmap _bitmap;

    public ScreenAmbientSampler(int sampleSize = 48)
    {
        _sampleSize = sampleSize;
        _bitmap = new Drawing.Bitmap(sampleSize, sampleSize);
    }

    public AmbientColorSample Sample(double screenX, double screenY)
    {
        var bounds = Forms.SystemInformation.VirtualScreen;
        var x = Clamp((int)Math.Round(screenX - (_sampleSize / 2.0)), bounds.Left, bounds.Right - _sampleSize);
        var y = Clamp((int)Math.Round(screenY - (_sampleSize / 2.0)), bounds.Top, bounds.Bottom - _sampleSize);

        using var graphics = Drawing.Graphics.FromImage(_bitmap);
        graphics.CopyFromScreen(x, y, 0, 0, new Drawing.Size(_sampleSize, _sampleSize), Drawing.CopyPixelOperation.SourceCopy);

        long red = 0;
        long green = 0;
        long blue = 0;
        var count = _sampleSize * _sampleSize;

        for (var px = 0; px < _sampleSize; px += 2)
        {
            for (var py = 0; py < _sampleSize; py += 2)
            {
                var color = _bitmap.GetPixel(px, py);
                red += color.R;
                green += color.G;
                blue += color.B;
            }
        }

        count /= 4;

        return new AmbientColorSample(
            (byte)(red / count),
            (byte)(green / count),
            (byte)(blue / count));
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }

    private static int Clamp(int value, int min, int max)
    {
        if (max < min)
        {
            return min;
        }

        return Math.Min(Math.Max(value, min), max);
    }
}
