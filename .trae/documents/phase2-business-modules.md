# Phase 2 实施计划:业务模块 + 真实外部服务集成

## 背景

Phase 1 完成了 WPF+Blazor Hybrid 宿主框架,7 个项目编译通过(0 警告 0 错误)。但业务模块严重缺失:NavMenu 有 6 个链接但只有 3 个页面存在,Finance/Health/Education/Work 模块的实体、端点、页面全部缺失,Agent 的 3 个插件返回硬编码 JSON 且未注册到 Kernel,OCR 和 Embedding 是占位实现。

Phase 2 目标:实现 P0 业务模块 + Agent 增强 + 真实 PaddleOCR/BGE 集成,使系统具备实际可用价值。

## 探查发现的关键问题(必须修复)

1. **FamilyDbContext 查询过滤器 bug**:第 21 行 `var currentMode = ...` 声明后未使用,FamilyMember/SystemConfig 仅过滤 `!x.IsDeleted`,未做模式隔离
2. **Agent 插件未注册**:`KernelBuilder.Build` 从未调用 `builder.Plugins.AddFromType<...>()`,3 个占位插件完全未被 Semantic Kernel 加载
3. **IRepository<T> 未注册**:DI 容器中无任何 `IRepository<T>` 绑定,`Repository<T>` 实现无人消费
4. **3 个空 DbContext 无 IModeManager 注入**:HealthDbContext/EducationDbContext/KnowledgeDbContext 使用无参构造
5. **ApiBootstrap 只注册 2 个 DbContext**,只调用 2 个端点 Map

## 实施批次(每批独立编译验证)

### 批次 A:Core 实体枚举 + Data 层

**新增枚举**(FamilySuper.Core/Enums/):
- `Gender`(Male, Female, Unknown)
- `FinanceType`(Income, Expense)
- `CertificateType`(IdCard, HouseholdRegister, PropertyCertificate, VehicleCertificate, MarriageCertificate, BirthCertificate, Passport, Other)
- `HealthRecordType`(Examination, MedicalHistory, Allergy, Medication, VitalSigns, Vaccination, Other)
- `TaskPriority`(Low, Medium, High, Urgent)
- `TaskStatus`(Pending, InProgress, Completed, Cancelled, Overdue)
- `MessageRole`(User, Assistant, System)

所有枚举通过 EF Core `.HasConversion<string>()` 存储为字符串。

**新增实体**(FamilySuper.Core/Entities/,均继承 EntityBase):

| 实体 | 所属 DbContext | 关键属性 |
|------|---------------|---------|
| `Certificate` | FamilyDbContext | MemberId, Name, Type(CertificateType), Number, EncryptedData(byte[]), FileName, IssueDate, ExpiryDate |
| `HealthRecord` | HealthDbContext | MemberId, RecordType(HealthRecordType), Title, Content, RecordDate, Doctor, Hospital |
| `EducationRecord` | EducationDbContext | MemberId, Subject, Question, Answer, RecordDate |
| `WorkTask` | FamilyDbContext | Title, Description, Priority(TaskPriority), Status(TaskStatus), DueDate, CompletedAt, MemberId, Category |
| `ConversationSession` | KnowledgeDbContext | Title, MemberId, LastMessageAt, MessageCount |
| `ConversationMessage` | KnowledgeDbContext | SessionId, Role(MessageRole), Content, TokenCount, ModelId |
| `MemoryEntry` | KnowledgeDbContext | Content, Category, MemberId, EmbeddingJson |

**修改现有实体**:
- `FamilyMember.Gender`: `string?` → `Gender?`
- `FinanceRecord.Type`: `string` → `FinanceType`

**DbContext 改造**:
- FamilyDbContext: 新增 `Certificates`、`WorkTasks` DbSet;**修复** FamilyMember/SystemConfig 查询过滤器保持 `!x.IsDeleted`(成员两模式可见),Certificate/WorkTask 用 `!x.IsDeleted && Mode == currentMode`;删除未使用变量
- HealthDbContext/EducationDbContext/KnowledgeDbContext: 注入 IModeManager,添加 DbSet + 查询过滤器
- 查询过滤器策略:family.db 部分隔离(成员/配置/教育全模式可见,证件/任务/对话按模式过滤),finance/health/knowledge 按模式过滤

