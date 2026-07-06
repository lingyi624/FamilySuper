using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FamilySuper.API.Middlewares;

public class ModeFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ModeFilterMiddleware> _logger;

    public ModeFilterMiddleware(RequestDelegate next, ILogger<ModeFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var modeHeader = context.Request.Headers["X-App-Mode"].ToString();
        if (!string.IsNullOrEmpty(modeHeader))
        {
            _logger.LogDebug("请求携带模式标识: {Mode}", modeHeader);
        }

        await _next(context);
    }
}
