using System.IO;
using System.Windows;
using FamilySuper.Agent;
using FamilySuper.API;
using FamilySuper.Core.Interfaces;
using FamilySuper.Data;
using FamilySuper.Data.Context;
using FamilySuper.Host.WPF.Services;
using FamilySuper.Infrastructure;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FamilySuper.Host.WPF;

public partial class App : System.Windows.Application
{
    private ServiceProvider _serviceProvider = null!;
    private Microsoft.AspNetCore.Builder.WebApplication _apiHost = null!;
    private SystemTrayService _trayService = null!;
    private CancellationTokenSource _appCts = new();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Directory.CreateDirectory("./data");
        Directory.CreateDirectory("./logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("./logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("=== 家庭超级管家启动 ===");

            var configuration = BuildConfiguration();
            Log.Information("配置加载完成");

            ConfigureServices(configuration);
            Log.Information("服务配置完成");

            Log.Information("开始数据库初始化...");
            await DbInitializer.InitializeAsync(_serviceProvider);
            Log.Information("数据库初始化完成");

            Log.Information("开始启动API服务...");
            StartApiHost(configuration);
            Log.Information("API服务启动完成");

            _trayService = _serviceProvider.GetRequiredService<SystemTrayService>();
            _trayService.Initialize();
            Log.Information("托盘服务初始化完成");

            Log.Information("创建主窗口...");
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Log.Information("主窗口显示完成");

            Log.Information("应用启动完成");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用启动失败");
            System.Windows.MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<App>(optional: true)
            .Build();
    }

    private void ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddLogging(b =>
        {
            b.AddSerilog(Log.Logger, dispose: false);
        });

        services.AddSingleton<IConfiguration>(configuration);

        services.AddFamilySuperInfrastructure(configuration);
        services.AddFamilySuperAgent(configuration);
        services.AddScoped<ModeStateContainer>();
        services.AddWpfBlazorWebView();
        services.AddFamilySuperData(configuration);

        services.AddSingleton<SystemTrayService>();
        services.AddSingleton<HotKeyService>();
        services.AddSingleton<NativeNotificationService>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private void StartApiHost(IConfiguration configuration)
    {
        var apiPort = configuration.GetValue("Api:Port", 5000);
        _apiHost = ApiBootstrap.Build(_serviceProvider, configuration);
        _apiHost.Urls.Add($"http://localhost:{apiPort}");
        _ = _apiHost.RunAsync();
        Log.Information("API 服务已启动: http://localhost:{Port}", apiPort);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_apiHost is not null)
            {
                using var exitCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                try
                {
                    await _apiHost.StopAsync(exitCts.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Log.Warning(ex, "API 主机关闭超时");
                }
            }
            _appCts.Cancel();
            await _serviceProvider.DisposeAsync();
            Log.Information("=== 应用退出 ===");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "退出时发生错误");
        }
        finally
        {
            Log.CloseAndFlush();
            _appCts.Dispose();
        }

        base.OnExit(e);
    }
}
