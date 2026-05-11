namespace BreatheWidget.Core;

public sealed class WorkStateMapper
{
    public BreathProfile GetProfile(WorkState state)
    {
        return state switch
        {
            WorkState.Light => BreathProfile.Default,
            WorkState.Focused => BreathProfile.Default with
            {
                MaxOpacity = 0.42,
                MaxScale = 1.20,
                CycleDuration = TimeSpan.FromSeconds(7)
            },
            WorkState.DeepFocus => BreathProfile.Default with
            {
                MaxOpacity = 0.52,
                MaxScale = 1.24,
                CycleDuration = TimeSpan.FromSeconds(9)
            },
            _ => BreathProfile.Default
        };
    }
}
