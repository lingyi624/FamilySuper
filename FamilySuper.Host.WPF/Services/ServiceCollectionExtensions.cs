using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Host.WPF.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilySuperBlazorUI(this IServiceCollection services)
    {
        services.AddScoped<ModeStateContainer>();
        return services;
    }
}
