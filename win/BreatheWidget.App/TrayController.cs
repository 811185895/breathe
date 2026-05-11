using Forms = System.Windows.Forms;

namespace BreatheWidget.App;

internal sealed class TrayController : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;

    public TrayController(
        Action visibleMode,
        Action subtleMode,
        Action centerPosition,
        Action lowerThirdPosition,
        Action goldenPosition,
        Action exit)
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Visible", null, (_, _) => visibleMode());
        menu.Items.Add("Subtle", null, (_, _) => subtleMode());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Position: Center", null, (_, _) => centerPosition());
        menu.Items.Add("Position: Lower Third", null, (_, _) => lowerThirdPosition());
        menu.Items.Add("Position: Golden Point", null, (_, _) => goldenPosition());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exit());

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Breathe Widget",
            ContextMenuStrip = menu,
            Visible = true
        };
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }
}
