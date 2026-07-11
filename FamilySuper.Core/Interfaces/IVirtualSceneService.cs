using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 家乡实景虚拟化服务接口
/// </summary>
public interface IVirtualSceneService
{
    /// <summary>
    /// 获取场景列表
    /// </summary>
    /// <param name="sceneType">场景类型筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>场景列表</returns>
    Task<List<VirtualScene>> GetScenesAsync(string? sceneType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取场景
    /// </summary>
    /// <param name="id">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>场景实体</returns>
    Task<VirtualScene?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加场景
    /// </summary>
    /// <param name="scene">场景实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的场景</returns>
    Task<VirtualScene> AddAsync(VirtualScene scene, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新场景
    /// </summary>
    /// <param name="scene">场景实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(VirtualScene scene, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除场景
    /// </summary>
    /// <param name="id">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}