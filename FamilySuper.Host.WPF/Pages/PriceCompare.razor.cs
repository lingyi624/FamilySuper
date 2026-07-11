using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using FamilySuper.Host.WPF.Components.Dialogs;

namespace FamilySuper.Host.WPF.Pages;

public partial class PriceCompare : ComponentBase
{
    [Inject] private IPriceCompareService PriceSvc { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<FamilySuper.Core.Entities.PriceCompare> items = new();
    private FamilySuper.Core.Entities.PriceCompare editing = new();
    private bool showForm;
    private MudForm _form = default!;
    private bool _isFormValid;
    private string? searchText;
    private DateTime? compareDateValue;

    protected override async Task OnInitializedAsync() => await LoadItems();

    private async Task LoadItems() => items = await PriceSvc.GetItemsAsync(searchText);

    private async Task LoadLowestPrices() => items = await PriceSvc.GetLowestPricesAsync();

    private void StartAdd()
    {
        editing = new FamilySuper.Core.Entities.PriceCompare { CompareDate = DateTime.UtcNow };
        compareDateValue = editing.CompareDate;
        showForm = true;
    }

    private void StartEdit(FamilySuper.Core.Entities.PriceCompare item)
    {
        editing = new FamilySuper.Core.Entities.PriceCompare
        {
            Id = item.Id, ProductName = item.ProductName, Category = item.Category,
            StoreName = item.StoreName, Price = item.Price, Unit = item.Unit,
            Url = item.Url, IsLowest = item.IsLowest, CompareDate = item.CompareDate,
            Notes = item.Notes, MemberId = item.MemberId
        };
        compareDateValue = item.CompareDate;
        showForm = true;
    }

    private void CancelForm() => showForm = false;

    private async Task Save()
    {
        await _form.ValidateAsync();
        if (!_isFormValid) return;
        if (compareDateValue.HasValue) editing.CompareDate = compareDateValue.Value;
        try
        {
            if (editing.Id > 0)
            {
                await PriceSvc.UpdateAsync(editing);
                Snackbar.Add("比价记录已更新", Severity.Success);
            }
            else
            {
                await PriceSvc.AddAsync(editing);
                Snackbar.Add("比价记录已添加", Severity.Success);
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
            await PriceSvc.DeleteAsync(id);
            await LoadItems();
            Snackbar.Add("比价记录已删除", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"删除失败: {ex.Message}", Severity.Error);
        }
    }
}