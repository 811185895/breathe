namespace BreatheWidget.Core;

public sealed record GentlePromptDecision(bool ShouldShow, string Text)
{
    public static GentlePromptDecision None { get; } = new(false, string.Empty);
}
