using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class FrpClientService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FrpClientService> _logger;
    private Process? _frpProcess;
    private bool _enabled;

    public FrpClientService(IConfiguration configuration, ILogger<FrpClientService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _enabled = _configuration.GetValue("Frp:Enabled", false);

        if (!_enabled)
        {
            _logger.LogInformation("frp 内网穿透未启用 (Frp:Enabled=false)");
            return;
        }

        var frpcPath = _configuration["Frp:ExecutablePath"] ?? "./frpc.exe";
        if (!File.Exists(frpcPath))
        {
            _logger.LogWarning("frp 可执行文件未找到: {Path},内网穿透服务未启动", frpcPath);
            return;
        }

        var configPath = Path.Combine(Path.GetDirectoryName(frpcPath) ?? ".", "frpc.ini");
        await File.WriteAllTextAsync(configPath, BuildFrpConfig(), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _frpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = frpcPath,
                        Arguments = $"-c \"{configPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                _frpProcess.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) _logger.LogInformation("frp: {Line}", e.Data);
                };
                _frpProcess.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) _logger.LogError("frp: {Line}", e.Data);
                };

                _frpProcess.Start();
                _frpProcess.BeginOutputReadLine();
                _frpProcess.BeginErrorReadLine();
                _logger.LogInformation("frp 客户端已启动,PID={Pid}", _frpProcess.Id);

                await _frpProcess.WaitForExitAsync(stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                _logger.LogWarning("frp 进程退出,5 秒后重启...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "frp 运行异常,10 秒后重试");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private string BuildFrpConfig()
    {
        var serverAddr = _configuration["Frp:ServerAddr"] ?? "";
        var serverPort = _configuration.GetValue("Frp:ServerPort", 7000);
        var token = _configuration["Frp:Token"] ?? "";
        var localPort = _configuration.GetValue("Frp:LocalPort", 5000);
        var customDomain = _configuration["Frp:CustomDomain"] ?? "";

        return $"""
[common]
server_addr = {serverAddr}
server_port = {serverPort}
token = {token}

[web]
type = https
local_ip = 127.0.0.1
local_port = {localPort}
custom_domains = {customDomain}
""";
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_frpProcess is { HasExited: false })
        {
            try
            {
                _frpProcess.Kill(entireProcessTree: true);
                await _frpProcess.WaitForExitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止 frp 进程失败");
            }
        }
        await base.StopAsync(cancellationToken);
    }
}
