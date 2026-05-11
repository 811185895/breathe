namespace BreatheWidget.Core;

public enum ScreenAnchor
{
    Center,
    LowerThird,
    GoldenLower
}

public static class ScreenAnchorExtensions
{
    public static ScreenPoint Resolve(this ScreenAnchor anchor, double width, double height)
    {
        return anchor switch
        {
            ScreenAnchor.Center => new ScreenPoint(width / 2, height / 2),
            ScreenAnchor.LowerThird => new ScreenPoint(width / 2, height * 2 / 3),
            ScreenAnchor.GoldenLower => new ScreenPoint(width / 2, height * 0.618),
            _ => new ScreenPoint(width / 2, height * 0.618)
        };
    }
}
