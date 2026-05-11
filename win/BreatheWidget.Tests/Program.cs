using BreatheWidget.Core;

var tests = new (string Name, Action Run)[]
{
    ("Default profile is visible enough to confirm startup", DefaultProfileIsVisibleEnoughToConfirmStartup),
    ("Breath sample reaches inhale peak halfway through cycle", BreathSampleReachesPeakHalfway),
    ("State mapper strengthens visual after long focus", StateMapperStrengthensAfterLongFocus),
    ("Adaptive tone uses classic glow on dark backgrounds", AdaptiveToneUsesClassicGlowOnDarkBackgrounds),
    ("Adaptive tone uses dawn glow on bright backgrounds", AdaptiveToneUsesDawnGlowOnBrightBackgrounds),
    ("Adaptive tone preserves classic neutral mode", AdaptiveTonePreservesClassicNeutralMode),
    ("Adaptive tone preserves manual moonlight mode", AdaptiveTonePreservesManualMoonlightMode),
    ("Adaptive tone follows colorful backgrounds with low saturation", AdaptiveToneFollowsColorfulBackgroundsWithLowSaturation),
    ("Adaptive tone stays visible on white backgrounds", AdaptiveToneStaysVisibleOnWhiteBackgrounds),
    ("Screen anchor resolves golden lower point", ScreenAnchorResolvesGoldenLowerPoint),
    ("Evaluator treats high keyboard output as light state", EvaluatorTreatsHighKeyboardOutputAsLight),
    ("Evaluator promotes low-output same-window dwell to focused state", EvaluatorPromotesLowOutputDwellToFocused),
    ("Evaluator promotes long low-output dwell to deep focus", EvaluatorPromotesLongLowOutputDwellToDeepFocus),
    ("Evaluator promotes frequent switching without output to deep focus", EvaluatorPromotesSwitchingWithoutOutputToDeepFocus),
    ("Evaluator treats inactivity as session break", EvaluatorTreatsInactivityAsSessionBreak),
    ("Prompt policy waits for sustained deep focus", PromptPolicyWaitsForSustainedDeepFocus),
    ("Prompt policy suppresses repeated prompt during cooldown", PromptPolicySuppressesRepeatedPromptDuringCooldown)
};

var failures = 0;

foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {test.Name}: {ex.Message}");
    }
}

if (failures > 0)
{
    Environment.Exit(1);
}

static void DefaultProfileIsVisibleEnoughToConfirmStartup()
{
    var profile = BreathProfile.Default;

    AssertEqual(TimeSpan.FromSeconds(6), profile.CycleDuration, "CycleDuration");
    AssertEqual(1.0, profile.MinScale, "MinScale");
    AssertEqual(1.18, profile.MaxScale, "MaxScale");
    AssertEqual(0.16, profile.MinOpacity, "MinOpacity");
    AssertEqual(0.34, profile.MaxOpacity, "MaxOpacity");
}

static void BreathSampleReachesPeakHalfway()
{
    var profile = BreathProfile.Default;
    var sampler = new BreathSampler(profile);

    var start = sampler.Sample(TimeSpan.Zero);
    var peak = sampler.Sample(TimeSpan.FromSeconds(3));
    var end = sampler.Sample(TimeSpan.FromSeconds(6));

    AssertClose(profile.MinScale, start.Scale, 0.0001, "start scale");
    AssertClose(profile.MaxScale, peak.Scale, 0.0001, "peak scale");
    AssertClose(profile.MinScale, end.Scale, 0.0001, "end scale");
    AssertClose(profile.MaxOpacity, peak.Opacity, 0.0001, "peak opacity");
}

static void StateMapperStrengthensAfterLongFocus()
{
    var mapper = new WorkStateMapper();

    var light = mapper.GetProfile(WorkState.Light);
    var deep = mapper.GetProfile(WorkState.DeepFocus);

    AssertTrue(deep.MaxOpacity > light.MaxOpacity, "deep focus should be more visible than light focus");
    AssertTrue(deep.CycleDuration > light.CycleDuration, "deep focus should breathe slower than light focus");
    AssertTrue(deep.MaxScale >= light.MaxScale, "deep focus should not shrink the visual peak");
}

