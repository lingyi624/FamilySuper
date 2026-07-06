using FamilySuper.Core.Constants;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using FamilySuper.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FamilySuper.Agent;

public class AgentService : IAgentService
{
    private readonly Kernel _kernel;
    private readonly bool _available;
    private readonly ILogger<AgentService> _logger;
    private readonly IMemoryManager _memoryManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _defaultReasoningEffort;

    private const string AdultSystemPrompt = """
你是一个家庭管家智能体,帮助管理家庭各项事务,提供专业、贴心的建议。
你可以调用工具查询家庭信息、财务记录、健康档案等。
请用简洁、清晰、有条理的中文回答用户问题。
""";

    private const string ChildSystemPrompt = """
你是一个儿童教育助手,回答需安全、正面、适合未成年人。
严禁输出任何成人、财务、医疗或工作相关内容。
专注于教育、百科、安全知识、趣味问答等适合儿童的话题。
语言要生动、易懂、富有鼓励性。
""";

    public const string TutorSystemPrompt = AgentPrompts.TutorSystemPrompt;

    public AgentService(
        IConfiguration configuration,
        ILogger<AgentService> logger,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _memoryManager = serviceProvider.GetRequiredService<IMemoryManager>();
        _serviceProvider = serviceProvider;
        _defaultReasoningEffort = configuration["DeepSeek:ReasoningEffort"] ?? string.Empty;
        var (kernel, available) = KernelBuilder.Build(configuration, loggerFactory, serviceProvider);
        _kernel = kernel;
        _available = available;

        if (!_available)
        {
            _logger.LogWarning("DeepSeek API Key 未配置,智能体降级为 Mock 模式");
        }
        else
        {
            _logger.LogInformation("DeepSeek V4 Pro 智能体已就绪,插件已注册,记忆管理已集成");
        }
    }

    public bool IsAvailable => _available;

    public async Task<string> ChatWithImageAsync(
        string userMessage,
        byte[] imageBytes,
        string imageMimeType,
        AppMode mode,
        List<ChatMessage> history,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage) && imageBytes.Length == 0)
        {
            return "请输入您的问题或上传图片。";
        }

        var ocrText = string.Empty;
        try
        {
            var ocr = _serviceProvider.GetService<IOcrService>();
            if (ocr is not null && ocr.IsAvailable && imageBytes.Length > 0)
            {
                ocrText = await ocr.RecognizeTextAsync(imageBytes, cancellationToken);
                _logger.LogInformation("图片 OCR 完成,提取文本 {Length} 字符", ocrText.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "图片 OCR 失败,继续仅文字对话");
        }

        var composedMessage = string.IsNullOrWhiteSpace(ocrText)
            ? (string.IsNullOrWhiteSpace(userMessage)
                ? "(用户上传了一张图片,但 OCR 未识别到文字内容,请描述图片内容或询问用户需求)"
                : userMessage + "\n\n[附:用户上传了一张图片,但 OCR 未识别到文字内容]")
            : (string.IsNullOrWhiteSpace(userMessage)
                ? $"(用户上传了一张图片,OCR 识别内容如下)\n{ocrText}"
                : $"{userMessage}\n\n[附:图片 OCR 识别内容]\n{ocrText}");

        return await ChatAsync(composedMessage, mode, history, systemPromptOverride, null, cancellationToken);
    }

    public async Task<string> ChatAsync(
        string userMessage,
        AppMode mode,
        List<ChatMessage> history,
        string? systemPromptOverride = null,
        string? reasoningEffort = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return "请输入您的问题。";
        }

        if (!_available)
        {
            return MockResponse(userMessage, mode);
        }

        var modeString = mode == AppMode.Child ? "child" : "adult";

        try
        {
            var systemPrompt = systemPromptOverride
                ?? (mode == AppMode.Child ? ChildSystemPrompt : AdultSystemPrompt);

            var chatHistory = new ChatHistory(systemPrompt);

            var memories = await _memoryManager.SearchAsync(userMessage, modeString, limit: 5);
            if (memories.Count > 0)
            {
                var memoryContext = string.Join("\n---\n", memories.Select(m => m.Content));
                chatHistory.AddSystemMessage($"【相关家庭记忆】\n{memoryContext}");
            }

            foreach (var msg in history)
            {
                if (msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    chatHistory.AddUserMessage(msg.Content);
                }
                else if (msg.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
                {
                    chatHistory.AddAssistantMessage(msg.Content);
                }
            }

            chatHistory.AddUserMessage(userMessage);

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
            var executionSettings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var effort = reasoningEffort;
            if (string.IsNullOrEmpty(effort)) effort = _defaultReasoningEffort;
            if (!string.IsNullOrEmpty(effort)
                && (effort.Equals("low", StringComparison.OrdinalIgnoreCase)
                    || effort.Equals("medium", StringComparison.OrdinalIgnoreCase)
                    || effort.Equals("high", StringComparison.OrdinalIgnoreCase)))
            {
                executionSettings.ExtensionData = new Dictionary<string, object>
                {
                    ["reasoning_effort"] = effort.ToLowerInvariant()
                };
            }

            var result = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: executionSettings,
                kernel: _kernel,
                cancellationToken: cancellationToken);
            var reply = result.Content ?? string.Empty;

            _ = PersistMemoryAsync(userMessage, reply, modeString);

            return reply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "智能体对话失败");
            return $"对话出错: {ex.Message}";
        }
    }

    private async Task PersistMemoryAsync(string userMessage, string reply, string modeString)
    {
        try
        {
            await _memoryManager.AddMemoryAsync($"用户: {userMessage}", "conversation", modeString);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                await _memoryManager.AddMemoryAsync($"助手: {reply}", "conversation", modeString);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "持久化对话记忆失败(不影响主流程)");
        }
    }

    private static string MockResponse(string userMessage, AppMode mode)
    {
        var modeName = mode == AppMode.Child ? "小孩" : "大人";
        return $@"[Mock 模式 - DeepSeek API Key 未配置]
当前模式: {modeName}
您的问题: {userMessage}

请配置 appsettings.json 中的 DeepSeek:ApiKey 以启用真实对话。
配置示例:
  ""DeepSeek"": {{
    ""ApiKey"": ""sk-your-key-here""
  }}";
    }
}
