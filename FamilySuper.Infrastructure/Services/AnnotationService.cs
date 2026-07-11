using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class AnnotationService : IAnnotationService
{
    private readonly IRepository<Annotation> _repo;
    private readonly ILogger<AnnotationService> _logger;

    public AnnotationService(IRepository<Annotation> repo, ILogger<AnnotationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<Annotation>> GetAnnotationsBySceneAsync(long sceneId, CancellationToken cancellationToken = default)
    {
        var annotations = await _repo.GetAllAsync(cancellationToken);
        return annotations.Where(a => a.SceneId == sceneId).OrderBy(a => a.CreatedAt).ToList();
    }

    public async Task<Annotation?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<Annotation> AddAsync(Annotation annotation, CancellationToken cancellationToken = default)
    {
        annotation.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(annotation, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增标注:{Id}", annotation.Id);
        return annotation;
    }

    public async Task UpdateAsync(Annotation annotation, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(annotation.Id, cancellationToken)
            ?? throw new InvalidOperationException("标注不存在");
        existing.Text = annotation.Text;
        existing.ImageUrl = annotation.ImageUrl;
        existing.Color = annotation.Color;
        existing.MemoryText = annotation.MemoryText;
        existing.MemoryDate = annotation.MemoryDate;
        existing.RelatedMediaPath = annotation.RelatedMediaPath;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var annotation = await _repo.GetByIdAsync(id, cancellationToken);
        if (annotation is null) return;
        await _repo.DeleteAsync(annotation, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}