static void AdaptiveToneUsesClassicGlowOnDarkBackgrounds()
{
    var selector = new AmbientToneSelector();

    var tone = selector.Select(new AmbientColorSample(18, 22, 28));

    AssertTrue(tone.Kind == AmbientToneKind.Moonlight, "dark backgrounds should use the original white-gray glow");
    AssertTrue(tone.Core.R >= 210 && tone.Core.G >= 220 && tone.Core.B >= 230, "dark backgrounds should stay in the original white-gray range");
    AssertTrue(tone.MaxOpacity >= 0.26, "dark backgrounds need enough visible glow");
}

static void AdaptiveToneUsesDawnGlowOnBrightBackgrounds()
{
    var selector = new AmbientToneSelector();

    var tone = selector.Select(new AmbientColorSample(242, 240, 236));

    AssertTrue(tone.Kind == AmbientToneKind.DawnGlow, "bright backgrounds should use dawn glow tone");
    AssertTrue(tone.Core.R > tone.Core.G && tone.Core.G > tone.Core.B, "dawn glow should lean warm gold");
    AssertTrue(tone.Core.G >= 170 && tone.Core.B >= 110, "dawn glow should stay soft instead of harsh yellow");
    AssertTrue(tone.Edge.A <= 30, "edge should remain atmospheric instead of a hard object");
}

static void AdaptiveTonePreservesClassicNeutralMode()
{
    var selector = new AmbientToneSelector();

    var darkTone = selector.Select(new AmbientColorSample(18, 22, 28), AmbientToneMode.ClassicAdaptive);
    var brightTone = selector.Select(new AmbientColorSample(242, 240, 236), AmbientToneMode.ClassicAdaptive);

    AssertTrue(darkTone.Kind == AmbientToneKind.Moonlight, "classic mode should preserve the early dark-background white glow");
    AssertTrue(darkTone.Core.R >= 210 && darkTone.Core.G >= 220 && darkTone.Core.B >= 230, "classic dark tone should remain soft white-blue");
    AssertTrue(brightTone.Kind == AmbientToneKind.CoolFog, "classic mode should preserve the early bright-background gray fog");
    AssertTrue(brightTone.Core.R <= 115 && brightTone.Core.G <= 130 && brightTone.Core.B <= 150, "classic bright tone should remain cool gray");
}

static void AdaptiveTonePreservesManualMoonlightMode()
{
    var selector = new AmbientToneSelector();

    var tone = selector.Select(new AmbientColorSample(252, 252, 252), AmbientToneMode.Moonlight);

    AssertTrue(tone.Kind == AmbientToneKind.Moonlight, "manual moonlight mode should be selectable");
    AssertTrue(tone.Core.R >= 210 && tone.Core.G >= 220 && tone.Core.B >= 230, "manual moonlight should use the original soft white-blue configuration");
}

static void AdaptiveToneFollowsColorfulBackgroundsWithLowSaturation()
{
    var selector = new AmbientToneSelector();

    var tone = selector.Select(new AmbientColorSample(40, 110, 210));

    AssertTrue(tone.Kind == AmbientToneKind.TintedFog, "colorful backgrounds should use tinted fog");
    AssertTrue(tone.Core.B > tone.Core.R, "blue background should produce a blue-leaning ambient tone");
    AssertTrue(tone.Core.G > tone.Core.R, "tinted tone should be softened toward neutral instead of saturated blue");
}

static void AdaptiveToneStaysVisibleOnWhiteBackgrounds()
{
    var selector = new AmbientToneSelector();

    var tone = selector.Select(new AmbientColorSample(252, 252, 252));

    AssertTrue(tone.Kind == AmbientToneKind.DawnGlow, "white backgrounds should use dawn glow tone");
    AssertTrue(tone.Core.R >= 220 && tone.Core.G >= 180 && tone.Core.B >= 120, "white backgrounds need a visible warm dawn core");
    AssertTrue(tone.MaxOpacity >= 0.38, "white backgrounds need enough warm glow to remain visible");
    AssertTrue(tone.Edge.A >= 20, "white backgrounds need a visible atmospheric edge");
}

static void ScreenAnchorResolvesGoldenLowerPoint()
{
    var anchor = ScreenAnchor.GoldenLower.Resolve(width: 1920, height: 1080);

    AssertClose(960, anchor.X, 0.0001, "golden lower x");
    AssertClose(668, anchor.Y, 1.0, "golden lower y");
}

static void EvaluatorTreatsHighKeyboardOutputAsLight()
{
    var evaluator = new WorkStateEvaluator();
    var snapshot = CreateSnapshot(
        dwell: TimeSpan.FromMinutes(18),
        keyboardInputs: 320,
        mouseTravel: 900,
        windowSwitches: 0);

    var state = evaluator.Evaluate(snapshot);

    AssertEqual(WorkState.Light, state, "high keyboard output should reduce reminder intensity");
}

