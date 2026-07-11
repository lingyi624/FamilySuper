using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Color = MudBlazor.Color;
using FamilySuper.Host.WPF.Components.Dialogs;
using FamilySuper.Host.WPF.Components;
using System.Timers;
using System.IO;
using Microsoft.JSInterop;

namespace FamilySuper.Host.WPF.Pages;

public partial class VirtualScene : ComponentBase, IAsyncDisposable
{
    [Inject] private IVirtualSceneService SceneSvc { get; set; } = default!;
    [Inject] private IAnnotationService AnnotationSvc { get; set; } = default!;
    [Inject] private IModelReconstructionService ReconstructionSvc { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    private SceneViewer? sceneViewer;
    private List<FamilySuper.Core.Entities.VirtualScene> scenes = new();
    private List<Annotation> annotations = new();
    private FamilySuper.Core.Entities.VirtualScene? selectedScene;
    private string? filterType;

    private int reconstructionProgress;
    private string reconstructionStatus = string.Empty;
    private string reconstructionStep = string.Empty;
    private string reconstructionTime = "00:00:00";
    private bool isReconstructing;
    private System.Timers.Timer? statusTimer;
    private bool _isDisposed;

    private string? ModelPath => selectedScene?.ModelPath;

    protected override async Task OnInitializedAsync() => await LoadScenes();

    private async Task LoadScenes() => scenes = await SceneSvc.GetScenesAsync(filterType);

    private async Task SelectScene(FamilySuper.Core.Entities.VirtualScene scene)
    {
        selectedScene = scene;
        await LoadAnnotations(scene.Id);
        StateHasChanged();
    }

    private async Task LoadAnnotations(long sceneId)
    {
        annotations = await AnnotationSvc.GetAnnotationsBySceneAsync(sceneId);
    }

    private static Color GetSceneTypeColor(string type) => type switch
    {
        "家乡" => Color.Primary,
        "旅游" => Color.Info,
        "故居" => Color.Warning,
        "地标" => Color.Success,
        _ => Color.Default
    };

    private static Color GetReconstructionStatusColor(string status) => status switch
    {
        "processing" => Color.Warning,
        "completed" => Color.Success,
        "failed" => Color.Error,
        _ => Color.Default
    };

    private static string GetReconstructionStatusText(string status) => status switch
    {
        "processing" => "重建中",
        "completed" => "已完成",
        "failed" => "失败",
        _ => "待处理"
    };

    private async Task StartAdd()
    {
        var scene = new FamilySuper.Core.Entities.VirtualScene { SceneType = "家乡" };
        var parameters = new DialogParameters { ["Scene"] = scene };
        var dialog = await DialogService.ShowAsync<SceneEditDialog>("添加场景", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedScene = (FamilySuper.Core.Entities.VirtualScene)result.Data;
            await SceneSvc.AddAsync(savedScene);
            await LoadScenes();
            Snackbar.Add("场景已添加", Severity.Success);
        }
    }

    private async Task StartEdit(FamilySuper.Core.Entities.VirtualScene scene)
    {
        var editScene = new FamilySuper.Core.Entities.VirtualScene
        {
            Id = scene.Id, Title = scene.Title, Location = scene.Location,
            Latitude = scene.Latitude, Longitude = scene.Longitude,
            Description = scene.Description, ImagePaths = scene.ImagePaths,
            SceneType = scene.SceneType, PanoramaPath = scene.PanoramaPath,
            ModelPath = scene.ModelPath, Notes = scene.Notes, MemberId = scene.MemberId,
            PhotoCount = scene.PhotoCount, PhotoDirectory = scene.PhotoDirectory,
            ReconstructionStatus = scene.ReconstructionStatus,
            ReconstructedAt = scene.ReconstructedAt,
            ReconstructionProgress = scene.ReconstructionProgress
        };
        var parameters = new DialogParameters { ["Scene"] = editScene };
        var dialog = await DialogService.ShowAsync<SceneEditDialog>("编辑场景", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedScene = (FamilySuper.Core.Entities.VirtualScene)result.Data;
            await SceneSvc.UpdateAsync(savedScene);
            await LoadScenes();
            Snackbar.Add("场景已更新", Severity.Success);
        }
    }

    private async Task Delete(long id)
    {
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("确认删除");
        var result = await dialog.Result;
        if (result.Canceled) return;

        try
        {
            await SceneSvc.DeleteAsync(id);
            await LoadScenes();
            if (selectedScene?.Id == id)
            {
                selectedScene = null;
                annotations.Clear();
            }
            Snackbar.Add("场景已删除", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"删除失败: {ex.Message}", Severity.Error);
        }
    }

    private async Task ShowAnnotationForm()
    {
        var annotation = new Annotation
        {
            SceneId = selectedScene!.Id,
            Color = "#ff0000"
        };
        var parameters = new DialogParameters { ["Annotation"] = annotation };
        var dialog = await DialogService.ShowAsync<AnnotationEditDialog>("添加标注", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedAnnotation = (Annotation)result.Data;
            var newAnnotation = await AnnotationSvc.AddAsync(savedAnnotation);
            annotations.Add(newAnnotation);
            sceneViewer?.AddAnnotation(newAnnotation);
            Snackbar.Add("标注已添加", Severity.Success);
        }
    }

    private async Task EditAnnotation(Annotation annotation)
    {
        var editAnnotation = new Annotation
        {
            Id = annotation.Id,
            SceneId = annotation.SceneId,
            PositionX = annotation.PositionX,
            PositionY = annotation.PositionY,
            PositionZ = annotation.PositionZ,
            Text = annotation.Text,
            Color = annotation.Color,
            ImageUrl = annotation.ImageUrl,
            MemoryText = annotation.MemoryText,
            MemoryDate = annotation.MemoryDate,
            RelatedMediaPath = annotation.RelatedMediaPath
        };
        var parameters = new DialogParameters { ["Annotation"] = editAnnotation };
        var dialog = await DialogService.ShowAsync<AnnotationEditDialog>("编辑标注", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedAnnotation = (Annotation)result.Data;
            await AnnotationSvc.UpdateAsync(savedAnnotation);
            var index = annotations.FindIndex(a => a.Id == savedAnnotation.Id);
            if (index != -1) annotations[index] = savedAnnotation;
            sceneViewer?.RemoveAnnotation(savedAnnotation.Id);
            sceneViewer?.AddAnnotation(savedAnnotation);
            Snackbar.Add("标注已更新", Severity.Success);
        }
    }

    private async Task DeleteAnnotation(long id)
    {
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("确认删除标注");
        var result = await dialog.Result;
        if (result.Canceled) return;

        try
        {
            await AnnotationSvc.DeleteAsync(id);
            annotations.RemoveAll(a => a.Id == id);
            sceneViewer?.RemoveAnnotation(id);
            Snackbar.Add("标注已删除", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"删除失败: {ex.Message}", Severity.Error);
        }
    }

    private async Task ReloadModel()
    {
        if (selectedScene != null && !string.IsNullOrEmpty(selectedScene.ModelPath))
        {
            await LoadAnnotations(selectedScene.Id);
            StateHasChanged();
        }
    }

    private async Task HandleAnnotationClick(AnnotationPosition position)
    {
        var annotation = new Annotation
        {
            SceneId = selectedScene!.Id,
            PositionX = position.X,
            PositionY = position.Y,
            PositionZ = position.Z,
            Color = "#ff0000"
        };
        var parameters = new DialogParameters { ["Annotation"] = annotation };
        var dialog = await DialogService.ShowAsync<AnnotationEditDialog>("添加标注", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var savedAnnotation = (Annotation)result.Data;
            var newAnnotation = await AnnotationSvc.AddAsync(savedAnnotation);
            annotations.Add(newAnnotation);
            sceneViewer?.AddAnnotation(newAnnotation);
            Snackbar.Add("标注已添加", Severity.Success);
        }
    }

    private void HandleModelLoaded(ModelInfo info)
    {
    }

    private async Task ShowUploadDialog()
    {
        var dialog = await DialogService.ShowAsync<PhotoUploadDialog>("上传照片");
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            var files = (List<IBrowserFile>)result.Data;
            await UploadPhotos(files);
        }
    }

