using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class PriceCompareService : IPriceCompareService
{
    private readonly IRepository<PriceCompare> _repo;
    private readonly ILogger<PriceCompareService> _logger;

    public PriceCompareService(IRepository<PriceCompare> repo, ILogger<PriceCompareService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<PriceCompare>> GetItemsAsync(string? productName = null, string? category = null, CancellationToken cancellationToken = default)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        if (!string.IsNullOrEmpty(productName))
            items = items.Where(i => i.ProductName.Contains(productName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrEmpty(category))
            items = items.Where(i => i.Category == category).ToList();
        return items.OrderByDescending(i => i.CompareDate).ToList();
    }

    public async Task<PriceCompare?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<PriceCompare> AddAsync(PriceCompare item, CancellationToken cancellationToken = default)
    {
        item.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(item, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增比价:{ProductName}", item.ProductName);
        return item;
    }

    public async Task UpdateAsync(PriceCompare item, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(item.Id, cancellationToken)
            ?? throw new InvalidOperationException("比价记录不存在");
        existing.ProductName = item.ProductName;
        existing.Category = item.Category;
        existing.StoreName = item.StoreName;
        existing.Price = item.Price;
        existing.Unit = item.Unit;
        existing.Url = item.Url;
        existing.IsLowest = item.IsLowest;
        existing.CompareDate = item.CompareDate;
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

    public async Task<List<PriceCompare>> GetLowestPricesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        return items
            .GroupBy(i => i.ProductName)
            .Select(g => g.OrderBy(i => i.Price).First())
            .OrderByDescending(i => i.CompareDate)
            .ToList();
    }
}