using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

public record ChatMessage(string Role, string Content, string? ImagePreview = null);

public interface IAgentService
{
    Task<string> ChatAsync(
        string userMessage,
        AppMode mode,
        List<ChatMessage> history,
        string? systemPromptOverride = null,
        string? reasoningEffort = null,
        CancellationToken cancellationToken = default);

    Task<string> ChatWithImageAsync(
        string userMessage,
        byte[] imageBytes,
        string imageMimeType,
        AppMode mode,
        List<ChatMessage> history,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default);

    bool IsAvailable { get; }
}