    private async Task UploadPhotos(List<IBrowserFile> files)
    {
        if (selectedScene == null || files.Count == 0) return;

        try
        {
            foreach (var file in files)
            {
                using (var stream = file.OpenReadStream())
                {
                    await ReconstructionSvc.UploadPhotoFromStreamAsync(selectedScene.Id, stream, file.Name);
                }
            }

            await LoadScenes();
            Snackbar.Add($"成功上传 {files.Count} 张照片", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"上传失败: {ex.Message}", Severity.Error);
        }
    }

    private async Task StartReconstruction()
    {
        if (selectedScene == null) return;

        reconstructionProgress = 0;
        reconstructionStatus = "准备开始...";
        reconstructionStep = string.Empty;
        reconstructionTime = "00:00:00";
        isReconstructing = true;

        StartStatusTimer();

        _ = Task.Run(async () =>
        {
            try
            {
                selectedScene.ReconstructionStatus = "processing";
                await SceneSvc.UpdateAsync(selectedScene);

                var result = await ReconstructionSvc.ReconstructModelAsync(selectedScene.Id);

                if (result.Success)
                {
                    selectedScene.ModelPath = result.ModelPath;
                    selectedScene.ReconstructionStatus = "completed";
                    selectedScene.ReconstructedAt = DateTime.UtcNow;
                    selectedScene.ReconstructionProgress = 100;
                    reconstructionProgress = 100;
                    reconstructionStatus = "重建完成!";

                    Snackbar.Add("模型重建成功!", Severity.Success);
                }
                else
                {
                    selectedScene.ReconstructionStatus = "failed";
                    reconstructionStatus = $"重建失败: {result.ErrorMessage}";

                    if (string.IsNullOrEmpty(result.ErrorMessage) || result.ErrorMessage.Contains("COLMAP"))
                    {
                        Snackbar.Add("COLMAP未安装或不可用，无法进行自动重建。请安装COLMAP后重试，或手动上传GLB模型文件。", Severity.Warning);
                    }
                    else
                    {
                        Snackbar.Add($"重建失败: {result.ErrorMessage}", Severity.Error);
                    }
                }

                await SceneSvc.UpdateAsync(selectedScene);
                await LoadScenes();
            }
            catch (Exception ex)
            {
                selectedScene.ReconstructionStatus = "failed";
                reconstructionStatus = $"重建失败: {ex.Message}";
                Snackbar.Add($"重建失败: {ex.Message}", Severity.Error);
            }
            finally
            {
                isReconstructing = false;
                StopStatusTimer();
            }
        });
    }

