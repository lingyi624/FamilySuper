using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 就医指南服务接口
/// </summary>
public interface IMedicalGuideService
{
    /// <summary>
    /// 获取就医指南列表
    /// </summary>
    /// <param name="category">分类筛选</param>
    /// <param name="department">科室筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>就医指南列表</returns>
    Task<List<MedicalGuide>> GetGuidesAsync(string? category = null, string? department = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取就医指南
    /// </summary>
    /// <param name="id">指南ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>就医指南</returns>
    Task<MedicalGuide?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加就医指南
    /// </summary>
    /// <param name="guide">就医指南</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的就医指南</returns>
    Task<MedicalGuide> AddAsync(MedicalGuide guide, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新就医指南
    /// </summary>
    /// <param name="guide">就医指南</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(MedicalGuide guide, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除就医指南
    /// </summary>
    /// <param name="id">指南ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}