namespace BreatheWidget.Core;

public sealed record WindowActivitySnapshot(
    string ProcessName,
    string Title,
    int ProcessId)
{
    public string Identity => $"{ProcessName}:{ProcessId}:{Title}";

    public static WindowActivitySnapshot Unknown { get; } = new(
        ProcessName: "unknown",
        Title: string.Empty,
        ProcessId: 0);
}