    private void StartStatusTimer()
    {
        statusTimer = new System.Timers.Timer(1000);
        statusTimer.Elapsed += async (sender, e) => await UpdateReconstructionStatus();
        statusTimer.Start();
    }

    private void StopStatusTimer()
    {
        statusTimer?.Stop();
        statusTimer?.Dispose();
        statusTimer = null;
    }

    private async Task UpdateReconstructionStatus()
    {
        if (_isDisposed || selectedScene == null) return;

        var status = await ReconstructionSvc.GetReconstructionStatusAsync(selectedScene.Id);
        if (status.IsProcessing)
        {
            reconstructionProgress = status.Progress;
            reconstructionStep = status.CurrentStep ?? string.Empty;
            reconstructionStatus = status.StatusMessage ?? "处理中...";
            reconstructionTime = status.ElapsedTime?.ToString(@"hh\:mm\:ss") ?? "00:00:00";
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task CancelReconstruction()
    {
        if (selectedScene == null) return;

        await ReconstructionSvc.CancelReconstructionAsync(selectedScene.Id);
        selectedScene.ReconstructionStatus = "cancelled";
        await SceneSvc.UpdateAsync(selectedScene);

        isReconstructing = false;
        StopStatusTimer();

        Snackbar.Add("重建已取消", Severity.Info);
        await LoadScenes();
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        StopStatusTimer();
        await Task.CompletedTask;
    }
}