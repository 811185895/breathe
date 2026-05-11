namespace BreatheWidget.Core;

public sealed class WorkStateEvaluator
{
    private static readonly TimeSpan FocusedDwell = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan DeepFocusDwell = TimeSpan.FromMinutes(45);
    private static readonly TimeSpan InactiveBreak = TimeSpan.FromMinutes(3);

    public WorkState Evaluate(ActivitySnapshot snapshot)
    {
        if (!snapshot.IsAvailable || snapshot.IdleDuration >= InactiveBreak)
        {
            return WorkState.Light;
        }

        if (snapshot.WindowDwell < FocusedDwell)
        {
            return WorkState.Light;
        }

        if (snapshot.WindowSwitches >= 3)
        {
            return WorkState.Light;
        }

        if (snapshot.WindowDwell >= DeepFocusDwell &&
            snapshot.WindowSwitches == 0 &&
            IsSustainedKeyboardActivity(snapshot) &&
            IsLowMouseTravel(snapshot))
        {
            return WorkState.DeepFocus;
        }

        if (IsSustainedKeyboardActivity(snapshot) || snapshot.MouseTravel >= 300)
        {
            return WorkState.Focused;
        }

        return WorkState.Light;
    }

    private static bool IsSustainedKeyboardActivity(ActivitySnapshot snapshot)
    {
        if (snapshot.SampleDuration <= TimeSpan.Zero)
        {
            return snapshot.KeyboardInputs >= 120;
        }

        return snapshot.KeyboardInputs / snapshot.SampleDuration.TotalMinutes >= 40;
    }

    private static bool IsLowMouseTravel(ActivitySnapshot snapshot)
    {
        if (snapshot.SampleDuration <= TimeSpan.Zero)
        {
            return snapshot.MouseTravel <= 250;
        }

        return snapshot.MouseTravel / snapshot.SampleDuration.TotalMinutes <= 80;
    }
}
