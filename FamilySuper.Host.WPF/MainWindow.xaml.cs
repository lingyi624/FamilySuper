using System.Windows;
using FamilySuper.Host.WPF.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Host.WPF;

public partial class MainWindow : Window
{
    private readonly SystemTrayService _trayService;
    private readonly HotKeyService _hotKeyService;
    private readonly NativeNotificationService _notificationService;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        blazorWebView.Services = serviceProvider;
        blazorWebView.StartPath = "/";

        _trayService = serviceProvider.GetRequiredService<SystemTrayService>();
        _hotKeyService = serviceProvider.GetRequiredService<HotKeyService>();
        _notificationService = serviceProvider.GetRequiredService<NativeNotificationService>();

        _trayService.ShowWindowRequested += (_, _) => Dispatcher.Invoke(ShowFromTray);
        _trayService.ExitRequested += (_, _) => Dispatcher.Invoke(ExitApp);

        Loaded += OnWindowLoaded;
        Closing += OnWindowClosing;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        _hotKeyService.RegisterGlobalHotKey();
        _hotKeyService.HotKeyPressed += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (Visibility == Visibility.Visible && WindowState != WindowState.Minimized)
                {
                    Hide();
                    _notificationService.Show("家庭超级管家", "已最小化到托盘,Ctrl+Alt+H 重新唤出");
                }
                else
                {
                    ShowFromTray();
                }
            });
        };

        _notificationService.Show("家庭超级管家", "应用已启动,按 Ctrl+Alt+H 可快速唤出");
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private void OnMinimizeToTray(object sender, RoutedEventArgs e)
    {
        Hide();
        _notificationService.Show("家庭超级管家", "已最小化到托盘");
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        ExitApp();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        _notificationService.Show("家庭超级管家", "窗口已最小化到托盘,右键托盘图标退出");
    }

    private void ExitApp()
    {
        _hotKeyService.UnregisterGlobalHotKey();
        _trayService.Dispose();
        Closing -= OnWindowClosing;
        System.Windows.Application.Current.Shutdown();
    }
}
