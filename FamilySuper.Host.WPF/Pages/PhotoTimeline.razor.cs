using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Color = MudBlazor.Color;
using FamilySuper.Host.WPF.Components.Dialogs;

namespace FamilySuper.Host.WPF.Pages;

public partial class PhotoTimeline
{
    [Inject] private IPhotoTimelineService Service { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<FamilySuper.Core.Entities.PhotoTimeline> photos = new();
    private List<int> sortedYears = new();
    private Dictionary<int, int> yearStats = new();
    private List<string> categories = new();
    private int? selectedYear;
    private string? selectedCategory;

    private const string BasePhotoPath = "./data/photos";

    protected override async Task OnInitializedAsync()
    {
        await LoadPhotos();
        await LoadCategories();
        await LoadYearStats();
    }

    private async Task LoadPhotos()
    {
        photos = await Service.GetAllAsync();
        
        if (selectedYear.HasValue)
        {
            photos = photos.Where(p => (p.TakenDate ?? p.CreatedAt).Year == selectedYear.Value).ToList();
        }
        
        if (!string.IsNullOrEmpty(selectedCategory))
        {
            photos = photos.Where(p => p.Category == selectedCategory).ToList();
        }

        UpdateYears();
    }

    private async Task LoadCategories()
    {
        categories = await Service.GetCategoriesAsync();
    }

    private async Task LoadYearStats()
    {
        yearStats = await Service.GetYearStatsAsync();
        UpdateYears();
    }

    private void UpdateYears()
    {
        sortedYears = photos.Select(p => (p.TakenDate ?? p.CreatedAt).Year).Distinct().OrderByDescending(y => y).ToList();
    }

    private async Task SelectYear(int year)
    {
        selectedYear = selectedYear == year ? null : year;
        await LoadPhotos();
    }

    private async Task SelectCategory(string? category)
    {
        selectedCategory = category;
        await LoadPhotos();
    }

    private IEnumerable<IGrouping<int, FamilySuper.Core.Entities.PhotoTimeline>> photosGroupedByYear =>
        photos.GroupBy(p => (p.TakenDate ?? p.CreatedAt).Year)
            .OrderByDescending(g => g.Key);

    private string GetPhotoUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return string.Empty;
        
        if (filePath.StartsWith("http")) return filePath;
        
        var relativePath = filePath.Replace("./", "").Replace("\\", "/");
        return $"/{relativePath}";
    }

    private async Task StartAdd()
    {
        var photo = new FamilySuper.Core.Entities.PhotoTimeline { Category = "生活" };
        var parameters = new DialogParameters { ["Photo"] = photo };
        var dialog = await DialogService.ShowAsync<PhotoTimelineEditDialog>("添加照片", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedPhoto = (FamilySuper.Core.Entities.PhotoTimeline)result.Data;
            await Service.AddAsync(savedPhoto);
            await LoadPhotos();
            await LoadYearStats();
            Snackbar.Add("照片已添加", Severity.Success);
        }
    }

    private async Task StartEdit(FamilySuper.Core.Entities.PhotoTimeline photo)
    {
        var editPhoto = new FamilySuper.Core.Entities.PhotoTimeline
        {
            Id = photo.Id,
            Title = photo.Title,
            FilePath = photo.FilePath,
            ThumbnailPath = photo.ThumbnailPath,
            Description = photo.Description,
            TakenDate = photo.TakenDate,
            Location = photo.Location,
            Latitude = photo.Latitude,
            Longitude = photo.Longitude,
            PeopleTags = photo.PeopleTags,
            SceneTags = photo.SceneTags,
            Category = photo.Category,
            MemberId = photo.MemberId,
            IsCover = photo.IsCover,
            Width = photo.Width,
            Height = photo.Height,
            FileSize = photo.FileSize
        };
        var parameters = new DialogParameters { ["Photo"] = editPhoto };
        var dialog = await DialogService.ShowAsync<PhotoTimelineEditDialog>("编辑照片", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedPhoto = (FamilySuper.Core.Entities.PhotoTimeline)result.Data;
            await Service.UpdateAsync(savedPhoto);
            await LoadPhotos();
            Snackbar.Add("照片已更新", Severity.Success);
        }
    }

    private async Task Delete(long id)
    {
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("确认删除");
        var result = await dialog.Result;
        if (result.Canceled) return;

        try
        {
            await Service.DeleteAsync(id);
            await LoadPhotos();
            await LoadYearStats();
            Snackbar.Add("照片已删除", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"删除失败: {ex.Message}", Severity.Error);
        }
    }

    private void ShowPhotoDetail(FamilySuper.Core.Entities.PhotoTimeline photo)
    {
        var parameters = new DialogParameters
        {
            ["Photo"] = photo
        };
        _ = DialogService.ShowAsync<PhotoTimelineDetailDialog>("照片详情", parameters);
    }
}
