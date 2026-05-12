using BreatheWidget.Core;
using Forms = System.Windows.Forms;
using System.Windows;

namespace BreatheWidget.App;

public partial class App : System.Windows.Application
{
    private TrayController? _trayController;
    private readonly List<MainWindow> _windows = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        foreach (var screen in Forms.Screen.AllScreens)
        {
            var window = new MainWindow(screen);
            _windows.Add(window);
            window.Show();
        }

        _trayController = new TrayController(
            () => Broadcast(w => w.UseVisibleMode()),
            () => Broadcast(w => w.UseSubtleMode()),
            () => Broadcast(w => w.UseAnchor(ScreenAnchor.Center)),
            () => Broadcast(w => w.UseAnchor(ScreenAnchor.LowerThird)),
            () => Broadcast(w => w.UseAnchor(ScreenAnchor.GoldenLower)),
            () => Shutdown());
    }

    private void Broadcast(Action<MainWindow> action)
    {
        foreach (var window in _windows)
        {
            action(window);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayController?.Dispose();
        base.OnExit(e);
    }
}
