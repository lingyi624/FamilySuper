# Phase 2 收尾计划 (Batch E 编译验证 + Batch F 配置对齐)

## 概述

承接上一会话,Phase 2 已完成 Batch A-D(核心实体、基础设施服务、API 端点、Agent 增强)。当前 Batch E 的 6 个 Blazor 页面与 Chat.razor 重写**已创建文件但尚未编译验证**;Batch F(外部模型配置、.gitignore、csproj 模型复制)**完全待办**。

本计划聚焦于**让 Batch E 通过编译**并**完成 Batch F 收尾**,然后做整解决方案最终验证。

---

## 当前状态分析

### Batch E 已创建的文件(未编译)
- [FamilySuper.BlazorUI/Pages/Games.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Games.razor) — 占位页,无依赖问题
- [FamilySuper.BlazorUI/Pages/Family.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Family.razor) — 成员 CRUD + OCR + 证件管理
- [FamilySuper.BlazorUI/Pages/Finance.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Finance.razor) — 财务汇总 + 记录表格
- [FamilySuper.BlazorUI/Pages/Health.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Health.razor) — 健康档案管理
- [FamilySuper.BlazorUI/Pages/Work.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Work.razor) — 任务看板
- [FamilySuper.BlazorUI/Pages/Education.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Education.razor) — 学习辅导对话
- [FamilySuper.BlazorUI/Pages/Chat.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Chat.razor) — 智能对话(集成会话持久化)

### 已验证的项目引用
[FamilySuper.BlazorUI.csproj](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/FamilySuper.BlazorUI.csproj) 仅引用 Core / Data / Infrastructure,**未引用 Agent 项目**。

### 已验证的服务接口签名
所有页面调用的服务方法签名(IFinanceService / IHealthService / IWorkTaskService / IEducationService / ICertificateService / IConversationService / IAgentService)均与接口定义匹配,无签名错误。

### 已验证的枚举/实体
- `MessageRole` 枚举有 `User` / `Assistant` / `System` — Chat.razor 调用正确
- `FinanceSummary(decimal, decimal, decimal, int)` 记录 — Finance.razor `new(0,0,0,0)` 匹配
- `TaskStatus` 别名在 Work.razor 已用 `@using TaskStatus = FamilySuper.Core.Enums.TaskStatus` 解决

---

## 发现的关键问题

### 问题 1(编译阻塞):Education.razor 引用了 Agent 项目

