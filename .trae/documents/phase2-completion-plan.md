# Phase 2 收尾实施计划:从当前实际状态到完整可用

## 当前状态分析(基于代码探查)

### 已完成
- **批次 A**(Core/Data 层):7 枚举、7 新实体、4 DbContext 改造、Data/ServiceCollectionExtensions.cs(10 个 IRepository<T> 绑定)、DbInitializer 更新 — 全部就绪
- **批次 B 部分**(Infrastructure 层):
  - 6 业务服务接口 + 6 实现已创建
  - OcrService.cs 已改为真实 ONNX 检测+识别+CTC 解码
  - BgeEmbeddingService.cs 已改为真实 ONNX 推理
  - Infrastructure.csproj 已加 Microsoft.ML.OnnxRuntime、Microsoft.ML.Tokenizers 2.0.0、System.Drawing.Common 10.0.9

### 待完成(本计划范围)
1. **批次 B 收尾**:Infrastructure/ServiceCollectionExtensions.cs 缺 6 个业务服务注册;构建验证并修复编译错误
2. **批次 C**:6 个新 API 端点 + ApiBootstrap 改造
3. **批次 D**:Agent 增强(插件注册 + 真实数据查询 + MemoryManager 持久化)
4. **批次 E**:6 个新 Blazor 页面 + Chat.razor 集成对话持久化 + BlazorUI 项目引用
5. **批次 F**:外部模型文件 + appsettings.json 配置对齐 + .gitignore + csproj 模型复制

### 探查发现的关键问题(必须修复)

1. **App.xaml.cs 未调用 `AddFamilySuperData()`**:第 92-101 行手动注册了 5 个 DbContext,但未注册任何 `IRepository<T>`。这会导致所有业务服务(FinanceService 等)在运行时 DI 解析失败。需替换为 `services.AddFamilySuperData(configuration)` 调用。
2. **BgeEmbeddingService.cs 类型不匹配**:`_tokenizer.Encode(text).Ids` 返回 `IReadOnlyList<int>`,但代码用 `101L`(long)拼接,`tokens` 列表类型为 `List<long>`,会编译失败。需统一为 `int`。
3. **`TiktokenTokenizer.CreateBpe(_vocabPath)` API 不存在**:Microsoft.ML.Tokenizers 2.0.0 的实际 API 是 `TiktokenTokenizer.CreateForModel(...)` 或 `BpeTokenizer.Create(...)`,且 BGE 用的是 BERT WordPiece tokenizer,不是 BPE。应改用 `BertTokenizer.Create(vocabPath)` 加载 vocab.txt。
4. **FinanceService.UpdateAsync 同步阻塞**:第 41 行 `_repo.UpdateAsync(record, ct).Wait(ct)` 是 sync-over-async,应改为 `await _repo.UpdateAsync(record, ct)`(假设 IRepository 有此方法;若无则用 Update + SaveChanges)。
5. **OcrService.cs 命名空间冲突**:`System.Drawing` 与 `Microsoft.ML.OnnxRuntime.Tensors` 中的 `Tensor` 不冲突,但 `Color`、`Bitmap` 等类型需确认 ImplicitUsings 不引入歧义。当前文件已显式 `using System.Drawing;`,应无问题。
6. **appsettings.json 配置不匹配**:Ocr 节 `ModelPath: "./models"` 但服务期望 `./models/ocr`;Embedding 节缺 `VocabPath`、`MaxTokens`、`Enabled` 字段。需对齐。
7. **BlazorUI.csproj 未引用 Data/Infrastructure**:页面无法注入业务服务。需添加项目引用。
8. **_Imports.razor 缺 using**:需追加 `FamilySuper.Core.Entities`、`FamilySuper.Data.Context`、`FamilySuper.Infrastructure.Services`。
9. **ApiBootstrap.cs 只注册 2 个 DbContext + 2 个端点**:需注册全部 5 个 DbContext、复制业务服务、Map 6 个新端点。
10. **Host.WPF.csproj 未配置 models 目录复制**:模型文件不会进入输出目录。

