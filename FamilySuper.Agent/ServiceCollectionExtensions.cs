using FamilySuper.Agent.Plugins;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Agent;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilySuperAgent(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAgentService, AgentService>();
        services.AddSingleton<IMemoryManager, MemoryManager>();
        return services;
    }
}
