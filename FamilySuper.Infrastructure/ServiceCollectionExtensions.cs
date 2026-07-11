using FamilySuper.Core.Interfaces;
using FamilySuper.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilySuperInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var encryptionKey = configuration["Encryption:Key"];
        services.AddSingleton<IEncryptionService>(_ => new EncryptionService(encryptionKey));
        services.AddSingleton<IModeManager, ModeManager>();
        services.AddSingleton<IOcrService, OcrService>();
        services.AddSingleton<IEmbeddingService, BgeEmbeddingService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddHostedService<FrpClientService>();

        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<IWorkTaskService, WorkTaskService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMedicationService, MedicationService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IStudyService, StudyService>();
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IShoppingService, ShoppingService>();
        services.AddScoped<IElderlyCareService, ElderlyCareService>();
        services.AddScoped<ICareReminderService, CareReminderService>();
        services.AddScoped<IHomeItemService, HomeItemService>();
        services.AddScoped<IMediaItemService, MediaItemService>();
        services.AddScoped<IMedicalGuideService, MedicalGuideService>();
        services.AddScoped<IMeetingNoteService, MeetingNoteService>();
        services.AddScoped<IPriceCompareService, PriceCompareService>();
        services.AddScoped<IVirtualSceneService, VirtualSceneService>();
        services.AddScoped<IAnnotationService, AnnotationService>();
        services.AddScoped<IModelReconstructionService, ModelReconstructionService>();
        services.AddScoped<IPhotoTimelineService, PhotoTimelineService>();

        return services;
    }
}