**新建 FamilySuper.Data/ServiceCollectionExtensions.cs**: `AddFamilySuperData()` 注册所有 `IRepository<T>` 绑定(每个实体对应正确 DbContext)

**更新 DbInitializer.cs**: 种子数据用新枚举值(Gender.Male, FinanceType.Expense)

**关键文件**:
- 参考 [FinanceDbContext.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/Context/FinanceDbContext.cs) 作为查询过滤器模板
- 修改 [FamilyDbContext.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/Context/FamilyDbContext.cs) 修复 bug
- 新建 [FamilySuper.Data/ServiceCollectionExtensions.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/ServiceCollectionExtensions.cs)

### 批次 B:Infrastructure 业务服务 + 真实 OCR/Embedding

**新增业务服务接口**(FamilySuper.Core/Interfaces/):
- `IFinanceService`、`IHealthService`、`IEducationService`、`IWorkTaskService`、`ICertificateService`、`IConversationService`

**新增业务服务实现**(FamilySuper.Infrastructure/Services/):
- `FinanceService`、`HealthService`、`EducationService`、`WorkTaskService`:注入 `IRepository<T>`,CRUD + 查询
- `CertificateService`:注入 `IRepository<Certificate>` + `IEncryptionService`,上传时 `EncryptBytes` 加密文件,下载时 `DecryptBytes` 解密
- `ConversationService`:管理会话和消息,AddMessage 时更新 Session.LastMessageAt/MessageCount

**真实 PaddleOCR 实现**(OcrService.cs 改造):
- 添加 `PaddleOCRSharp` NuGet 包到 Infrastructure.csproj
- 模型文件(4 个,约 17MB):`ch_PP-OCRv4_det_infer.onnx`、`ch_PP-OCRv4_rec_infer.onnx`、`ch_ppocr_mobile_v2.0_cls_infer.onnx`、`ppocr_keys_v1.txt`,放置 `models/ocr/`
- 延迟初始化(首次调用加载),构造函数读取 `Ocr:ModelPath` 配置
- `RecognizeAsync`:全图 OCR + 正则提取(姓名/18位身份证号/住址/出生/性别)
- **备选方案**:若 PaddleOCRSharp 不兼容 .NET 10,改用 ONNX Runtime 直接加载三阶段管线(det→cls→rec)

**真实 BGE Embedding 实现**(BgeEmbeddingService.cs 改造):
- 添加 `Microsoft.ML.Tokenizers` NuGet 包
- 模型文件:`bge-small-zh-v1.5.onnx`(~100MB)+ `vocab.txt`(~100KB),放置 `models/embedding/`
- 延迟初始化 `InferenceSession`
- 流程:BERT tokenize(添加 [CLS]/[SEP],max 512)→ ONNX 推理 → 均值池化(按 attention_mask)→ L2 归一化 → 返回 float[512]
- **模型来源**:`https://huggingface.co/xlangai/bge-small-zh-v1.5-onnx` 或 `hf-mirror.com` 镜像

**更新 Infrastructure/ServiceCollectionExtensions.cs**: 追加 6 个业务服务 Scoped 注册

### 批次 C:API 端点

**新增端点**(FamilySuper.API/Endpoints/,参考 [FamilyEndpoints.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.API/Endpoints/FamilyEndpoints.cs) 模板):

| 端点文件 | 路由组 | 路由 |
|---------|-------|------|
| `FinanceEndpoints.cs` | `/api/finance` | GET/POST/PUT/DELETE `/records`,GET `/summary` |
| `HealthRecordsEndpoints.cs` | `/api/health-records` | GET/POST/PUT/DELETE `/records`(避免与系统健康检查 `/health` 冲突) |
| `EducationEndpoints.cs` | `/api/education` | GET/POST/DELETE `/records` |
| `WorkTaskEndpoints.cs` | `/api/work` | GET/POST/PUT/DELETE `/tasks`,PATCH `/tasks/{id}/status` |
| `CertificateEndpoints.cs` | `/api/certificates` | GET/POST/DELETE,GET `/{id}/file`(下载解密) |
| `ConversationEndpoints.cs` | `/api/conversations` | GET/POST/DELETE `/sessions`,GET/POST `/sessions/{id}/messages` |

