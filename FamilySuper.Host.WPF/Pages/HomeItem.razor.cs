using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Color = MudBlazor.Color;
using FamilySuper.Host.WPF.Components.Dialogs;

namespace FamilySuper.Host.WPF.Pages;

public partial class HomeItem : ComponentBase
{
    [Inject] private IHomeItemService HomeSvc { get; set; } = default!;
    [Inject] private IRepository<FamilyMember> MemberRepo { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<FamilySuper.Core.Entities.HomeItem> items = new();
    private FamilySuper.Core.Entities.HomeItem editing = new();
    private bool showForm;
    private MudForm _form = default!;
    private bool _isFormValid;
    private string? filterCategory;
    private string? filterStatus;

    protected override async Task OnInitializedAsync() => await LoadItems();

    private async Task LoadItems() => items = await HomeSvc.GetItemsAsync(filterCategory, filterStatus);

    private static Color GetStatusColor(string status) => status switch
    {
        "正常" => Color.Success,
        "维修中" => Color.Warning,
        _ => Color.Default
    };

    private void StartAdd()
    {
        editing = new FamilySuper.Core.Entities.HomeItem { Quantity = 1, Status = "正常", Category = "其他" };
        showForm = true;
    }

    private void StartEdit(FamilySuper.Core.Entities.HomeItem item)
    {
        editing = new FamilySuper.Core.Entities.HomeItem
        {
            Id = item.Id, Name = item.Name, Category = item.Category, Location = item.Location,
            Quantity = item.Quantity, PurchaseDate = item.PurchaseDate, Price = item.Price,
            WarrantyExpiry = item.WarrantyExpiry, Status = item.Status, Brand = item.Brand,
            Notes = item.Notes, MemberId = item.MemberId
        };
        showForm = true;
    }

    private void CancelForm() => showForm = false;

    private async Task Save()
    {
        await _form.ValidateAsync();
        if (!_isFormValid) return;
        try
        {
            if (editing.Id > 0)
            {
                await HomeSvc.UpdateAsync(editing);
                Snackbar.Add("物品信息已更新", Severity.Success);
            }
            else
            {
                await HomeSvc.AddAsync(editing);
                Snackbar.Add("物品已添加", Severity.Success);
            }
            showForm = false;
            await LoadItems();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"保存失败: {ex.Message}", Severity.Error);
        }
    }

    private async Task Delete(long id)
    {
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("确认删除");
        var result = await dialog.Result;
        if (result.Canceled) return;

        try
        {
            await HomeSvc.DeleteAsync(id);
            await LoadItems();
            Snackbar.Add("物品已删除", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"删除失败: {ex.Message}", Severity.Error);
        }
    }
}