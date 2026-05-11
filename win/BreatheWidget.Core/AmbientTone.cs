namespace BreatheWidget.Core;

public sealed record AmbientTone(
    AmbientToneKind Kind,
    AmbientColor Core,
    AmbientColor Mid,
    AmbientColor Edge,
    double MinOpacity,
    double MaxOpacity);