**更新 ApiBootstrap.cs**:
- 注册 3 个新 DbContext(HealthDbContext, EducationDbContext, KnowledgeDbContext)
- 从 rootProvider 复制业务服务
- 调用 6 个新端点的 Map 方法
- 连接字符串添加 `Cache=Shared` 避免与 Host 端 DbContext 锁冲突

### 批次 D:Agent 增强

**改造 KernelBuilder.cs**:
- `Build` 方法新增 `IServiceProvider? rootProvider` 参数
- 当 rootProvider 非空时:`builder.Plugins.AddFromObject(new FamilyTools(rootProvider))` 等

**改造 AgentService.cs**:
- 构造函数新增 `IServiceProvider` 参数,传入 KernelBuilder
- `ChatAsync` 新增可选参数 `string? systemPromptOverride`(供教育辅导页传入专属提示词)

**重写 3 个插件**(FamilySuper.Agent/Plugins/):
- 每个插件构造函数注入 `IServiceProvider`,每个 KernelFunction 调用内部 `using var scope = _sp.CreateScope()` 解析 `IRepository<T>`(避免 Singleton 注入 Scoped 的 captive dependency)
- `FamilyTools`:查询真实 FamilyMember 列表
- `FinanceTools`:查询真实 FinanceRecord,支持 add_finance_record 写入
- `HealthTools`:查询真实 HealthRecord,先按 memberName 查 FamilyMember 获取 MemberId

**升级 MemoryManager.cs**:
- 注入 `IServiceProvider`,每次调用创建 Scope 解析 `IRepository<MemoryEntry>`
- `AddMemoryAsync`:创建 MemoryEntry 实体持久化
- `SearchAsync`:用 `FindAsync(x => x.Mode == mode && x.Content.Contains(query))` 替代内存 List

**新增常量**:`TutorSystemPrompt`(教育辅导专属系统提示词,强调耐心、安全、未成年人适宜)

### 批次 E:Blazor 页面

**项目引用变更**(FamilySuper.BlazorUI.csproj):
- 添加 `FamilySuper.Data` 和 `FamilySuper.Infrastructure` 项目引用

**_Imports.razor 追加**:
- `@using FamilySuper.Core.Entities`
- `@using FamilySuper.Core.Enums`
- `@using FamilySuper.Data.Context`
- `@using FamilySuper.Infrastructure.Services`

**新增页面**:

| 页面 | 路由 | 模式 | 关键 UI |
|------|------|------|--------|
| `Family.razor` | `/family` | Adult | 成员列表表格 + EditForm 编辑 + OCR 上传(InputFile→IOcrService→自动填充)+ 证件管理(上传/下载/删除) |
| `Finance.razor` | `/finance` | Adult | 顶部汇总卡(收入/支出/余额)+ 记录列表 + EditForm + 月份选择器 |
| `Health.razor` | `/health` | Adult | 成员筛选 + 类型筛选 + 记录列表 + EditForm |
| `Work.razor` | `/work` | Adult | 任务看板(按状态分组)+ EditForm + 状态快速切换 + 优先级颜色 |
| `Education.razor` | `/education` | Child(Adult 可见) | 学科标签 + 聊天窗口(传入 TutorSystemPrompt)+ 问答自动保存到 EducationRecord + 历史侧边栏 |
| `Games.razor` | `/games` | Child | 占位页("即将上线") |

**更新 Chat.razor**:集成 `IConversationService`,侧边栏显示历史会话列表,可新建/切换/删除会话,消息从 DB 加载,每次问答持久化

**NavMenu.razor**:已有所有链接,无需修改

### 批次 F:外部模型 + 配置

**模型文件下载**(约 120-150MB 总计):

