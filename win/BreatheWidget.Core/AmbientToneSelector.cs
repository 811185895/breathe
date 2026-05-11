namespace BreatheWidget.Core;

public sealed class AmbientToneSelector
{
    public AmbientTone Select(AmbientColorSample sample)
    {
        if (sample.Brightness < 0.35)
        {
            return new AmbientTone(
                AmbientToneKind.Moonlight,
                Core: new AmbientColor(210, 226, 236, 248),
                Mid: new AmbientColor(108, 180, 204, 238),
                Edge: new AmbientColor(8, 210, 226, 248),
                MinOpacity: 0.14,
                MaxOpacity: 0.32);
        }

        if (sample.Saturation > 0.30 && sample.Brightness < 0.82)
        {
            var softened = SoftenTowardNeutral(sample, 0.64);

            return new AmbientTone(
                AmbientToneKind.TintedFog,
                Core: new AmbientColor(180, softened.R, softened.G, softened.B),
                Mid: new AmbientColor(82, softened.R, softened.G, softened.B),
                Edge: new AmbientColor(10, softened.R, softened.G, softened.B),
                MinOpacity: 0.12,
                MaxOpacity: 0.28);
        }

        return new AmbientTone(
            AmbientToneKind.CoolFog,
            Core: new AmbientColor(220, 92, 108, 128),
            Mid: new AmbientColor(118, 76, 92, 112),
            Edge: new AmbientColor(24, 64, 80, 100),
            MinOpacity: 0.24,
            MaxOpacity: 0.50);
    }

    private static AmbientColorSample SoftenTowardNeutral(AmbientColorSample sample, double amount)
    {
        const byte neutral = 164;

        return new AmbientColorSample(
            Blend(sample.R, neutral, amount),
            Blend(sample.G, neutral, amount),
            Blend(sample.B, neutral, amount));
    }

    private static byte Blend(byte source, byte target, double amount)
    {
        return (byte)Math.Clamp(Math.Round(source + ((target - source) * amount)), 0, 255);
    }
}
