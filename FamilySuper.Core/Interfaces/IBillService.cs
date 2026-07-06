using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public interface IBillService
{
    Task<List<BillReminder>> GetBillsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<BillReminder?> GetBillByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<BillReminder> AddBillAsync(BillReminder bill, CancellationToken cancellationToken = default);
    Task UpdateBillAsync(BillReminder bill, CancellationToken cancellationToken = default);
    Task DeleteBillAsync(long id, CancellationToken cancellationToken = default);
    Task MarkPaidAsync(long id, CancellationToken cancellationToken = default);
    Task<List<BillReminder>> GetDueSoonAsync(int days = 7, CancellationToken cancellationToken = default);
}
