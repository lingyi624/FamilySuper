using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;

namespace FamilySuper.Infrastructure.Services;

public class ElderlyCareService : IElderlyCareService
{
    private readonly IRepository<EmergencyContact> _contactRepo;
    private readonly IRepository<HealthMetric> _metricRepo;

    public ElderlyCareService(IRepository<EmergencyContact> contactRepo, IRepository<HealthMetric> metricRepo)
    {
        _contactRepo = contactRepo;
        _metricRepo = metricRepo;
    }

    public async Task<List<EmergencyContact>> GetContactsAsync(long? memberId = null)
    {
        var contacts = await _contactRepo.GetAllAsync();
        if (memberId.HasValue)
            contacts = contacts.Where(c => c.MemberId == memberId.Value).ToList();
        return contacts.OrderBy(c => c.IsPrimary ? 0 : 1).ThenBy(c => c.Name).ToList();
    }

    public async Task<EmergencyContact?> GetContactByIdAsync(long id)
        => await _contactRepo.GetByIdAsync(id);

    public async Task<EmergencyContact> AddContactAsync(EmergencyContact contact)
    {
        contact.CreatedAt = DateTime.UtcNow;
        await _contactRepo.AddAsync(contact);
        await _contactRepo.SaveChangesAsync();
        return contact;
    }

    public async Task<EmergencyContact> UpdateContactAsync(EmergencyContact contact)
    {
        var existing = await _contactRepo.GetByIdAsync(contact.Id)
            ?? throw new InvalidOperationException($"EmergencyContact {contact.Id} not found");
        existing.Name = contact.Name;
        existing.Relationship = contact.Relationship;
        existing.PhoneNumber = contact.PhoneNumber;
        existing.AlternatePhone = contact.AlternatePhone;
        existing.IsPrimary = contact.IsPrimary;
        existing.Notes = contact.Notes;
        existing.MemberId = contact.MemberId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _contactRepo.UpdateAsync(existing);
        await _contactRepo.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteContactAsync(long id)
    {
        var contact = await _contactRepo.GetByIdAsync(id);
        if (contact is not null)
        {
            await _contactRepo.DeleteAsync(contact);
            await _contactRepo.SaveChangesAsync();
        }
    }

    public async Task<List<HealthMetric>> GetMetricsAsync(long? memberId = null, DateTime? from = null, DateTime? to = null)
    {
        var metrics = await _metricRepo.GetAllAsync();
        if (memberId.HasValue)
            metrics = metrics.Where(m => m.MemberId == memberId.Value).ToList();
        if (from.HasValue)
            metrics = metrics.Where(m => m.RecordDate >= from.Value).ToList();
        if (to.HasValue)
            metrics = metrics.Where(m => m.RecordDate <= to.Value).ToList();
        return metrics.OrderByDescending(m => m.RecordDate).ToList();
    }

    public async Task<HealthMetric?> GetMetricByIdAsync(long id)
        => await _metricRepo.GetByIdAsync(id);

    public async Task<HealthMetric> AddMetricAsync(HealthMetric metric)
    {
        metric.CreatedAt = DateTime.UtcNow;
        await _metricRepo.AddAsync(metric);
        await _metricRepo.SaveChangesAsync();
        return metric;
    }

    public async Task DeleteMetricAsync(long id)
    {
        var metric = await _metricRepo.GetByIdAsync(id);
        if (metric is not null)
        {
            await _metricRepo.DeleteAsync(metric);
            await _metricRepo.SaveChangesAsync();
        }
    }
}