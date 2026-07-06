using System.Text.Json;
using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class SurveyService : ISurveyService
{
    private readonly IRepository<Survey> _surveyRepo;
    private readonly IRepository<SurveyResponse> _responseRepo;
    private readonly ILogger<SurveyService> _logger;

    public SurveyService(
        IRepository<Survey> surveyRepo,
        IRepository<SurveyResponse> responseRepo,
        ILogger<SurveyService> logger)
    {
        _surveyRepo = surveyRepo;
        _responseRepo = responseRepo;
        _logger = logger;
    }

    public async Task<List<Survey>> GetSurveysAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var surveys = await _surveyRepo.GetAllAsync(cancellationToken);
        if (activeOnly == true) surveys = surveys.Where(s => s.IsActive).ToList();
        return surveys.OrderByDescending(s => s.StartDate).ToList();
    }

    public async Task<Survey?> GetSurveyByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _surveyRepo.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Survey> AddSurveyAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        await _surveyRepo.AddAsync(survey, cancellationToken);
        await _surveyRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增调研:{Title}", survey.Title);
        return survey;
    }

    public async Task UpdateSurveyAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        survey.UpdatedAt = DateTime.UtcNow;
        await _surveyRepo.UpdateAsync(survey, cancellationToken);
        await _surveyRepo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSurveyAsync(long id, CancellationToken cancellationToken = default)
    {
        var survey = await _surveyRepo.GetByIdAsync(id, cancellationToken);
        if (survey is null) return;
        survey.IsDeleted = true;
        survey.UpdatedAt = DateTime.UtcNow;
        await _surveyRepo.SaveChangesAsync(cancellationToken);
    }

    public async Task<SurveyResponse> SubmitResponseAsync(SurveyResponse response, CancellationToken cancellationToken = default)
    {
        response.SubmittedAt = DateTime.UtcNow;
        await _responseRepo.AddAsync(response, cancellationToken);
        await _responseRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("提交调研响应:调研#{SurveyId}", response.SurveyId);
        return response;
    }

    public async Task<List<SurveyResponse>> GetResponsesAsync(long surveyId, CancellationToken cancellationToken = default)
    {
        var all = await _responseRepo.GetAllAsync(cancellationToken);
        return all.Where(r => r.SurveyId == surveyId).OrderByDescending(r => r.SubmittedAt).ToList();
    }

    public async Task<SurveyStatistic> GetStatisticAsync(long surveyId, CancellationToken cancellationToken = default)
    {
        var survey = await _surveyRepo.GetByIdAsync(surveyId, cancellationToken);
        var responses = await GetResponsesAsync(surveyId, cancellationToken);

        var fields = ParseFields(survey?.FieldsJson);
        var distribution = new Dictionary<string, Dictionary<string, int>>();

        foreach (var field in fields)
        {
            var fieldDist = new Dictionary<string, int>();
            foreach (var resp in responses)
            {
                var values = ParseValues(resp.ValuesJson);
                if (values.TryGetValue(field.Name, out var val))
                {
                    var key = val.ValueKind == JsonValueKind.Null ? "(空)" : val.ToString();
                    if (field.Type == "multi" && val.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in val.EnumerateArray())
                        {
                            var k = item.GetString() ?? "(空)";
                            fieldDist[k] = fieldDist.GetValueOrDefault(k) + 1;
                        }
                    }
                    else
                    {
                        fieldDist[key] = fieldDist.GetValueOrDefault(key) + 1;
                    }
                }
            }
            distribution[field.Name] = fieldDist;
        }

        return new SurveyStatistic(responses.Count, distribution);
    }

    private static List<SurveyField> ParseFields(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<SurveyField>();
        try
        {
            return JsonSerializer.Deserialize<List<SurveyField>>(json) ?? new List<SurveyField>();
        }
        catch { return new List<SurveyField>(); }
    }

    private static Dictionary<string, JsonElement> ParseValues(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone());
        }
        catch { return new(); }
    }
}
