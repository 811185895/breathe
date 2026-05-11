using BreatheWidget.Core;
using System.Windows;

namespace BreatheWidget.App;

public partial class App : System.Windows.Application
{
    private TrayController? _trayController;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _mainWindow = new MainWindow();
        _trayController = new TrayController(
            () => _mainWindow.UseVisibleMode(),
            () => _mainWindow.UseSubtleMode(),
            () => _mainWindow.UseAnchor(ScreenAnchor.Center),
            () => _mainWindow.UseAnchor(ScreenAnchor.LowerThird),
            () => _mainWindow.UseAnchor(ScreenAnchor.GoldenLower),
            () => Shutdown());
        _mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayController?.Dispose();
        base.OnExit(e);
    }
}