| 文件 | 大小 | 来源 | 路径 |
|------|------|------|------|
| `bge-small-zh-v1.5.onnx` | ~100MB | `huggingface.co/xlangai/bge-small-zh-v1.5-onnx` 或 `hf-mirror.com` | `models/embedding/` |
| `vocab.txt` | ~100KB | `huggingface.co/BAAI/bge-small-zh-v1.5` | `models/embedding/` |
| `ch_PP-OCRv4_det_infer.onnx` | ~5MB | `paddleocr.bj.bcebos.com/PP-OCRv4/chinese/` | `models/ocr/` |
| `ch_PP-OCRv4_rec_infer.onnx` | ~10MB | 同上 | `models/ocr/` |
| `ch_ppocr_mobile_v2.0_cls_infer.onnx` | ~2MB | `paddleocr.bj.bcebos.com/dygraph_v2.0/ch/` | `models/ocr/` |
| `ppocr_keys_v1.txt` | ~30KB | PaddleOCR GitHub 仓库 `paddleocr/utils/` | `models/ocr/` |

**appsettings.json 更新**:Ocr 节添加 ModelPath/DetModelName/RecModelName/ClsModelName;Embedding 节添加 ModelPath/VocabPath/MaxTokens

**.gitignore**:添加 `models/` 目录

**Host.WPF.csproj**:添加 `<None Update="models\**\*"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` 确保模型文件复制到输出目录

## DI 注册变更汇总

| 层级 | 文件 | 变更 |
|------|------|------|
| Data | 新建 ServiceCollectionExtensions.cs | `AddFamilySuperData()` 注册所有 IRepository<T> |
| Infrastructure | ServiceCollectionExtensions.cs | 追加 6 个业务服务 Scoped 注册 |
| Agent | ServiceCollectionExtensions.cs | 保持 Singleton,但 MemoryManager 改注入 IServiceProvider |
| Host.WPF | App.xaml.cs | 追加 `services.AddFamilySuperData()` |
| API | ApiBootstrap.cs | 注册 3 个新 DbContext + 复制业务服务 + 6 个端点 Map |

## 验证步骤

### 构建验证
```powershell
cd "d:\study\.net core\HomeAgent"
dotnet build FamilySuper.slnx -c Debug
# 预期:0 错误 0 警告
```

### 模型文件验证
```powershell
ls models/embedding/bge-small-zh-v1.5.onnx  # ~100MB
ls models/embedding/vocab.txt                # ~100KB
ls models/ocr/                               # 4 个文件
```

### 运行验证
```powershell
dotnet run --project FamilySuper.Host.WPF -c Debug
```

### 逐页验证清单

| # | 操作 | 预期 |
|---|------|------|
| 1 | 启动,查看首页 | 显示"大人模式" |
| 2 | 导航到 `/family` | 显示成员列表(含种子数据) |
| 3 | 添加/编辑/删除成员 | CRUD 生效 |
| 4 | 上传身份证图片 | OCR 自动填充表单 |
| 5 | 上传/下载证件 | 加解密正确 |
| 6 | 导航到 `/finance` | 添加收支记录,汇总卡更新 |
| 7 | 导航到 `/health` | 添加健康档案 |
| 8 | 导航到 `/work` | 任务看板,状态切换 |
| 9 | 切换小孩模式 | NavMenu 变为"学习辅导"+"互动游戏" |
| 10 | 导航到 `/education` | 辅导聊天,问答保存到历史 |
| 11 | 导航到 `/chat` | 会话列表,对话持久化 |
| 12 | API:GET `/api/finance/records` | 返回 JSON |
| 13 | API:GET `/api/conversations/sessions` | 返回 JSON |
| 14 | Agent 对话问"家庭成员有哪些" | 插件调用返回真实数据 |
| 15 | 大人模式财务记录,切换小孩模式直接访问 `/finance` URL | 后端 Mode 过滤返回空 |

## 风险与缓解

