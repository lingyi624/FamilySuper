using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class VirtualSceneService : IVirtualSceneService
{
    private readonly IRepository<VirtualScene> _repo;
    private readonly ILogger<VirtualSceneService> _logger;

    public VirtualSceneService(IRepository<VirtualScene> repo, ILogger<VirtualSceneService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<VirtualScene>> GetScenesAsync(string? sceneType = null, CancellationToken cancellationToken = default)
    {
        var scenes = await _repo.GetAllAsync(cancellationToken);
        if (!string.IsNullOrEmpty(sceneType))
            scenes = scenes.Where(s => s.SceneType == sceneType).ToList();
        return scenes.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task<VirtualScene?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<VirtualScene> AddAsync(VirtualScene scene, CancellationToken cancellationToken = default)
    {
        scene.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(scene, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增场景:{Title}", scene.Title);
        return scene;
    }

    public async Task UpdateAsync(VirtualScene scene, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(scene.Id, cancellationToken)
            ?? throw new InvalidOperationException("场景不存在");
        existing.Title = scene.Title;
        existing.Location = scene.Location;
        existing.Latitude = scene.Latitude;
        existing.Longitude = scene.Longitude;
        existing.Description = scene.Description;
        existing.ImagePaths = scene.ImagePaths;
        existing.SceneType = scene.SceneType;
        existing.PanoramaPath = scene.PanoramaPath;
        existing.ModelPath = scene.ModelPath;
        existing.Notes = scene.Notes;
        existing.MemberId = scene.MemberId;
        existing.PhotoCount = scene.PhotoCount;
        existing.PhotoDirectory = scene.PhotoDirectory;
        existing.ReconstructionStatus = scene.ReconstructionStatus;
        existing.ReconstructedAt = scene.ReconstructedAt;
        existing.ReconstructionProgress = scene.ReconstructionProgress;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var scene = await _repo.GetByIdAsync(id, cancellationToken);
        if (scene is null) return;
        await _repo.DeleteAsync(scene, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}