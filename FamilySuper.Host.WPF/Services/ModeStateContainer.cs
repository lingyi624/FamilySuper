using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;

namespace FamilySuper.Host.WPF.Services;

public class ModeStateContainer : IDisposable
{
    private readonly IModeManager _modeManager;
    private bool _disposed;

    public AppMode CurrentMode => _modeManager.CurrentMode;
    public event Action? OnChange;

    public ModeStateContainer(IModeManager modeManager)
    {
        _modeManager = modeManager;
        _modeManager.ModeChanged += OnModeChanged;
    }

    private void OnModeChanged(AppMode newMode)
    {
        OnChange?.Invoke();
    }

    public Task<bool> SwitchModeAsync(AppMode newMode, string password)
    {
        return _modeManager.SwitchModeAsync(newMode, password);
    }

    public void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        if (!_disposed)
        {
            _modeManager.ModeChanged -= OnModeChanged;
            _disposed = true;
        }
    }
}
