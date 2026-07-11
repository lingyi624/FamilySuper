using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 调查字段
/// </summary>
/// <param name="Name">字段名</param>
/// <param name="Label">标签</param>
/// <param name="Type">类型</param>
/// <param name="Options">选项列表</param>
public record SurveyField(string Name, string Label, string Type, List<string>? Options);

/// <summary>
/// 调查统计信息
/// </summary>
/// <param name="ResponseCount">响应数量</param>
/// <param name="Distribution">分布统计</param>
public record SurveyStatistic(
    int ResponseCount,
    Dictionary<string, Dictionary<string, int>> Distribution);

/// <summary>
/// 调查服务接口
/// </summary>
public interface ISurveyService
{
    /// <summary>
    /// 获取调查列表
    /// </summary>
    /// <param name="activeOnly">是否只获取激活的</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>调查列表</returns>
    Task<List<Survey>> GetSurveysAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取调查
    /// </summary>
    /// <param name="id">调查ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>调查</returns>
    Task<Survey?> GetSurveyByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加调查
    /// </summary>
    /// <param name="survey">调查</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新调查</returns>
    Task<Survey> AddSurveyAsync(Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新调查
    /// </summary>
    /// <param name="survey">调查</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateSurveyAsync(Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除调查
    /// </summary>
    /// <param name="id">调查ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteSurveyAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交调查响应
    /// </summary>
    /// <param name="response">响应</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新响应</returns>
    Task<SurveyResponse> SubmitResponseAsync(SurveyResponse response, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取调查响应列表
    /// </summary>
    /// <param name="surveyId">调查ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应列表</returns>
    Task<List<SurveyResponse>> GetResponsesAsync(long surveyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取调查统计
    /// </summary>
    /// <param name="surveyId">调查ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>统计信息</returns>
    Task<SurveyStatistic> GetStatisticAsync(long surveyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 导出调查数据为CSV
    /// </summary>
    /// <param name="surveyId">调查ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>CSV内容</returns>
    Task<string> ExportCsvAsync(long surveyId, CancellationToken cancellationToken = default);
}
