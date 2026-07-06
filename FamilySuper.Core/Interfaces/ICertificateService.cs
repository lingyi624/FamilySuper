using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public interface ICertificateService
{
    Task<List<Certificate>> GetByMemberAsync(long? memberId, CancellationToken cancellationToken = default);
    Task<Certificate?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Certificate> AddAsync(Certificate certificate, byte[]? fileBytes = null, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)?> GetFileAsync(long id, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
