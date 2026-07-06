using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.BlazorUI.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilySuperBlazorUI(this IServiceCollection services)
    {
        services.AddScoped<ModeStateContainer>();
        return services;
    }
}
