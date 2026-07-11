using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class HomeItemService : IHomeItemService
{
    private readonly IRepository<HomeItem> _repo;
    private readonly ILogger<HomeItemService> _logger;

    public HomeItemService(IRepository<HomeItem> repo, ILogger<HomeItemService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<HomeItem>> GetItemsAsync(string? category = null, string? status = null, string? location = null, CancellationToken cancellationToken = default)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        if (!string.IsNullOrEmpty(category))
            items = items.Where(i => i.Category == category).ToList();
        if (!string.IsNullOrEmpty(status))
            items = items.Where(i => i.Status == status).ToList();
        if (!string.IsNullOrEmpty(location))
            items = items.Where(i => i.Location == location).ToList();
        return items.OrderByDescending(i => i.CreatedAt).ToList();
    }

    public async Task<HomeItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<HomeItem> AddAsync(HomeItem item, CancellationToken cancellationToken = default)
    {
        item.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(item, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增物品:{Name}", item.Name);
        return item;
    }

    public async Task UpdateAsync(HomeItem item, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(item.Id, cancellationToken)
            ?? throw new InvalidOperationException("物品不存在");
        existing.Name = item.Name;
        existing.Category = item.Category;
        existing.Location = item.Location;
        existing.Quantity = item.Quantity;
        existing.PurchaseDate = item.PurchaseDate;
        existing.Price = item.Price;
        existing.WarrantyExpiry = item.WarrantyExpiry;
        existing.Status = item.Status;
        existing.Brand = item.Brand;
        existing.Notes = item.Notes;
        existing.MemberId = item.MemberId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repo.GetByIdAsync(id, cancellationToken);
        if (item is null) return;
        await _repo.DeleteAsync(item, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}