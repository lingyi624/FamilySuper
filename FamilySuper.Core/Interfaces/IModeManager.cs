using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

public interface IModeManager
{
    AppMode CurrentMode { get; }
    event Action<AppMode>? ModeChanged;
    Task<bool> SwitchModeAsync(AppMode newMode, string password);
}