## 实施批次(每批独立编译验证)

### 批次 B 收尾:Infrastructure 注册 + 编译修复

**修改 `FamilySuper.Infrastructure/ServiceCollectionExtensions.cs`**:在现有 5 个注册后追加 6 个 Scoped 业务服务注册:
```csharp
services.AddScoped<IFinanceService, FinanceService>();
services.AddScoped<IHealthService, HealthService>();
services.AddScoped<IEducationService, EducationService>();
services.AddScoped<IWorkTaskService, WorkTaskService>();
services.AddScoped<ICertificateService, CertificateService>();
services.AddScoped<IConversationService, ConversationService>();
```

**修复 `BgeEmbeddingService.cs`**:
- 将 `TiktokenTokenizer.CreateBpe(_vocabPath)` 改为 `BertTokenizer.Create(_vocabPath)`(BERT WordPiece,与 BGE 模型匹配)
- 将所有 `long` token 类型改为 `int`(`DenseTensor<int>`、`new[] { 101 }`、`tokens.Where(t => t != 101 && t != 102)`、`Append(102)`)
- 添加 `using Microsoft.ML.Tokenizers;`(已有)

**修复 `FinanceService.cs`**:第 41 行 `_repo.UpdateAsync(record, ct).Wait(ct)` 改为 `await _repo.UpdateAsync(record, ct)`(若 IRepository 接口无此异步方法,则用 `_repo.Update(record);` + `await _repo.SaveChangesAsync(ct);`)。需先确认 [IRepository.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Core/Interfaces/IRepository.cs) 的实际方法签名,可能其他 Service 也有相同问题。

**验证**:
```powershell
dotnet build FamilySuper.slnx -c Debug
```
预期:0 错误 0 警告。若有 `System.Drawing.Common` 平台警告,在 Infrastructure.csproj 添加 `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` 或在 AssemblyInfo 添加 `[System.Runtime.Versioning.SupportedOSPlatform("windows")]`。

### 批次 C:API 端点

**新建 6 个端点文件**(`FamilySuper.API/Endpoints/`),参考 [FamilyEndpoints.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.API/Endpoints/FamilyEndpoints.cs) 的 minimal API 模式,但**通过 DI 注入业务服务而非直接注入 DbContext**(保持与 Host 端一致的模式隔离):

| 文件 | 路由组 | 端点 |
|------|-------|------|
| `FinanceEndpoints.cs` | `/api/finance` | GET `/records`(支持 type/category 查询)、POST/PUT/DELETE `/records/{id}`、GET `/summary` |
| `HealthRecordsEndpoints.cs` | `/api/health-records` | GET/POST/PUT/DELETE `/records` |
| `EducationEndpoints.cs` | `/api/education` | GET/POST/DELETE `/records` |
| `WorkTaskEndpoints.cs` | `/api/work` | GET/POST/PUT/DELETE `/tasks`、PATCH `/tasks/{id}/status` |
| `CertificateEndpoints.cs` | `/api/certificates` | GET(按 memberId)、POST(IFormFile 上传)、DELETE、GET `/{id}/file`(下载解密) |
| `ConversationEndpoints.cs` | `/api/conversations` | GET/POST/DELETE `/sessions`、GET/POST `/sessions/{id}/messages` |

**端点服务注入模式**:
```csharp
group.MapGet("/records", async (IFinanceService svc, FinanceType? type, string? category, CancellationToken ct) =>
    Results.Ok(await svc.GetRecordsAsync(type, category, ct)));
```

**改造 `ApiBootstrap.cs`**:
- 从 rootProvider 复制所有业务服务:`IFinanceService`、`IHealthService`、`IEducationService`、`IWorkTaskService`、`ICertificateService`、`IConversationService`、`IEncryptionService`、`IEmbeddingService`
- 注册 3 个新 DbContext:HealthDbContext、EducationDbContext、KnowledgeDbContext(从 rootProvider 取或本地注册)
- 连接字符串追加 `Cache=Shared` 避免与 Host 端 SQLite 锁冲突
- 调用 6 个新端点的 `Map(app)`

