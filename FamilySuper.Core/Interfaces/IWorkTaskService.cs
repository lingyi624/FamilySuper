using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

public interface IWorkTaskService
{
    Task<List<WorkTask>> GetTasksAsync(Enums.TaskStatus? status = null, TaskPriority? priority = null, CancellationToken cancellationToken = default);
    Task<WorkTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkTask> AddAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(long id, Enums.TaskStatus status, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
