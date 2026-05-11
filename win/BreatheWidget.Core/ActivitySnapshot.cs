namespace BreatheWidget.Core;

public sealed record ActivitySnapshot(
    WindowActivitySnapshot Window,
    TimeSpan WindowDwell,
    int KeyboardInputs,
    double MouseTravel,
    int WindowSwitches,
    TimeSpan IdleDuration,
    TimeSpan SampleDuration,
    bool IsAvailable);