**验证**:`dotnet build`,启动后用 curl 测试 `GET /api/finance/records` 返回 `[]`。

### 批次 D:Agent 增强

**改造 `KernelBuilder.cs`**:`Build` 方法签名新增 `IServiceProvider? rootProvider = null` 参数。在 `builder.Build()` 前当 rootProvider 非空时:
```csharp
builder.Plugins.AddFromType<FamilyTools>();
builder.Plugins.AddFromType<FinanceTools>();
builder.Plugins.AddFromType<HealthTools>();
```
注:Semantic Kernel 的 `AddFromType<T>` 会用 DI 容器解析 T 的构造参数,因此插件构造函数可注入 `IServiceProvider`。

**改造 `AgentService.cs`**:
- 构造函数新增 `IServiceProvider` 参数,传入 `KernelBuilder.Build(configuration, loggerFactory, serviceProvider)`
- `ChatAsync` 新增可选参数 `string? systemPromptOverride = null`,当非空时覆盖默认 systemPrompt(供教育辅导页传入 `TutorSystemPrompt`)
- 新增内部常量 `TutorSystemPrompt`(耐心、安全、未成年人适宜、强调启发式教学)

**重写 3 个插件**(`FamilySuper.Agent/Plugins/`):
- `FamilyTools`:构造函数注入 `IServiceProvider`;每个 KernelFunction 内 `using var scope = _sp.CreateScope();` 解析 `IRepository<FamilyMember>`;返回真实成员 JSON
- `FinanceTools`:同模式,解析 `IRepository<FinanceRecord>`;新增 `add_finance_record` KernelFunction 写入记录
- `HealthTools`:同模式;先按 memberName 查 FamilyMember 获取 MemberId,再查 HealthRecord

**升级 `MemoryManager.cs`**:
- 构造函数注入 `IServiceProvider` + `IEmbeddingService`(可选,用于语义检索)
- `AddMemoryAsync`:创建 scope 解析 `IRepository<MemoryEntry>`,生成 embedding(JSON 序列化),持久化到 knowledge.db
- `SearchAsync`:用 `Contains` 关键词匹配查询 MemoryEntry(无 embedding 时降级为关键词),按时间倒序返回

**验证**:`dotnet build`;启动后 Agent 对话问"家庭成员有哪些",观察日志是否触发插件调用并返回真实数据。

### 批次 E:Blazor 页面

**项目引用变更**(`FamilySuper.BlazorUI.csproj`):
```xml
<ProjectReference Include="..\FamilySuper.Data\FamilySuper.Data.csproj" />
<ProjectReference Include="..\FamilySuper.Infrastructure\FamilySuper.Infrastructure.csproj" />
```

**`_Imports.razor` 追加**:
```
@using FamilySuper.Core.Entities
@using FamilySuper.Core.Enums
@using FamilySuper.Data.Context
@using FamilySuper.Infrastructure.Services
```

**新增 6 个页面**(`FamilySuper.BlazorUI/Pages/`):

| 页面 | 路由 | 注入服务 | 核心 UI |
|------|------|---------|--------|
| `Family.razor` | `/family` | `IRepository<FamilyMember>`、`ICertificateService`、`IOcrService` | 成员列表表格 + EditForm + InputFile 上传身份证→OCR 自动填充 + 证件上传/下载 |
| `Finance.razor` | `/finance` | `IFinanceService` | 顶部汇总卡(收入/支出/余额)+ 记录列表 + EditForm + 月份筛选 |
| `Health.razor` | `/health` | `IHealthService`、`IRepository<FamilyMember>` | 成员筛选 + 类型筛选 + 记录列表 + EditForm |
| `Work.razor` | `/work` | `IWorkTaskService` | 任务看板(按状态分组)+ EditForm + 状态快速切换 + 优先级颜色标识 |
| `Education.razor` | `/education` | `IAgentService`、`IEducationService` | 学科标签 + 聊天窗口(传 TutorSystemPrompt)+ 问答自动保存 + 历史侧边栏 |
| `Games.razor` | `/games` | (无) | 占位页"即将上线" |

