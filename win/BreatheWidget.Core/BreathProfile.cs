namespace BreatheWidget.Core;

public sealed record BreathProfile(
    TimeSpan CycleDuration,
    double MinScale,
    double MaxScale,
    double MinOpacity,
    double MaxOpacity)
{
    public static BreathProfile Default { get; } = new(
        CycleDuration: TimeSpan.FromSeconds(6),
        MinScale: 1.0,
        MaxScale: 1.18,
        MinOpacity: 0.16,
        MaxOpacity: 0.34);

    public static BreathProfile Subtle { get; } = new(
        CycleDuration: TimeSpan.FromSeconds(6),
        MinScale: 1.0,
        MaxScale: 1.08,
        MinOpacity: 0.04,
        MaxOpacity: 0.10);
}
