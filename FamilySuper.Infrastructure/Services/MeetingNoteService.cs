using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class MeetingNoteService : IMeetingNoteService
{
    private readonly IRepository<MeetingNote> _repo;
    private readonly ILogger<MeetingNoteService> _logger;

    public MeetingNoteService(IRepository<MeetingNote> repo, ILogger<MeetingNoteService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<MeetingNote>> GetNotesAsync(string? status = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var notes = await _repo.GetAllAsync(cancellationToken);
        if (!string.IsNullOrEmpty(status))
            notes = notes.Where(n => n.Status == status).ToList();
        if (from.HasValue)
            notes = notes.Where(n => n.MeetingDate >= from.Value).ToList();
        if (to.HasValue)
            notes = notes.Where(n => n.MeetingDate <= to.Value).ToList();
        return notes.OrderByDescending(n => n.MeetingDate).ToList();
    }

    public async Task<MeetingNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<MeetingNote> AddAsync(MeetingNote note, CancellationToken cancellationToken = default)
    {
        note.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(note, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增会议记录:{Title}", note.Title);
        return note;
    }

    public async Task UpdateAsync(MeetingNote note, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(note.Id, cancellationToken)
            ?? throw new InvalidOperationException("会议记录不存在");
        existing.Title = note.Title;
        existing.MeetingDate = note.MeetingDate;
        existing.DurationMinutes = note.DurationMinutes;
        existing.Attendees = note.Attendees;
        existing.Agenda = note.Agenda;
        existing.Notes = note.Notes;
        existing.ActionItems = note.ActionItems;
        existing.Location = note.Location;
        existing.Status = note.Status;
        existing.MemberId = note.MemberId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var note = await _repo.GetByIdAsync(id, cancellationToken);
        if (note is null) return;
        await _repo.DeleteAsync(note, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}