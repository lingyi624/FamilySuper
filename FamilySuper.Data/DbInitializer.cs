using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        await EnsureCreatedAsync<FamilyDbContext>(sp);
        await EnsureCreatedAsync<FinanceDbContext>(sp);
        await EnsureCreatedAsync<HealthDbContext>(sp);
        await EnsureCreatedAsync<EducationDbContext>(sp);
        await EnsureCreatedAsync<KnowledgeDbContext>(sp);

        await SeedFamilyDataAsync(sp);
    }

    private static async Task EnsureCreatedAsync<T>(IServiceProvider sp) where T : DbContext
    {
        var ctx = sp.GetRequiredService<T>();
        await ctx.Database.EnsureCreatedAsync();
    }

    private static async Task SeedFamilyDataAsync(IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<FamilyDbContext>();

        if (await ctx.FamilyMembers.AnyAsync()) return;

        ctx.FamilyMembers.Add(new FamilyMember
        {
            Name = "家庭管理员",
            Role = "家庭管理员",
            Gender = Gender.Unknown,
            Mode = AppMode.Adult.ToString().ToLower(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        ctx.SystemConfigs.Add(new SystemConfig
        {
            Key = "system.initialized",
            Value = DateTime.UtcNow.ToString("O"),
            Description = "系统初始化时间",
            Category = "System",
            Mode = AppMode.Adult.ToString().ToLower(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();
    }
}
