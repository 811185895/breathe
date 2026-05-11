namespace BreatheWidget.Core;

public sealed class WorkStateEvaluator
{
    private static readonly TimeSpan FocusedDwell = TimeSpan.FromMinutes(7.5);
    private static readonly TimeSpan DeepFocusDwell = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan InactiveBreak = TimeSpan.FromMinutes(3);

    public WorkState Evaluate(ActivitySnapshot snapshot)
    {
        if (!snapshot.IsAvailable || snapshot.IdleDuration >= InactiveBreak)
        {
            return WorkState.Light;
        }

        if (HasStrongKeyboardOutput(snapshot))
        {
            return WorkState.Light;
        }

        if (IsSwitchingWithoutOutput(snapshot))
        {
            return WorkState.DeepFocus;
        }

        if (snapshot.WindowDwell < FocusedDwell)
        {
            return WorkState.Light;
        }

        if (snapshot.WindowDwell >= DeepFocusDwell && HasLowKeyboardOutput(snapshot))
        {
            return WorkState.DeepFocus;
        }

        if (HasLowKeyboardOutput(snapshot) && HasMouseActivity(snapshot))
        {
            return WorkState.Focused;
        }

        return WorkState.Light;
    }

    private static bool HasStrongKeyboardOutput(ActivitySnapshot snapshot)
    {
        return KeyboardInputsPerMinute(snapshot) >= 40;
    }

    private static bool HasLowKeyboardOutput(ActivitySnapshot snapshot)
    {
        return KeyboardInputsPerMinute(snapshot) <= 6;
    }

    private static bool IsSwitchingWithoutOutput(ActivitySnapshot snapshot)
    {
        return snapshot.WindowSwitches >= 5 && HasLowKeyboardOutput(snapshot);
    }

    private static bool HasMouseActivity(ActivitySnapshot snapshot)
    {
        return snapshot.MouseTravel >= 120;
    }

    private static double KeyboardInputsPerMinute(ActivitySnapshot snapshot)
    {
        if (snapshot.SampleDuration <= TimeSpan.Zero)
        {
            return snapshot.KeyboardInputs;
        }

        return snapshot.KeyboardInputs / snapshot.SampleDuration.TotalMinutes;
    }
}