| 风险 | 级别 | 缓解 |
|------|------|------|
| PaddleOCRSharp NuGet 兼容性 | 高 | 先测试包;备选 ONNX Runtime 直接加载三阶段管线 |
| 模型下载(HuggingFace 国内访问) | 中 | 提供 hf-mirror.com 镜像 |
| SQLite 并发(ApiBootstrap vs Host DbContext) | 中 | 连接字符串加 `Cache=Shared` |
| 修改实体类型后旧 .db schema 不匹配 | 中 | 实施前删除 `data/*.db` 重新创建 |
| BGE tokenizer 中文分词一致性 | 中 | 用 Python sentence-transformers 对比验证余弦相似度 >0.99 |
| Agent Singleton + Scoped 服务 | 低 | 插件每次调用 CreateScope 并 Dispose |

## 实施顺序

按批次 A→B→C→D→E→F 顺序实施,每批完成后 `dotnet build` 验证。批次 F(模型下载)可与 E 并行进行。

## 关键文件清单

**修改**(11 个):
- [FamilySuper.Core/Entities/FamilyMember.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Core/Entities/FamilyMember.cs) - Gender 改枚举
- [FamilySuper.Core/Entities/FinanceRecord.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Core/Entities/FinanceRecord.cs) - Type 改枚举
- [FamilySuper.Data/Context/FamilyDbContext.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/Context/FamilyDbContext.cs) - 修复查询过滤器 + 新增 DbSet
- [FamilySuper.Data/Context/HealthDbContext.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/Context/HealthDbContext.cs) - 注入 IModeManager + DbSet
- [FamilySuper.Data/Context/EducationDbContext.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/Context/EducationDbContext.cs) - 同上
- [FamilySuper.Data/Context/KnowledgeDbContext.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/Context/KnowledgeDbContext.cs) - 同上
- [FamilySuper.Data/DbInitializer.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Data/DbInitializer.cs) - 种子数据用枚举
- [FamilySuper.Infrastructure/Services/OcrService.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Infrastructure/Services/OcrService.cs) - 真实 PaddleOCR
- [FamilySuper.Infrastructure/Services/BgeEmbeddingService.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Infrastructure/Services/BgeEmbeddingService.cs) - 真实 BGE
- [FamilySuper.Infrastructure/ServiceCollectionExtensions.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Infrastructure/ServiceCollectionExtensions.cs) - 注册业务服务
- [FamilySuper.Agent/KernelBuilder.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/KernelBuilder.cs) - 注册插件
- [FamilySuper.Agent/AgentService.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/AgentService.cs) - 新增 systemPromptOverride
- [FamilySuper.Agent/MemoryManager.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/MemoryManager.cs) - SQLite 持久化
- [FamilySuper.Agent/Plugins/FamilyTools.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/Plugins/FamilyTools.cs) - 真实查询
- [FamilySuper.Agent/Plugins/FinanceTools.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/Plugins/FinanceTools.cs) - 真实查询
- [FamilySuper.Agent/Plugins/HealthTools.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Agent/Plugins/HealthTools.cs) - 真实查询
- [FamilySuper.API/ApiBootstrap.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.API/ApiBootstrap.cs) - 注册 DbContext + 端点
- [FamilySuper.BlazorUI/_Imports.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/_Imports.razor) - 追加 using
- [FamilySuper.BlazorUI/Pages/Chat.razor](file:///d:/study/.net%20core/HomeAgent/FamilySuper.BlazorUI/Pages/Chat.razor) - 集成 ConversationService
- [FamilySuper.Host.WPF/App.xaml.cs](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/App.xaml.cs) - 调用 AddFamilySuperData
- [FamilySuper.Host.WPF/appsettings.json](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/appsettings.json) - OCR/Embedding 配置
- [FamilySuper.Host.WPF/FamilySuper.Host.WPF.csproj](file:///d:/study/.net%20core/HomeAgent/FamilySuper.Host.WPF/FamilySuper.Host.WPF.csproj) - 模型文件复制

**新建**(约 25 个):
- Core/Enums/: 7 个枚举文件
- Core/Entities/: 7 个新实体文件
- Core/Interfaces/: 6 个业务服务接口
- Data/ServiceCollectionExtensions.cs
- Infrastructure/Services/: 6 个业务服务实现
- API/Endpoints/: 6 个新端点文件
- BlazorUI/Pages/: 6 个新页面(Family/Finance/Health/Work/Education/Games)
