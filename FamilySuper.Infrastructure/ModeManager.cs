using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure;

public class ModeManager : IModeManager
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IEncryptionService _encryption;
    private readonly ILogger<ModeManager>? _logger;
    private AppMode _currentMode = AppMode.Adult;
    private readonly string _adminPasswordHash;

    public ModeManager(IConfiguration configuration, IEncryptionService encryption, ILogger<ModeManager>? logger = null)
    {
        _encryption = encryption;
        _logger = logger;
        _adminPasswordHash = configuration["Admin:PasswordHash"] ?? string.Empty;

        var initialMode = configuration["Mode:Initial"];
        if (!string.IsNullOrEmpty(initialMode) && Enum.TryParse<AppMode>(initialMode, true, out var mode))
        {
            _currentMode = mode;
        }
    }

    public AppMode CurrentMode => _currentMode;

    public event Action<AppMode>? ModeChanged;

    public async Task<bool> SwitchModeAsync(AppMode newMode, string password)
    {
        await _lock.WaitAsync();
        try
        {
            if (_currentMode == newMode)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_adminPasswordHash))
            {
                _logger?.LogWarning("管理员密码哈希未配置,使用开发模式默认密码 admin123");
                if (password != "admin123")
                {
                    _logger?.LogWarning("模式切换密码验证失败");
                    return false;
                }
            }
            else if (!_encryption.VerifyPassword(password, _adminPasswordHash))
            {
                _logger?.LogWarning("模式切换密码验证失败");
                return false;
            }

            var previousMode = _currentMode;
            _currentMode = newMode;
            _logger?.LogInformation("模式切换: {Previous} -> {Current}", previousMode, newMode);

            ModeChanged?.Invoke(newMode);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public static string GeneratePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