**更新 `Chat.razor`**:
- 注入 `IConversationService`
- 左侧侧边栏:会话列表(新建/切换/删除)
- 选中会话时:从 DB 加载历史消息显示
- 每次发送:先 `AddMessageAsync(sessionId, User, text)`,收到回复后 `AddMessageAsync(sessionId, Assistant, reply)`
- 新建会话:`CreateSessionAsync(title)`,首条消息后更新标题

**验证**:`dotnet build`;启动后逐页点击 NavMenu,确保每个页面可访问、CRUD 操作生效。

### 批次 F:外部模型 + 配置对齐

**模型文件下载**(约 120-150MB,可与 E 并行,但本批次不做实际下载,只在 appsettings + csproj 配置就绪,模型缺失时服务降级返回零向量/空结果):

| 文件 | 路径 | 来源(用户手动下载) |
|------|------|---------------------|
| `bge-small-zh-v1.5.onnx` | `models/embedding/` | `huggingface.co/xlangai/bge-small-zh-v1.5-onnx` 或 `hf-mirror.com` |
| `vocab.txt` | `models/embedding/` | `huggingface.co/BAAI/bge-small-zh-v1.5` |
| `ch_PP-OCRv4_det_infer.onnx` | `models/ocr/` | `paddleocr.bj.bcebos.com/PP-OCRv4/chinese/` |
| `ch_PP-OCRv4_rec_infer.onnx` | `models/ocr/` | 同上 |
| `ch_ppocr_mobile_v2.0_cls_infer.onnx` | `models/ocr/`(可选,当前 OcrService 未用 cls) | `paddleocr.bj.bcebos.com/dygraph_v2.0/ch/` |
| `ppocr_keys_v1.txt` | `models/ocr/` | PaddleOCR GitHub `paddleocr/utils/` |

**更新 `appsettings.json`**:
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

