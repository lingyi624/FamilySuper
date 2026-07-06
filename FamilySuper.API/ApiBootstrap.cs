using FamilySuper.API.Endpoints;
using FamilySuper.API.Middlewares;
using FamilySuper.Core.Interfaces;
using FamilySuper.Data;
using FamilySuper.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FamilySuper.API;

public static class ApiBootstrap
{
    public static WebApplication Build(IServiceProvider rootProvider, IConfiguration configuration)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddConfiguration(configuration);

        builder.Services.AddSingleton<IServiceProvider>(rootProvider);
        builder.Services.AddSingleton<IModeManager>(rootProvider.GetRequiredService<IModeManager>());
        builder.Services.AddSingleton<IAgentService>(rootProvider.GetRequiredService<IAgentService>());
        builder.Services.AddSingleton<IEncryptionService>(rootProvider.GetRequiredService<IEncryptionService>());
        builder.Services.AddSingleton<INotificationService>(rootProvider.GetRequiredService<INotificationService>());

        builder.Services.AddHostedService<MedicationReminderWorker>();
        builder.Services.AddHostedService<ProactivePushWorker>();

        builder.Services.AddFamilySuperData(builder.Configuration);

        builder.Services.AddScoped<IFinanceService, FinanceService>();
        builder.Services.AddScoped<IHealthService, HealthService>();
        builder.Services.AddScoped<IEducationService, EducationService>();
        builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();
        builder.Services.AddScoped<ICertificateService, CertificateService>();
        builder.Services.AddScoped<IConversationService, ConversationService>();
        builder.Services.AddScoped<IMedicationService, MedicationService>();
        builder.Services.AddScoped<IBudgetService, BudgetService>();
        builder.Services.AddScoped<IBillService, BillService>();
        builder.Services.AddScoped<IStudyService, StudyService>();
        builder.Services.AddScoped<ISurveyService, SurveyService>();

        builder.Services.AddLogging(b => b.AddProvider(rootProvider.GetService<ILoggerProvider>()!));

        var app = builder.Build();

        app.UseMiddleware<ModeFilterMiddleware>();

        HealthEndpoints.Map(app);
        FamilyEndpoints.Map(app);
        FinanceEndpoints.Map(app);
        HealthRecordsEndpoints.Map(app);
        EducationEndpoints.Map(app);
        WorkTaskEndpoints.Map(app);
        CertificateEndpoints.Map(app);
        ConversationEndpoints.Map(app);
        MedicationEndpoints.Map(app);
        BudgetEndpoints.Map(app);
        BillEndpoints.Map(app);
        StudyEndpoints.Map(app);
        SurveyEndpoints.Map(app);
        NotificationEndpoints.Map(app);

        return app;
    }
}
