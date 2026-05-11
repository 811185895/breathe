using BreatheWidget.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace BreatheWidget.App;

public partial class MainWindow : Window, IDisposable
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly AmbientToneSelector _toneSelector = new();
    private readonly ScreenAmbientSampler _screenSampler = new();
    private readonly DispatcherTimer _ambientTimer;
    private AmbientTone _tone = new AmbientToneSelector().Select(new AmbientColorSample(18, 22, 28));
    private BreathSampler _sampler = new(BreathProfile.Default);
    private ScreenAnchor _anchor = ScreenAnchor.GoldenLower;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        SourceInitialized += HandleSourceInitialized;
        CompositionTarget.Rendering += HandleRendering;

        _ambientTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(900)
        };
        _ambientTimer.Tick += HandleAmbientTick;
    }

    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        CompositionTarget.Rendering -= HandleRendering;
        base.OnClosed(e);
    }

    public void Dispose()
    {
        _ambientTimer.Stop();
        _ambientTimer.Tick -= HandleAmbientTick;
        _screenSampler.Dispose();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        PositionBreathVisual();
        ApplyTone(_tone);
        _ambientTimer.Start();
    }

    private void HandleSourceInitialized(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(handle, GwlExStyle);

        SetWindowLong(
            handle,
            GwlExStyle,
            extendedStyle | WsExLayered | WsExTransparent | WsExNoActivate | WsExToolWindow);
    }

    private void HandleRendering(object? sender, EventArgs e)
    {
        var sample = _sampler.Sample(_clock.Elapsed);

        HaloScale.ScaleX = sample.Scale;
        HaloScale.ScaleY = sample.Scale;
        BreathHalo.Opacity = MapOpacity(sample.Opacity);
        BreathDot.Opacity = Math.Min(0.55, BreathHalo.Opacity * 1.35);
    }

    private void HandleAmbientTick(object? sender, EventArgs e)
    {
        var centerX = Left + Canvas.GetLeft(BreathHalo) + (BreathHalo.Width / 2);
        var centerY = Top + Canvas.GetTop(BreathHalo) + (BreathHalo.Height / 2);
        var offset = (BreathHalo.Width / 2) + 32;
        var sample = AverageSamples(
            _screenSampler.Sample(centerX - offset, centerY),
            _screenSampler.Sample(centerX + offset, centerY),
            _screenSampler.Sample(centerX, centerY - offset),
            _screenSampler.Sample(centerX, centerY + offset));

        _tone = _toneSelector.Select(sample);
        ApplyTone(_tone);
    }

    public void UseVisibleMode()
    {
        _sampler = new BreathSampler(BreathProfile.Default);
    }

    public void UseSubtleMode()
    {
        _sampler = new BreathSampler(BreathProfile.Subtle);
    }

    public void UseAnchor(ScreenAnchor anchor)
    {
        _anchor = anchor;
        PositionBreathVisual();
        HandleAmbientTick(this, EventArgs.Empty);
    }

    private double MapOpacity(double sampledOpacity)
    {
        var profile = BreathProfile.Default;
        var amount = (sampledOpacity - profile.MinOpacity) / (profile.MaxOpacity - profile.MinOpacity);
        amount = Math.Clamp(amount, 0, 1);

        return _tone.MinOpacity + ((_tone.MaxOpacity - _tone.MinOpacity) * amount);
    }

    private void ApplyTone(AmbientTone tone)
    {
        HaloCoreStop.Color = ToMediaColor(tone.Core);
        HaloMidStop.Color = ToMediaColor(tone.Mid);
        HaloEdgeStop.Color = ToMediaColor(tone.Edge);
        BreathDot.Fill = new SolidColorBrush(ToMediaColor(new AmbientColor(255, tone.Core.R, tone.Core.G, tone.Core.B)));
    }

    private static System.Windows.Media.Color ToMediaColor(AmbientColor color)
    {
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static AmbientColorSample AverageSamples(params AmbientColorSample[] samples)
    {
        var red = 0;
        var green = 0;
        var blue = 0;

        foreach (var sample in samples)
        {
            red += sample.R;
            green += sample.G;
            blue += sample.B;
        }

        return new AmbientColorSample(
            (byte)(red / samples.Length),
            (byte)(green / samples.Length),
            (byte)(blue / samples.Length));
    }

    private void PositionBreathVisual()
    {
        const double margin = 24;
        var point = _anchor.Resolve(Width, Height);
        var left = Math.Clamp(point.X - (BreathHalo.Width / 2), margin, Width - BreathHalo.Width - margin);
        var top = Math.Clamp(point.Y - (BreathHalo.Height / 2), margin, Height - BreathHalo.Height - margin);

        Canvas.SetLeft(BreathHalo, left);
        Canvas.SetTop(BreathHalo, top);
        Canvas.SetLeft(BreathDot, left + ((BreathHalo.Width - BreathDot.Width) / 2));
        Canvas.SetTop(BreathDot, top + ((BreathHalo.Height - BreathDot.Height) / 2));
    }

    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExLayered = 0x00080000;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
