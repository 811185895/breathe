namespace BreatheWidget.Core;

public sealed record AmbientColorSample(byte R, byte G, byte B)
{
    public double Brightness => ((0.2126 * R) + (0.7152 * G) + (0.0722 * B)) / 255.0;

    public double Saturation
    {
        get
        {
            var max = Math.Max(R, Math.Max(G, B)) / 255.0;
            var min = Math.Min(R, Math.Min(G, B)) / 255.0;

            return max <= 0 ? 0 : (max - min) / max;
        }
    }
}
