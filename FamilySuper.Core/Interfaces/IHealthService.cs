using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

public interface IHealthService
{
    Task<List<HealthRecord>> GetRecordsAsync(long? memberId = null, HealthRecordType? type = null, CancellationToken cancellationToken = default);
    Task<HealthRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<HealthRecord> AddAsync(HealthRecord record, CancellationToken cancellationToken = default);
    Task UpdateAsync(HealthRecord record, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
