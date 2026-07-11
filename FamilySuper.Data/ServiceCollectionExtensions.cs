using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using FamilySuper.Data.Context;
using FamilySuper.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilySuperData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FamilyDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("FamilyDb") ?? "Data Source=data/family.db"));
        services.AddDbContext<FinanceDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("FinanceDb") ?? "Data Source=data/finance.db"));
        services.AddDbContext<HealthDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("HealthDb") ?? "Data Source=data/health.db"));
        services.AddDbContext<EducationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("EducationDb") ?? "Data Source=data/education.db"));
        services.AddDbContext<KnowledgeDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("KnowledgeDb") ?? "Data Source=data/knowledge.db"));

        services.AddScoped<IRepository<FamilyMember>>(sp => new Repository<FamilyMember>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<SystemConfig>>(sp => new Repository<SystemConfig>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<Certificate>>(sp => new Repository<Certificate>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<WorkTask>>(sp => new Repository<WorkTask>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<Survey>>(sp => new Repository<Survey>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<SurveyResponse>>(sp => new Repository<SurveyResponse>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<ShoppingItem>>(sp => new Repository<ShoppingItem>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<EmergencyContact>>(sp => new Repository<EmergencyContact>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<HealthMetric>>(sp => new Repository<HealthMetric>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<EntertainmentRecord>>(sp => new Repository<EntertainmentRecord>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<CareReminder>>(sp => new Repository<CareReminder>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<HomeItem>>(sp => new Repository<HomeItem>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<MediaItem>>(sp => new Repository<MediaItem>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<MedicalGuide>>(sp => new Repository<MedicalGuide>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<MeetingNote>>(sp => new Repository<MeetingNote>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<PriceCompare>>(sp => new Repository<PriceCompare>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<VirtualScene>>(sp => new Repository<VirtualScene>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<Annotation>>(sp => new Repository<Annotation>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<PhotoTimeline>>(sp => new Repository<PhotoTimeline>(sp.GetRequiredService<FamilyDbContext>()));
        services.AddScoped<IRepository<FinanceRecord>>(sp => new Repository<FinanceRecord>(sp.GetRequiredService<FinanceDbContext>()));
        services.AddScoped<IRepository<Budget>>(sp => new Repository<Budget>(sp.GetRequiredService<FinanceDbContext>()));
        services.AddScoped<IRepository<BillReminder>>(sp => new Repository<BillReminder>(sp.GetRequiredService<FinanceDbContext>()));
        services.AddScoped<IRepository<HealthRecord>>(sp => new Repository<HealthRecord>(sp.GetRequiredService<HealthDbContext>()));
        services.AddScoped<IRepository<MedicationPlan>>(sp => new Repository<MedicationPlan>(sp.GetRequiredService<HealthDbContext>()));
        services.AddScoped<IRepository<MedicationRecord>>(sp => new Repository<MedicationRecord>(sp.GetRequiredService<HealthDbContext>()));
        services.AddScoped<IRepository<EducationRecord>>(sp => new Repository<EducationRecord>(sp.GetRequiredService<EducationDbContext>()));
        services.AddScoped<IRepository<StudyPlan>>(sp => new Repository<StudyPlan>(sp.GetRequiredService<EducationDbContext>()));
        services.AddScoped<IRepository<StudyLog>>(sp => new Repository<StudyLog>(sp.GetRequiredService<EducationDbContext>()));
        services.AddScoped<IRepository<ConversationSession>>(sp => new Repository<ConversationSession>(sp.GetRequiredService<KnowledgeDbContext>()));
        services.AddScoped<IRepository<ConversationMessage>>(sp => new Repository<ConversationMessage>(sp.GetRequiredService<KnowledgeDbContext>()));
        services.AddScoped<IRepository<MemoryEntry>>(sp => new Repository<MemoryEntry>(sp.GetRequiredService<KnowledgeDbContext>()));

        return services;
    }
}