**更新 `FamilySuper.Host.WPF.csproj`**:在 `<ItemGroup>` 中追加:
```xml
<None Update="models\**\*">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

**更新 `.gitignore`**:追加 `models/` 目录。

**最终验证**:
```powershell
dotnet build FamilySuper.slnx -c Debug    # 0 错误 0 警告
dotnet run --project FamilySuper.Host.WPF -c Debug
```
逐页验证清单(参考原 phase2-business-modules.md 的 15 项验证)。

## 关键决策与假设

1. **服务注入模式**:API 端点通过 DI 注入业务服务(IFinanceService 等),不直接注入 DbContext,保持模式隔离一致性。需在 ApiBootstrap 从 rootProvider 复制这些服务。
2. **IRepository.UpdateAsync 签名**:需先读 [IRepository.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Core/Interfaces/IRepository.cs) 确认。若无异步 Update 方法,所有 Service 的 UpdateAsync 都需调整为 `_repo.Update(record); await _repo.SaveChangesAsync(ct);`。
3. **插件 DI 模式**:Semantic Kernel 的 `AddFromType<T>` 用 DI 解析,插件构造函数注入 `IServiceProvider`,每次 KernelFunction 调用内部 `CreateScope()` 避免 Singleton 捕获 Scoped 服务的 captive dependency 问题。
4. **Tokenizer API**:`BertTokenizer.Create(vocabPath)` 是 Microsoft.ML.Tokenizers 2.0.0 的正确 API(返回 `Tokenizer`)。Encode 返回 `IReadOnlyList<int>` 的 Ids。
5. **System.Drawing.Common Windows-only**:WPF Host 已是 `net10.0-windows`,Infrastructure 项目保持 `net10.0` 但运行时只在 Windows 调用。构建时可能产生 CA1416 警告,需在 Infrastructure.csproj 添加 `<NoWarn>CA1416</NoWarn>` 或标注 `[SupportedOSPlatform]`。
6. **模型文件不自动下载**:用户网络环境下载 HuggingFace 不稳定,本计划只配置就绪,模型缺失时服务优雅降级(返回零向量/空 OCR 结果)。用户可后续手动下载启用。
7. **SQLite schema 重置**:由于实体类型变更(Gender string→enum、FinanceRecord.Type string→enum、新增多个 DbSet),实施前需删除 `data/*.db` 让 DbInitializer 重建。

## 实施顺序

1. 批次 B 收尾(Infrastructure 注册 + BGE/Finance 修复 + 构建验证)
2. 批次 C(API 端点 + ApiBootstrap)
3. 批次 D(Agent 增强)
4. 批次 E(Blazor 页面)
5. 批次 F(配置对齐 + csproj + .gitignore)
6. 最终全量构建 + 删除旧 .db + 启动验证

每批完成后 `dotnet build` 验证。批次 F 的模型下载可由用户后续手动完成。

## 修改文件清单(本计划范围)

**修改**(15 个):
- [FamilySuper.Infrastructure/ServiceCollectionExtensions.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Infrastructure/ServiceCollectionExtensions.cs) — 追加 6 业务服务注册
- [FamilySuper.Infrastructure/Services/BgeEmbeddingService.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Infrastructure/Services/BgeEmbeddingService.cs) — Tokenizer API + int 类型修复
- [FamilySuper.Infrastructure/Services/FinanceService.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Infrastructure/Services/FinanceService.cs) — UpdateAsync 同步阻塞修复(其他 Service 同理)
- [FamilySuper.API/ApiBootstrap.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.API/ApiBootstrap.cs) — 注册 3 DbContext + 复制业务服务 + 6 端点 Map
- [FamilySuper.Agent/KernelBuilder.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/KernelBuilder.cs) — 新增 rootProvider 参数 + 注册插件
- [FamilySuper.Agent/AgentService.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/AgentService.cs) — 注入 IServiceProvider + systemPromptOverride + TutorSystemPrompt
- [FamilySuper.Agent/MemoryManager.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/MemoryManager.cs) — SQLite 持久化
- [FamilySuper.Agent/Plugins/FamilyTools.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/Plugins/FamilyTools.cs) — 真实查询
- [FamilySuper.Agent/Plugins/FinanceTools.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/Plugins/FinanceTools.cs) — 真实查询 + 写入
- [FamilySuper.Agent/Plugins/HealthTools.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/Plugins/HealthTools.cs) — 真实查询
- [FamilySuper.BlazorUI/FamilySuper.BlazorUI.csproj](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/FamilySuper.BlazorUI.csproj) — 添加 Data/Infrastructure 项目引用
- [FamilySuper.BlazorUI/_Imports.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/_Imports.razor) — 追加 using
- [FamilySuper.BlazorUI/Pages/Chat.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Chat.razor) — 集成 ConversationService
- [FamilySuper.Host.WPF/App.xaml.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/App.xaml.cs) — 替换手动 DbContext 注册为 AddFamilySuperData()
- [FamilySuper.Host.WPF/appsettings.json](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/appsettings.json) — Ocr/Embedding 配置对齐
- [FamilySuper.Host.WPF/FamilySuper.Host.WPF.csproj](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/FamilySuper.Host.WPF.csproj) — 添加 models 目录复制
- [.gitignore](file:///d:/study/.net%20core/HomeAgent/.gitignore) — 追加 models/

**新建**(12 个):
- API/Endpoints/:FinanceEndpoints.cs、HealthRecordsEndpoints.cs、EducationEndpoints.cs、WorkTaskEndpoints.cs、CertificateEndpoints.cs、ConversationEndpoints.cs(6 个)
- BlazorUI/Pages/:Family.razor、Finance.razor、Health.razor、Work.razor、Education.razor、Games.razor(6 个)
