using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Host.WPF.Services;

public class SystemTrayService : IDisposable
{
    private readonly ILogger<SystemTrayService> _logger;
    private NotifyIcon? _notifyIcon;
    private bool _disposed;

    public event EventHandler? ShowWindowRequested;
    public event EventHandler? ExitRequested;

    public SystemTrayService(ILogger<SystemTrayService> logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "家庭超级管家"
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("显示主窗口", null, (_, _) => ShowWindowRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));

        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);

        _logger.LogInformation("系统托盘已初始化");
    }

    public void ShowBalloon(string title, string message, int timeout = 3000)
    {
        if (_notifyIcon is null || !_notifyIcon.Visible)
        {
            _logger.LogWarning("托盘图标未初始化,无法显示气泡通知");
            return;
        }

        _notifyIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Info);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _notifyIcon?.Visible = false;
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            _disposed = true;
        }
    }
}