static void EvaluatorPromotesLowOutputDwellToFocused()
{
    var evaluator = new WorkStateEvaluator();
    var snapshot = CreateSnapshot(
        dwell: TimeSpan.FromMinutes(8),
        keyboardInputs: 8,
        mouseTravel: 420,
        windowSwitches: 0);

    var state = evaluator.Evaluate(snapshot);

    AssertEqual(WorkState.Focused, state, "same-window dwell with low output should be focused");
}

static void EvaluatorPromotesLongLowOutputDwellToDeepFocus()
{
    var evaluator = new WorkStateEvaluator();
    var snapshot = CreateSnapshot(
        dwell: TimeSpan.FromMinutes(16),
        keyboardInputs: 4,
        mouseTravel: 240,
        windowSwitches: 0);

    var state = evaluator.Evaluate(snapshot);

    AssertEqual(WorkState.DeepFocus, state, "long same-window dwell without output should be deep focus");
}

static void EvaluatorPromotesSwitchingWithoutOutputToDeepFocus()
{
    var evaluator = new WorkStateEvaluator();
    var snapshot = CreateSnapshot(
        dwell: TimeSpan.FromMinutes(2),
        keyboardInputs: 0,
        mouseTravel: 1800,
        windowSwitches: 6);

    var state = evaluator.Evaluate(snapshot);

    AssertEqual(WorkState.DeepFocus, state, "frequent switching without output should be deep focus");
}

static void EvaluatorTreatsInactivityAsSessionBreak()
{
    var evaluator = new WorkStateEvaluator();
    var snapshot = CreateSnapshot(
        dwell: TimeSpan.FromMinutes(40),
        keyboardInputs: 0,
        mouseTravel: 0,
        windowSwitches: 0) with
    {
        IdleDuration = TimeSpan.FromMinutes(8)
    };

    var state = evaluator.Evaluate(snapshot);

    AssertEqual(WorkState.Light, state, "inactivity should break the focus session");
}

static void PromptPolicyWaitsForSustainedDeepFocus()
{
    var policy = new GentlePromptPolicy();
    var window = new WindowActivitySnapshot("devenv", "Editor", 42);
    var enteredDeepFocusAt = DateTimeOffset.Parse("2026-05-12T10:00:00+08:00");

    var early = policy.Evaluate(WorkState.DeepFocus, window, enteredDeepFocusAt, enteredDeepFocusAt.AddMinutes(1));
    var ready = policy.Evaluate(WorkState.DeepFocus, window, enteredDeepFocusAt, enteredDeepFocusAt.AddMinutes(3));

    AssertTrue(!early.ShouldShow, "prompt should wait before showing");
    AssertTrue(ready.ShouldShow, "prompt should show after sustained deep focus");
    AssertTrue(!string.IsNullOrWhiteSpace(ready.Text), "prompt should include copy");
}

static void PromptPolicySuppressesRepeatedPromptDuringCooldown()
{
    var policy = new GentlePromptPolicy();
    var window = new WindowActivitySnapshot("devenv", "Editor", 42);
    var enteredDeepFocusAt = DateTimeOffset.Parse("2026-05-12T10:00:00+08:00");

    var first = policy.Evaluate(WorkState.DeepFocus, window, enteredDeepFocusAt, enteredDeepFocusAt.AddMinutes(3));
    var repeated = policy.Evaluate(WorkState.DeepFocus, window, enteredDeepFocusAt, enteredDeepFocusAt.AddMinutes(10));

    AssertTrue(first.ShouldShow, "first prompt should show");
    AssertTrue(!repeated.ShouldShow, "repeated prompt should be suppressed during cooldown");
}

static ActivitySnapshot CreateSnapshot(
    TimeSpan dwell,
    int keyboardInputs,
    double mouseTravel,
    int windowSwitches)
{
    return new ActivitySnapshot(
        Window: new WindowActivitySnapshot("devenv", "Editor", 42),
        WindowDwell: dwell,
        KeyboardInputs: keyboardInputs,
        MouseTravel: mouseTravel,
        WindowSwitches: windowSwitches,
        IdleDuration: TimeSpan.FromSeconds(5),
        SampleDuration: TimeSpan.FromMinutes(5),
        IsAvailable: true);
}

static void AssertEqual<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }
}

static void AssertClose(double expected, double actual, double tolerance, string name)
{
    if (Math.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
