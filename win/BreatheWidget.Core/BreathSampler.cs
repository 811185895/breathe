namespace BreatheWidget.Core;

public sealed class BreathSampler
{
    private readonly BreathProfile _profile;

    public BreathSampler(BreathProfile profile)
    {
        if (profile.CycleDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(profile), "Cycle duration must be positive.");
        }

        _profile = profile;
    }

    public BreathSample Sample(TimeSpan elapsed)
    {
        var cycleSeconds = _profile.CycleDuration.TotalSeconds;
        var seconds = elapsed.TotalSeconds % cycleSeconds;

        if (seconds < 0)
        {
            seconds += cycleSeconds;
        }

        var phase = seconds / cycleSeconds;
        var amount = (1 - Math.Cos(phase * Math.Tau)) / 2;

        return new BreathSample(
            Scale: Lerp(_profile.MinScale, _profile.MaxScale, amount),
            Opacity: Lerp(_profile.MinOpacity, _profile.MaxOpacity, amount));
    }

    private static double Lerp(double start, double end, double amount)
    {
        return start + ((end - start) * amount);
    }
}
