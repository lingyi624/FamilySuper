using System.ClientModel;
using FamilySuper.Agent.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenAI;

namespace FamilySuper.Agent;

public static class KernelBuilder
{
    public const string ModelId = "deepseek-v4-pro";
    public static readonly Uri Endpoint = new("https://api.deepseek.com/v1");

    public static (Kernel Kernel, bool Available) Build(
        IConfiguration configuration,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? rootProvider = null)
    {
        var apiKey = configuration["DeepSeek:ApiKey"];
        var available = !string.IsNullOrWhiteSpace(apiKey);

        var builder = Kernel.CreateBuilder();

        if (loggerFactory is not null)
        {
            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);
        }

        if (available)
        {
            var openAIClient = new OpenAIClient(
                new ApiKeyCredential(apiKey!),
                new OpenAIClientOptions { Endpoint = Endpoint });
            builder.AddOpenAIChatCompletion(ModelId, openAIClient);
        }

        if (rootProvider is not null)
        {
            builder.Services.AddSingleton(rootProvider);
            builder.Plugins.AddFromType<FamilyTools>();
            builder.Plugins.AddFromType<FinanceTools>();
            builder.Plugins.AddFromType<HealthTools>();
        }

        var kernel = builder.Build();
        return (kernel, available);
    }
}
