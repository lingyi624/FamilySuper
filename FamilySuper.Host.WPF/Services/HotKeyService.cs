using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Host.WPF.Services;

public class HotKeyService : IDisposable
{
    private readonly ILogger<HotKeyService> _logger;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HotKeyId = 0x9001;
    private const uint ModControl = 0x0002;
    private const uint ModAlt = 0x0001;
    private const uint VkH = 0x48;

    private HwndSource? _source;
    private bool _registered;

    public event EventHandler? HotKeyPressed;

    public HotKeyService(ILogger<HotKeyService> logger)
    {
        _logger = logger;
    }

    public void RegisterGlobalHotKey()
    {
        if (_registered) return;

        var window = System.Windows.Application.Current.MainWindow
            ?? System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault();
        if (window == null)
        {
            _logger.LogWarning("注册热键失败:未找到活动窗口");
            return;
        }

        var helper = new WindowInteropHelper(window);
        if (helper.Handle == IntPtr.Zero)
        {
            helper.EnsureHandle();
        }

        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(HwndHook);

        _registered = RegisterHotKey(helper.Handle, HotKeyId, ModControl | ModAlt, VkH);

        if (_registered)
        {
            _logger.LogInformation("全局热键 Ctrl+Alt+H 已注册");
        }
        else
        {
            _logger.LogError("全局热键注册失败,错误码: {Error}", Marshal.GetLastWin32Error());
        }
    }

    public void UnregisterGlobalHotKey()
    {
        if (!_registered) return;

        var window = System.Windows.Application.Current.MainWindow
            ?? System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault();
        if (window != null)
        {
            var helper = new WindowInteropHelper(window);
            UnregisterHotKey(helper.Handle, HotKeyId);
        }

        _source?.RemoveHook(HwndHook);
        _source = null;
        _registered = false;
        _logger.LogInformation("全局热键已注销");
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WmHotkey = 0x0312;

        if (msg == WmHotkey && wParam.ToInt32() == HotKeyId)
        {
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterGlobalHotKey();
    }
}
