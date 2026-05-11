namespace BreatheWidget.Core;

public sealed class GentlePromptPolicy
{
    private static readonly TimeSpan DeepFocusPersistence = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PromptCooldown = TimeSpan.FromMinutes(15);

    private readonly string[] _prompts =
    [
        "你已经在这里停留很久了。可以先回到一次呼吸。",
        "先不急着解决它，觉察一下身体现在的状态。",
        "让视线离开屏幕几秒，看看呼吸有没有变浅。",
        "你可以暂停一下，不是放弃，只是回来看看自己。",
        "慢慢吸气，慢慢呼气。先把自己找回来。"
    ];

    private readonly Dictionary<string, DateTimeOffset> _lastPromptByWindow = new();
    private int _nextPromptIndex;

    public GentlePromptDecision Evaluate(
        WorkState state,
        WindowActivitySnapshot window,
        DateTimeOffset? enteredDeepFocusAt,
        DateTimeOffset now)
    {
        if (state != WorkState.DeepFocus || enteredDeepFocusAt is null)
        {
            return GentlePromptDecision.None;
        }

        if (now - enteredDeepFocusAt.Value < DeepFocusPersistence)
        {
            return GentlePromptDecision.None;
        }

        var identity = window.Identity;
        if (_lastPromptByWindow.TryGetValue(identity, out var lastPromptAt) &&
            now - lastPromptAt < PromptCooldown)
        {
            return GentlePromptDecision.None;
        }

        _lastPromptByWindow[identity] = now;
        var text = _prompts[_nextPromptIndex % _prompts.Length];
        _nextPromptIndex++;
        return new GentlePromptDecision(true, text);
    }
}
