using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    private readonly IRepository<Certificate> _repo;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(IRepository<Certificate> repo, IEncryptionService encryption, ILogger<CertificateService> logger)
    {
        _repo = repo;
        _encryption = encryption;
        _logger = logger;
    }

    public async Task<List<Certificate>> GetByMemberAsync(long? memberId, CancellationToken cancellationToken = default)
    {
        var certs = await _repo.GetAllAsync(cancellationToken);
        if (memberId is not null) certs = certs.Where(c => c.MemberId == memberId).ToList();
        return certs.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<Certificate?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<Certificate> AddAsync(Certificate certificate, byte[]? fileBytes = null, CancellationToken cancellationToken = default)
    {
        if (fileBytes is not null && fileBytes.Length > 0)
        {
            certificate.EncryptedData = _encryption.EncryptBytes(fileBytes);
            certificate.FileSize = fileBytes.Length;
        }
        await _repo.AddAsync(certificate, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增证件: {Name} ({Type})", certificate.Name, certificate.Type);
        return certificate;
    }

    public async Task<(byte[] Content, string FileName)?> GetFileAsync(long id, CancellationToken cancellationToken = default)
    {
        var cert = await _repo.GetByIdAsync(id, cancellationToken);
        if (cert?.EncryptedData is null) return null;
        var bytes = _encryption.DecryptBytes(cert.EncryptedData);
        return (bytes, cert.FileName ?? "certificate");
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var cert = await _repo.GetByIdAsync(id, cancellationToken);
        if (cert is null) return;
        cert.IsDeleted = true;
        cert.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
    }
}