**位置**: [Education.razor:113](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Education.razor#L113)

```csharp
var reply = await Agent.ChatAsync(question, AppMode.Child, historyMsgs, FamilySuper.Agent.AgentService.TutorSystemPrompt);
```

**原因**: BlazorUI.csproj 不引用 FamilySuper.Agent,无法访问 `FamilySuper.Agent.AgentService.TutorSystemPrompt` 常量。直接添加 Agent 项目引用会破坏分层(UI 不应依赖 Agent 实现)。

**修复方案**: 将 `TutorSystemPrompt` 常量移到 Core 共享层。

### 问题 2(运行时阻塞):Family.razor 调用了不存在的 JS 函数

**位置**: [Family.razor:258](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Family.razor#L258)

```csharp
await JS.InvokeVoidAsync("downloadFile", fileName, ms.ToArray());
```

**原因**: [wwwroot/index.html](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/wwwroot/index.html) 未定义 `downloadFile` JS 函数,点击证件"下载"按钮会静默失败。

**修复方案**: 在 index.html 添加 `downloadFile(fileName, byteArray)` JS 函数(基于 Blob + URL.createObjectURL)。

### 问题 3(配置不对齐):appsettings.json 缺少 OCR/Embedding 子键

**当前** [appsettings.json](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/appsettings.json):
```json
"Ocr": { "Enabled": false, "ModelPath": "./models" },
"Embedding": { "Enabled": false, "ModelPath": "./models/bge-small-zh-v1.5.onnx" }
```

**OcrService 实际读取**: `Ocr:Enabled` / `Ocr:ModelPath` / `Ocr:DetModelName` / `Ocr:RecModelName` / `Ocr:ClsModelName`
- 默认 `ModelPath = ./models/ocr`,默认文件名 `ch_PP-OCRv4_det_infer.onnx` 等

**BgeEmbeddingService 实际读取**: `Embedding:Enabled` / `Embedding:ModelPath` / `Embedding:VocabPath` / `Embedding:MaxTokens`
- 默认 `ModelPath = ./models/embedding/bge-small-zh-v1.5.onnx`,默认 `VocabPath = ./models/embedding/vocab.txt`

**修复方案**: 对齐 appsettings.json 路径与默认值一致(`./models/ocr/` 与 `./models/embedding/`),并显式写出所有子键避免歧义。

### 问题 4:Host.WPF.csproj 未配置 models 目录复制

[FamilySuper.Host.WPF.csproj](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/FamilySuper.Host.WPF.csproj) 已配置 `appsettings.json` 和 `wwwroot\**\*` 的复制,但**未配置 `models\**\*`**。用户放入 `models/` 的 ONNX 模型不会输出到 bin 目录,导致运行时 `IsAvailable = false`。

### 问题 5:仓库根目录无 .gitignore

Glob 确认根目录无 .gitignore,需创建(忽略 bin/obj/models/data/用户文件)。

---

## 实施步骤

### 步骤 1:创建共享常量类(解决问题 1)

**新建** `FamilySuper.Core/Constants/AgentPrompts.cs`:
```csharp
namespace FamilySuper.Core.Constants;

public static class AgentPrompts
{
    public const string TutorSystemPrompt = """
你是一位耐心、友善的家庭教师,专门为 K12 学生提供学习辅导。
请遵循以下原则:
1. 启发式教学:引导学生思考,而非直接给出答案
2. 语言生动易懂,适合未成年人
3. 严禁输出任何成人、财务、医疗或与学习无关的内容
4. 鼓励学生提问,对错误答案给予温和纠正
5. 涉及复杂概念时,用生活中的例子类比
6. 回答需包含:思路讲解、关键知识点、鼓励语
""";
}
```

### 步骤 2:AgentService 引用共享常量(保持向后兼容)

**修改** [FamilySuper.Agent/AgentService.cs:30](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/AgentService.cs#L30):
- 将 `public const string TutorSystemPrompt = """...""";` 改为 `public const string TutorSystemPrompt = FamilySuper.Core.Constants.AgentPrompts.TutorSystemPrompt;`
- 添加 `using FamilySuper.Core.Constants;`
- 这样既保留 Agent 层的 `AgentService.TutorSystemPrompt` 公共 API,又让常量真正定义在 Core 共享层

### 步骤 3:Education.razor 改用 Core 常量

**修改** [Education.razor:113](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Education.razor#L113):
```csharp
var reply = await Agent.ChatAsync(question, AppMode.Child, historyMsgs, FamilySuper.Core.Constants.AgentPrompts.TutorSystemPrompt);
```

### 步骤 4:_Imports.razor 添加 Constants 命名空间

**修改** [_Imports.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/_Imports.razor) 添加:
```
@using FamilySuper.Core.Constants
```
这样 Education.razor 可简化为 `AgentPrompts.TutorSystemPrompt`(可选,Education.razor 用全限定名也可)。

### 步骤 5:添加 downloadFile JS 函数(解决问题 2)

**修改** [wwwroot/index.html](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/wwwroot/index.html) 在 `</body>` 前、`<script src="_framework/blazor.webview.js">` 后添加:
```html
<script>
    window.downloadFile = function(fileName, byteArray) {
        const blob = new Blob([new Uint8Array(byteArray)]);
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    };
</script>
```

### 步骤 6:BlazorUI 项目编译验证

运行:
```
dotnet build FamilySuper.BlazorUI/FamilySuper.BlazorUI.csproj -c Debug
```
预期 0 错误。如有错误,逐个修复(重点关注 InputDate/InputNumber 绑定、nullable 警告)。

### 步骤 7:对齐 appsettings.json(解决问题 3)

**修改** [FamilySuper.Host.WPF/appsettings.json](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/appsettings.json) 的 Ocr/Embedding 节:
```json
"Ocr": {
    "Enabled": false,
    "ModelPath": "./models/ocr",
    "DetModelName": "ch_PP-OCRv4_det_infer.onnx",
    "RecModelName": "ch_PP-OCRv4_rec_infer.onnx",
    "ClsModelName": "ch_ppocr_mobile_v2.0_cls_infer.onnx"
},
"Embedding": {
    "Enabled": false,
    "ModelPath": "./models/embedding/bge-small-zh-v1.5.onnx",
    "VocabPath": "./models/embedding/vocab.txt",
    "MaxTokens": 512
}
```

同步更新 [FamilySuper.API/appsettings.json](file:///d:/study/.net%20core/HomeAgent/FamilySuper.API/appsettings.json)(API 项目作为可选自宿主,保持配置一致)。

### 步骤 8:Host.WPF.csproj 添加 models 复制(解决问题 4)

**修改** [FamilySuper.Host.WPF.csproj](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/FamilySuper.Host.WPF.csproj) 在现有 `<ItemGroup>` 中添加:
```xml
<None Update="models\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

### 步骤 9:创建 .gitignore(解决问题 5)

**新建** `d:\study\.net core\HomeAgent\.gitignore`:
```
## .NET 构建产物
bin/
obj/

## 用户专属文件
*.user
*.suo
.vs/
*.rsuser

## 模型与运行时数据(体积大,不入库)
models/
data/

## 日志
*.log
logs/

## IDE
.idea/
.vscode/

## 操作系统
Thumbs.db
ehthumbs.db
Desktop.ini
.DS_Store

## 临时文件
*.tmp
*.bak
```

### 步骤 10:整解决方案最终编译验证

运行:
```
dotnet build FamilySuper.slnx -c Debug
```
预期:0 错误 / 0 警告(或仅 CA1416 已被 NoWarn 抑制)。

如出现错误,按以下优先级修复:
1. CS0103(命名空间未找到)— 检查 using / 项目引用
2. CS8601/CS8602(nullable)— 添加 `?? string.Empty` 或 `!`
3. Razor 编译错误 — 检查 `@bind-Value` / `@inject` / `@code` 语法

---

## 验证清单

完成后需确认:
- [ ] `FamilySuper.Core/Constants/AgentPrompts.cs` 存在且包含 TutorSystemPrompt
- [ ] `FamilySuper.Agent/AgentService.cs` 引用 `AgentPrompts.TutorSystemPrompt`
- [ ] `Education.razor` 编译通过(无 Agent 项目引用)
- [ ] `wwwroot/index.html` 包含 `downloadFile` JS 函数
- [ ] `appsettings.json` 包含完整 Ocr/Embedding 子键
- [ ] `FamilySuper.Host.WPF.csproj` 包含 `models\**\*` 复制条目
- [ ] 根目录 `.gitignore` 已创建
- [ ] `dotnet build FamilySuper.slnx -c Debug` 成功(0 错误)

---

## 假设与决策

1. **决策**: 将 `TutorSystemPrompt` 移到 Core 而非添加 Agent 项目引用 — 保持 UI 层不依赖 Agent 实现层,符合分层架构。
2. **假设**: `wwwroot/index.html` 是 BlazorWebView 加载的入口(已确认),JS 函数定义在此即可全局可用。
3. **决策**: appsettings.json 路径对齐到服务默认值(`./models/ocr/` 与 `./models/embedding/`),而非修改服务代码 — 服务代码默认值已合理,配置应跟随代码。
4. **假设**: 用户尚未放置实际 ONNX 模型文件,因此 `Enabled: false` 保持不变;仅修复路径配置,启用模型时改 `true` 即可。
5. **决策**: 不在本批次实现 Frp 配置或 DeepSeek API Key 注入 — 这些属于运维配置,不在代码收尾范围。
