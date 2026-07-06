using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using TaskStatus = FamilySuper.Core.Enums.TaskStatus;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class WorkTaskService : IWorkTaskService
{
    private readonly IRepository<WorkTask> _repo;
    private readonly ILogger<WorkTaskService> _logger;

    public WorkTaskService(IRepository<WorkTask> repo, ILogger<WorkTaskService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<WorkTask>> GetTasksAsync(TaskStatus? status = null, TaskPriority? priority = null, CancellationToken cancellationToken = default)
    {
        var tasks = await _repo.GetAllAsync(cancellationToken);
        if (status is not null) tasks = tasks.Where(t => t.Status == status).ToList();
        if (priority is not null) tasks = tasks.Where(t => t.Priority == priority).ToList();
        return tasks.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).ToList();
    }

    public async Task<WorkTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<WorkTask> AddAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        await _repo.AddAsync(task, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增任务: {Title} (优先级 {Priority})", task.Title, task.Priority);
        return task;
    }

    public async Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        task.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(task, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(long id, TaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await _repo.GetByIdAsync(id, cancellationToken);
        if (task is null) return;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        if (status == TaskStatus.Completed) task.CompletedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("任务 {Id} 状态更新为 {Status}", id, status);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var task = await _repo.GetByIdAsync(id, cancellationToken);
        if (task is null) return;
        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
    }
}
