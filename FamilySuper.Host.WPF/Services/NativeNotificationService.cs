using Microsoft.Extensions.Logging;

namespace FamilySuper.Host.WPF.Services;

public class NativeNotificationService
{
    private readonly SystemTrayService _trayService;
    private readonly ILogger<NativeNotificationService> _logger;

    public NativeNotificationService(SystemTrayService trayService, ILogger<NativeNotificationService> logger)
    {
        _trayService = trayService;
        _logger = logger;
    }

    public void Show(string title, string message, int timeout = 3000)
    {
        try
        {
            _trayService.ShowBalloon(title, message, timeout);
            _logger.LogDebug("通知: {Title} - {Message}", title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示通知失败");
        }
    }
}
