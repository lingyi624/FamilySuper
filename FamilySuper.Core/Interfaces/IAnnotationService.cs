using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public interface IAnnotationService
{
    Task<List<Annotation>> GetAnnotationsBySceneAsync(long sceneId, CancellationToken cancellationToken = default);

    Task<Annotation?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Annotation> AddAsync(Annotation annotation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Annotation annotation, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}