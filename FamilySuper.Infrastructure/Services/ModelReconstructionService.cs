using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FamilySuper.Infrastructure.Services;

/// <summary>
/// 模型重建服务
/// 基于COLMAP实现SfM（Structure-from-Motion）三维重建
/// </summary>
public class ModelReconstructionService : IModelReconstructionService
{
    private readonly IRepository<VirtualScene> _sceneRepo;
    private readonly ILogger<ModelReconstructionService> _logger;
    private readonly Dictionary<long, ReconstructionStatus> _statusCache = new();

    /// <summary>
    /// 基础数据存储路径
    /// </summary>
    private const string BaseDataPath = "./data/virtual-hometown";
    
    /// <summary>
    /// COLMAP可执行文件路径
    /// </summary>
    private readonly string _colmapPath;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sceneRepo">场景仓储</param>
    /// <param name="logger">日志记录器</param>
    public ModelReconstructionService(IRepository<VirtualScene> sceneRepo, ILogger<ModelReconstructionService> logger)
    {
        _sceneRepo = sceneRepo;
        _logger = logger;
        _colmapPath = FindColmapPath();
    }

    /// <summary>
    /// 查找COLMAP可执行文件路径
    /// </summary>
    /// <returns>COLMAP可执行文件路径</returns>
    private static string FindColmapPath()
    {
        string[] possiblePaths = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { @"C:\Program Files\COLMAP\colmap.exe", @"C:\COLMAP\colmap.exe", "./colmap.exe" }
            : new[] { "/usr/local/bin/colmap", "/usr/bin/colmap", "./colmap" };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path)) return path;
        }
        return "colmap";
    }

    /// <summary>
    /// 批量上传照片
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="photos">照片数据列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片存储目录路径</returns>
    public async Task<string> UploadPhotosAsync(long sceneId, List<FileUploadData> photos, CancellationToken cancellationToken = default)
    {
        var scene = await _sceneRepo.GetByIdAsync(sceneId, cancellationToken);
        if (scene == null) throw new InvalidOperationException("场景不存在");

        var sceneDir = Path.Combine(BaseDataPath, sceneId.ToString());
        var imagesDir = Path.Combine(sceneDir, "images");
        Directory.CreateDirectory(imagesDir);

        foreach (var photo in photos)
        {
            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(imagesDir, fileName);

            await File.WriteAllBytesAsync(filePath, photo.Data, cancellationToken);
            _logger.LogInformation("上传照片: {FilePath}", filePath);
        }

        scene.PhotoCount = photos.Count;
        scene.PhotoDirectory = imagesDir;
        await _sceneRepo.UpdateAsync(scene, cancellationToken);
        await _sceneRepo.SaveChangesAsync(cancellationToken);

        return imagesDir;
    }

    /// <summary>
    /// 从流上传单张照片
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="stream">照片数据流</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片存储路径</returns>
    public async Task<string> UploadPhotoFromStreamAsync(long sceneId, Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var scene = await _sceneRepo.GetByIdAsync(sceneId, cancellationToken);
        if (scene == null) throw new InvalidOperationException("场景不存在");

        var sceneDir = Path.Combine(BaseDataPath, sceneId.ToString());
        var imagesDir = Path.Combine(sceneDir, "images");
        Directory.CreateDirectory(imagesDir);

        var newFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(imagesDir, newFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        scene.PhotoCount = (scene.PhotoCount ?? 0) + 1;
        scene.PhotoDirectory = imagesDir;
        await _sceneRepo.UpdateAsync(scene, cancellationToken);
        await _sceneRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("上传照片: {FilePath}", filePath);
        return filePath;
    }

    /// <summary>
    /// 执行模型重建
    /// 包含：数据库创建、特征提取、特征匹配、稀疏重建、稠密重建、网格生成、格式转换
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>重建结果</returns>
    public async Task<ReconstructionResult> ReconstructModelAsync(long sceneId, CancellationToken cancellationToken = default)
    {
        var scene = await _sceneRepo.GetByIdAsync(sceneId, cancellationToken);
        if (scene == null) throw new InvalidOperationException("场景不存在");

        if (string.IsNullOrEmpty(scene.PhotoDirectory) || !Directory.Exists(scene.PhotoDirectory))
        {
            throw new InvalidOperationException("未找到照片目录");
        }

        var sceneDir = Path.Combine(BaseDataPath, sceneId.ToString());
        var databasePath = Path.Combine(sceneDir, "reconstruction.db");
        var outputDir = Path.Combine(sceneDir, "sparse");
        var denseDir = Path.Combine(sceneDir, "dense");
        var meshDir = Path.Combine(sceneDir, "mesh");
        var glbPath = Path.Combine(sceneDir, "model.glb");

        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(denseDir);
        Directory.CreateDirectory(meshDir);

        var startTime = DateTime.Now;
        var result = new ReconstructionResult();

        try
        {
            UpdateStatus(sceneId, true, 10, "创建数据库...", startTime);

            if (!await RunColmapCommand($"database_creator --database_path {databasePath}", cancellationToken))
            {
                result.Success = false;
                result.ErrorMessage = "创建数据库失败";
                UpdateStatus(sceneId, false, 0, "创建数据库失败", startTime);
                return result;
            }

            UpdateStatus(sceneId, true, 20, "导入照片...", startTime);

            if (!await RunColmapCommand($"image_extractor --database_path {databasePath} --image_path \"{scene.PhotoDirectory}\"", cancellationToken))
            {
                result.Success = false;
                result.ErrorMessage = "导入照片失败";
                UpdateStatus(sceneId, false, 0, "导入照片失败", startTime);
                return result;
            }

            UpdateStatus(sceneId, true, 30, "特征匹配...", startTime);

            if (!await RunColmapCommand($"exhaustive_matcher --database_path {databasePath}", cancellationToken))
            {
                result.Success = false;
                result.ErrorMessage = "特征匹配失败";
                UpdateStatus(sceneId, false, 0, "特征匹配失败", startTime);
                return result;
            }

            UpdateStatus(sceneId, true, 40, "稀疏重建...", startTime);

            if (!await RunColmapCommand($"mapper --database_path {databasePath} --image_path \"{scene.PhotoDirectory}\" --output_path {outputDir}", cancellationToken))
            {
                result.Success = false;
                result.ErrorMessage = "稀疏重建失败";
                UpdateStatus(sceneId, false, 0, "稀疏重建失败", startTime);
                return result;
            }

            UpdateStatus(sceneId, true, 60, "稠密重建...", startTime);

            if (!await RunColmapCommand($"dense_stereo --workspace_path {sceneDir} --workspace_format COLMAP --input_path {outputDir} --output_path {denseDir}", cancellationToken))
            {
                result.Success = false;
                result.ErrorMessage = "稠密重建失败";
                UpdateStatus(sceneId, false, 0, "稠密重建失败", startTime);
                return result;
            }

            UpdateStatus(sceneId, true, 80, "生成网格...", startTime);

            if (!await RunColmapCommand($"poisson_mesher --input_path {denseDir} --output_path {meshDir}", cancellationToken))
            {
                result.Success = false;
                result.ErrorMessage = "生成网格失败";
                UpdateStatus(sceneId, false, 0, "生成网格失败", startTime);
                return result;
            }

            UpdateStatus(sceneId, true, 90, "转换为GLB格式...", startTime);

            var plyPath = Path.Combine(meshDir, "mesh.ply");
            if (File.Exists(plyPath))
            {
                await ConvertToGlbAsync(plyPath, glbPath, cancellationToken);
                result.ModelPath = glbPath;
                result.PointCloudPath = Path.Combine(denseDir, "fused.ply");
                result.MeshPath = plyPath;
            }

            result.Success = true;
            result.ImageCount = scene.PhotoCount;
            result.ProcessingTime = DateTime.Now - startTime;

            scene.ModelPath = glbPath;
            scene.ReconstructionStatus = "completed";
            scene.ReconstructedAt = DateTime.UtcNow;
            await _sceneRepo.UpdateAsync(scene, cancellationToken);
            await _sceneRepo.SaveChangesAsync(cancellationToken);

            UpdateStatus(sceneId, false, 100, "重建完成", startTime);

            _logger.LogInformation("模型重建成功: {SceneId}, 耗时: {ProcessingTime}", sceneId, result.ProcessingTime);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            UpdateStatus(sceneId, false, 0, $"重建失败: {ex.Message}", startTime);
            _logger.LogError(ex, "模型重建失败: {SceneId}", sceneId);
        }

        return result;
    }

    /// <summary>
    /// 执行COLMAP命令
    /// </summary>
    /// <param name="arguments">命令参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    private async Task<bool> RunColmapCommand(string arguments, CancellationToken cancellationToken)
    {
        if (!IsColmapAvailable())
        {
            _logger.LogWarning("COLMAP不可用，跳过命令: {Arguments}", arguments);
            return false;
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _colmapPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(output)) _logger.LogDebug("COLMAP输出: {Output}", output);
            if (!string.IsNullOrEmpty(error)) _logger.LogWarning("COLMAP错误: {Error}", error);

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行COLMAP命令失败: {Arguments}", arguments);
            return false;
        }
    }

    /// <summary>
    /// 检查COLMAP是否可用
    /// </summary>
    /// <returns>是否可用</returns>
    private bool IsColmapAvailable()
    {
        if (string.Equals(_colmapPath, "colmap", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "colmap",
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        return File.Exists(_colmapPath);
    }

    /// <summary>
    /// 获取重建状态
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>重建状态信息</returns>
    public Task<ReconstructionStatus> GetReconstructionStatusAsync(long sceneId, CancellationToken cancellationToken = default)
    {
        if (_statusCache.TryGetValue(sceneId, out var status))
        {
            if (status.IsProcessing && status.StartedAt.HasValue)
            {
                status.ElapsedTime = DateTime.Now - status.StartedAt.Value;
            }
            return Task.FromResult(status);
        }
        return Task.FromResult(new ReconstructionStatus { SceneId = sceneId, IsProcessing = false });
    }

    /// <summary>
    /// 取消重建任务
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public Task CancelReconstructionAsync(long sceneId, CancellationToken cancellationToken = default)
    {
        if (_statusCache.ContainsKey(sceneId))
        {
            _statusCache[sceneId] = new ReconstructionStatus
            {
                SceneId = sceneId,
                IsProcessing = false,
                Progress = 0,
                StatusMessage = "已取消"
            };
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 转换为GLB格式
    /// </summary>
    /// <param name="inputPath">输入文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>输出文件路径</returns>
    public Task<string> ConvertToGlbAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var objPath = outputPath.Replace(".glb", ".obj");
            var mtlPath = outputPath.Replace(".glb", ".mtl");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _colmapPath,
                    Arguments = $"model_converter --input_path {inputPath} --output_path {objPath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (File.Exists(objPath))
            {
                File.Copy(objPath, outputPath, true);
            }

            return Task.FromResult(outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转换GLB格式失败: {InputPath}", inputPath);
            return Task.FromResult(inputPath);
        }
    }

    /// <summary>
    /// 更新重建状态
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="isProcessing">是否正在处理</param>
    /// <param name="progress">进度百分比</param>
    /// <param name="message">状态消息</param>
    /// <param name="startedAt">开始时间</param>
    private void UpdateStatus(long sceneId, bool isProcessing, int progress, string message, DateTime? startedAt)
    {
        if (!_statusCache.ContainsKey(sceneId))
        {
            _statusCache[sceneId] = new ReconstructionStatus { SceneId = sceneId };
        }

        var status = _statusCache[sceneId];
        status.IsProcessing = isProcessing;
        status.Progress = progress;
        status.StatusMessage = message;
        status.StartedAt = startedAt ?? status.StartedAt;
        status.CurrentStep = message;
        status.ElapsedTime = startedAt.HasValue ? DateTime.Now - startedAt.Value : null;
    }
}
