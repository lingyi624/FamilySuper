using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;

namespace FamilySuper.Infrastructure.Services;

public class ShoppingService : IShoppingService
{
    private readonly IRepository<ShoppingItem> _repo;

    public ShoppingService(IRepository<ShoppingItem> repo)
    {
        _repo = repo;
    }

    public async Task<List<ShoppingItem>> GetItemsAsync(ShoppingStatus? status = null, long? memberId = null)
    {
        var items = await _repo.GetAllAsync();
        if (status.HasValue)
            items = items.Where(x => x.Status == status.Value).ToList();
        if (memberId.HasValue)
            items = items.Where(x => x.MemberId == memberId.Value).ToList();
        return items.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task<ShoppingItem?> GetByIdAsync(long id)
        => await _repo.GetByIdAsync(id);

    public async Task<ShoppingItem> AddAsync(ShoppingItem item)
    {
        item.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();
        return item;
    }

    public async Task<ShoppingItem> UpdateAsync(ShoppingItem item)
    {
        var existing = await _repo.GetByIdAsync(item.Id)
            ?? throw new InvalidOperationException($"ShoppingItem {item.Id} not found");
        existing.Name = item.Name;
        existing.Category = item.Category;
        existing.Quantity = item.Quantity;
        existing.Unit = item.Unit;
        existing.EstimatedPrice = item.EstimatedPrice;
        existing.Status = item.Status;
        existing.Notes = item.Notes;
        existing.MemberId = item.MemberId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is not null)
        {
            await _repo.DeleteAsync(item);
            await _repo.SaveChangesAsync();
        }
    }

    public async Task MarkPurchasedAsync(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is not null)
        {
            item.Status = ShoppingStatus.Purchased;
            item.PurchasedDate = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(item);
            await _repo.SaveChangesAsync();
        }
    }

    public async Task<List<string>> SuggestPurchasesAsync()
    {
        var all = await _repo.GetAllAsync();
        var recentlyPurchased = all
            .Where(x => x.Status == ShoppingStatus.Purchased)
            .OrderByDescending(x => x.PurchasedDate)
            .Take(20)
            .Select(x => x.Name.ToLower())
            .ToList();

        var pending = all
            .Where(x => x.Status == ShoppingStatus.Pending)
            .Select(x => x.Name.ToLower())
            .ToList();

        return recentlyPurchased
            .Where(x => !pending.Contains(x))
            .Distinct()
            .Take(5)
            .ToList();
    }
}