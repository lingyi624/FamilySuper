using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class MedicalGuideService : IMedicalGuideService
{
    private readonly IRepository<MedicalGuide> _repo;
    private readonly ILogger<MedicalGuideService> _logger;

    public MedicalGuideService(IRepository<MedicalGuide> repo, ILogger<MedicalGuideService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<MedicalGuide>> GetGuidesAsync(string? category = null, string? department = null, CancellationToken cancellationToken = default)
    {
        var guides = await _repo.GetAllAsync(cancellationToken);
        if (!string.IsNullOrEmpty(category))
            guides = guides.Where(g => g.Category == category).ToList();
        if (!string.IsNullOrEmpty(department))
            guides = guides.Where(g => g.Department == department).ToList();
        return guides.OrderByDescending(g => g.Rating).ThenByDescending(g => g.CreatedAt).ToList();
    }

    public async Task<MedicalGuide?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<MedicalGuide> AddAsync(MedicalGuide guide, CancellationToken cancellationToken = default)
    {
        guide.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(guide, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增就医指南:{Title}", guide.Title);
        return guide;
    }

    public async Task UpdateAsync(MedicalGuide guide, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(guide.Id, cancellationToken)
            ?? throw new InvalidOperationException("就医指南不存在");
        existing.Title = guide.Title;
        existing.HospitalName = guide.HospitalName;
        existing.Department = guide.Department;
        existing.DoctorName = guide.DoctorName;
        existing.Address = guide.Address;
        existing.Phone = guide.Phone;
        existing.Rating = guide.Rating;
        existing.Category = guide.Category;
        existing.Description = guide.Description;
        existing.RegistrationMethod = guide.RegistrationMethod;
        existing.Notes = guide.Notes;
        existing.MemberId = guide.MemberId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var guide = await _repo.GetByIdAsync(id, cancellationToken);
        if (guide is null) return;
        await _repo.DeleteAsync(guide, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}