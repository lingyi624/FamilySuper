using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public record SurveyField(string Name, string Label, string Type, List<string>? Options);

public record SurveyStatistic(
    int ResponseCount,
    Dictionary<string, Dictionary<string, int>> Distribution);

public interface ISurveyService
{
    Task<List<Survey>> GetSurveysAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<Survey?> GetSurveyByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Survey> AddSurveyAsync(Survey survey, CancellationToken cancellationToken = default);
    Task UpdateSurveyAsync(Survey survey, CancellationToken cancellationToken = default);
    Task DeleteSurveyAsync(long id, CancellationToken cancellationToken = default);

    Task<SurveyResponse> SubmitResponseAsync(SurveyResponse response, CancellationToken cancellationToken = default);
    Task<List<SurveyResponse>> GetResponsesAsync(long surveyId, CancellationToken cancellationToken = default);
    Task<SurveyStatistic> GetStatisticAsync(long surveyId, CancellationToken cancellationToken = default);
}
