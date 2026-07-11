using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 模型重建服务接口
/// 提供基于COLMAP的SfM（Structure-from-Motion）三维重建功能
/// </summary>
public interface IModelReconstructionService
{
    /// <summary>
    /// 批量上传照片
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="photos">照片数据列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片存储目录路径</returns>
    Task<string> UploadPhotosAsync(long sceneId, List<FileUploadData> photos, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从流上传单张照片
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="stream">照片数据流</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片存储路径</returns>
    Task<string> UploadPhotoFromStreamAsync(long sceneId, Stream stream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行模型重建
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>重建结果</returns>
    Task<ReconstructionResult> ReconstructModelAsync(long sceneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取重建状态
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>重建状态信息</returns>
    Task<ReconstructionStatus> GetReconstructionStatusAsync(long sceneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取消重建任务
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task CancelReconstructionAsync(long sceneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 转换为GLB格式
    /// </summary>
    /// <param name="inputPath">输入文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>输出文件路径</returns>
    Task<string> ConvertToGlbAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);
}

/// <summary>
/// 重建结果
/// </summary>
public class ReconstructionResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 生成的GLB模型路径
    /// </summary>
    public string? ModelPath { get; set; }

    /// <summary>
    /// 点云路径
    /// </summary>
    public string? PointCloudPath { get; set; }

    /// <summary>
    /// 网格路径
    /// </summary>
    public string? MeshPath { get; set; }

    /// <summary>
    /// 使用的图像数量
    /// </summary>
    public int? ImageCount { get; set; }

    /// <summary>
    /// 生成的点数量
    /// </summary>
    public int? PointCount { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 处理耗时
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// 重建状态
/// </summary>
public class ReconstructionStatus
{
    /// <summary>
    /// 场景ID
    /// </summary>
    public long SceneId { get; set; }

    /// <summary>
    /// 是否正在处理中
    /// </summary>
    public bool IsProcessing { get; set; }

    /// <summary>
    /// 进度百分比（0-100）
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 当前步骤名称
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 已消耗时间
    /// </summary>
    public TimeSpan? ElapsedTime { get; set; }
}

/// <summary>
/// 文件上传数据
/// </summary>
public class FileUploadData
{
    /// <summary>
    /// 文件二进制数据
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}
