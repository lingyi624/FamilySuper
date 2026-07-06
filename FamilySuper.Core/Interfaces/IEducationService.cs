using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public interface IEducationService
{
    Task<List<EducationRecord>> GetRecordsAsync(long? memberId = null, string? subject = null, CancellationToken cancellationToken = default);
    Task<EducationRecord> AddQaAsync(string question, string answer, string? subject = null, long? memberId = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
