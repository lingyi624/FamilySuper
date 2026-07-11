using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Color = MudBlazor.Color;
using FamilySuper.Host.WPF.Components.Dialogs;

namespace FamilySuper.Host.WPF.Pages;

public partial class MedicalGuide : ComponentBase
{
    [Inject] private IMedicalGuideService MedicalSvc { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private List<FamilySuper.Core.Entities.MedicalGuide> guides = new();
    private FamilySuper.Core.Entities.MedicalGuide editing = new();
    private bool showForm;
    private MudForm _form = default!;
    private bool _isFormValid;
    private string? filterCategory;

    protected override async Task OnInitializedAsync() => await LoadGuides();

    private async Task LoadGuides() => guides = await MedicalSvc.GetGuidesAsync(filterCategory);

    private static Color GetCategoryColor(string category) => category switch
    {
        "综合医院" => Color.Primary,
        "专科医院" => Color.Info,
        "社区诊所" => Color.Success,
        "药房" => Color.Warning,
        _ => Color.Default
    };

    private void StartAdd()
    {
        editing = new FamilySuper.Core.Entities.MedicalGuide { Category = "综合医院", Rating = 3 };
        showForm = true;
    }

    private void StartEdit(FamilySuper.Core.Entities.MedicalGuide guide)
    {
        editing = new FamilySuper.Core.Entities.MedicalGuide
        {
            Id = guide.Id, Title = guide.Title, HospitalName = guide.HospitalName,
            Department = guide.Department, DoctorName = guide.DoctorName,
            Address = guide.Address, Phone = guide.Phone, Rating = guide.Rating,
            Category = guide.Category, Description = guide.Description,
            RegistrationMethod = guide.RegistrationMethod, Notes = guide.Notes, MemberId = guide.MemberId
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
                await MedicalSvc.UpdateAsync(editing);
                Snackbar.Add("就医指南已更新", Severity.Success);
            }
            else
            {
                await MedicalSvc.AddAsync(editing);
                Snackbar.Add("就医指南已添加", Severity.Success);
            }
            showForm = false;
            await LoadGuides();
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
            await MedicalSvc.DeleteAsync(id);
            await LoadGuides();
            Snackbar.Add("就医指南已删除", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"删除失败: {ex.Message}", Severity.Error);
        }
    }
}