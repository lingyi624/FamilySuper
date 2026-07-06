# 家庭超级管家智能体 - 第一阶段实现计划

## Context

需求文档 `D:\study\.net core\HomeAgent\家庭超级管家智能体 —— 需求规格与技术架构说明书.txt` 定义了一个本地优先、隐私保护的家庭智能中枢系统,包含13个功能模块和8个工程子项目,完整实现需数月团队工作量。

**本阶段目标**:搭建完整8工程解决方案骨架,聚焦实现 WPF+Blazor Hybrid 宿主框架(可编译运行),为后续模块开发奠定基础。所有外部服务(DeepSeek API、PaddleOCR、frp、BGE)采用真实集成,通过配置驱动。

**用户确认的关键决策**:
- 使用 .NET 10 (SDK 10.0.300 已安装)
- 第一阶段聚焦 WPF+Blazor Hybrid 宿主框架
- 完整集成真实外部服务(API Key 通过配置注入)

## 解决方案结构

工作目录: `D:\study\.net core\HomeAgent`

```
FamilySuper管家.sln
├── FamilySuper.Core/              # net10.0 - 核心实体/接口/枚举(无依赖)
├── FamilySuper.Data/              # net10.0 - EF Core + SQLite (依赖 Core)
├── FamilySuper.Infrastructure/    # net10.0 - 加密/OCR/frp/嵌入 (依赖 Core)
├── FamilySuper.Agent/             # net10.0 - Semantic Kernel + DeepSeek (依赖 Core/Data/Infrastructure)
├── FamilySuper.BlazorUI/          # net10.0 - Razor类库 (依赖 Core)
├── FamilySuper.API/               # net10.0 - ASP.NET Core API (依赖 Core/Data/Agent/Infrastructure)
├── FamilySuper.Host.WPF/          # net10.0-windows - 启动入口 (引用所有)
└── FamilySuper.H5/                # Vue3 占位 README,本阶段留空
```

依赖链: Core ← {Data, Infrastructure, BlazorUI} ← Agent ← API ← Host.WPF

## 各工程实现清单

### 1. FamilySuper.Core
- `Enums/AppMode.cs` - 大人/小孩模式枚举
- `Entities/EntityBase.cs` - 基类(Id, CreatedAt, UpdatedAt, IsDeleted, Mode)
- `Entities/FamilyMember.cs` - 家庭成员实体
- `Entities/SystemConfig.cs` - 系统配置实体
- `Entities/FinanceRecord.cs` - 财务记录实体(用于演示模式过滤)
- `Interfaces/IModeManager.cs` - 模式管理接口
- `Interfaces/IEncryptionService.cs` - 加密服务接口
- `Interfaces/IAgentService.cs` - 智能体服务接口
- `Interfaces/IRepository{T}.cs` - 泛型仓储接口

### 2. FamilySuper.Data
- `Context/FamilyDbContext.cs` - 主库(family.db),注入IModeManager实现查询过滤
- `Context/FinanceDbContext.cs` - 财务库(finance.db)
- `Context/HealthDbContext.cs` - 健康库(health.db,预留空结构)
- `Context/EducationDbContext.cs` - 教育库(education.db,预留)
- `Context/KnowledgeDbContext.cs` - 知识库(knowledge.db,预留)
- `Repositories/Repository{T}.cs` - 仓储实现
- `DbInitializer.cs` - 数据库初始化(EnsureCreated + 种子数据)

### 3. FamilySuper.Infrastructure
- `EncryptionService.cs` - AES-256 加密 + BCrypt 密码哈希
- `ModeManager.cs` - 模式管理器实现(线程安全 + 观察者模式)
- `Services/OcrService.cs` - PaddleOCRSharp 封装(身份证识别)
- `Services/FrpClientService.cs` - frp 进程托管(BackgroundService)
- `Services/BgeEmbeddingService.cs` - ONNX Runtime + BGE-small-zh 嵌入

### 4. FamilySuper.Agent
- `KernelBuilder.cs` - Semantic Kernel 构建(注册 DeepSeek V4 Pro + Plugins)
- `AgentService.cs` - 对话服务(支持模式专属系统提示词 + 记忆检索 + reasoning_effort)
- `MemoryManager.cs` - 记忆管理(预留 LanceDB/Joust 接口)
- `Plugins/FamilyTools.cs` - 家庭信息工具(函数调用)
- `Plugins/FinanceTools.cs` - 财务工具
- `Plugins/HealthTools.cs` - 健康工具

### 5. FamilySuper.BlazorUI
- `App.razor` - 根组件
- `_Imports.razor` - 全局 using
- `Components/Layout/MainLayout.razor` - 主布局(顶栏 + 侧边栏 + 内容区)
- `Components/Layout/NavMenu.razor` - 导航菜单(根据模式动态显示)
- `Pages/Home.razor` - 首页(显示当前模式、家庭成员概览)
- `Pages/ModeSwitch.razor` - 模式切换页(密码验证)
- `Pages/Chat.razor` - 智能体对话演示页(调用 AgentService)
- `Services/ModeStateContainer.cs` - Blazor 状态容器(订阅 ModeManager 事件)

### 6. FamilySuper.API
- `Program.cs` - 最小 API(Kestrel,供 H5/frp 访问)
- `ApiBootstrap.cs` - 静态方法 Build(sp, config) 复用 Host 的 ServiceProvider
- `Middlewares/ModeFilterMiddleware.cs` - 模式过滤中间件
- `Endpoints/FamilyEndpoints.cs` - 家庭成员 CRUD 端点
- `Endpoints/HealthEndpoints.cs` - /health 健康检查

### 7. FamilySuper.Host.WPF (启动入口)
- `App.xaml` / `App.xaml.cs` - DI 容器装配 + API 后台启动 + 生命周期管理
- `MainWindow.xaml` / `MainWindow.xaml.cs` - 主窗口(WPF标题栏 + BlazorWebView)
- `Services/SystemTrayService.cs` - 系统托盘(NotifyIcon)
- `Services/HotKeyService.cs` - 全局热键(Win32 RegisterHotKey, Ctrl+Alt+H 唤出)
- `Services/NativeNotificationService.cs` - Windows 原生通知
- `wwwroot/index.html` - Blazor 静态宿主页(div#app)
- `appsettings.json` - 配置(DeepSeek:ApiKey, Frp:*, Admin:PasswordHash, ConnectionStrings)
- `FamilySuper.Host.WPF.csproj` - x64 平台目标

### 8. FamilySuper.H5
- `README.md` - 占位说明,本阶段不实现

## 关键实现要点

### MainWindow.xaml - BlazorWebView 集成
```xml
<blazor:BlazorWebView Grid.Row="1" HostPage="wwwroot/index.html"
                      x:Name="blazorWebView">
    <blazor:BlazorWebView.RootComponents>
        <blazor:RootComponent Selector="#app" ComponentType="{x:Type ui:App}" />
    </blazor:BlazorWebView.RootComponents>
</blazor:BlazorWebView>
```

### App.xaml.cs - DI 装配
- ServiceCollection 注册所有服务(Core/Data/Agent/Infrastructure/BlazorUI)
- `AddWpfBlazorWebView()` 启用 Blazor WebView
- 各 DbContext 使用对应连接字符串
- AgentService 单例(复用 Kernel)
- ApiBootstrap.Build() 后台启动 Kestrel(localhost:5000)
- ServiceProvider 设置为 BlazorWebView 的 Services

### ModeManager - 线程安全 + 观察者模式
- SemaphoreSlim 保护状态切换
- BCrypt 验证管理员密码
- ModeChanged 事件通知观察者(WPF/Blazor)
- 切换后清空缓存

### FamilyDbContext - 模式隔离全局过滤器
- 注入 IModeManager
- FinanceRecord 实体 `HasQueryFilter(e => e.Mode == mode.CurrentMode.ToString())`
- 所有软删除实体 `HasQueryFilter(e => !e.IsDeleted)`

### AgentService - DeepSeek V4 Pro 真实集成
- 通过 Microsoft.SemanticKernel + OpenAI 兼容客户端
- modelId: "deepseek-v4-pro", endpoint: https://api.deepseek.com/v1
- API Key 从 IConfiguration["DeepSeek:ApiKey"] 读取
- 启动时校验非空,缺失则降级为 Mock 返回提示信息
- 支持模式专属系统提示词(adult/child)
- KernelArguments["reasoning_effort"] = "high"

### 配置驱动的外部服务
- appsettings.json 包含所有配置项(DeepSeek/Frp/Admin/ConnectionStrings)
- API Key 留空时降级 Mock,非空时启用真实服务
- 启动时 Serilog 日志记录各服务启用状态

## 验证步骤

```powershell
# 1. 创建解决方案与8工程
cd "D:\study\.net core\HomeAgent"
dotnet new sln -n FamilySuper管家
dotnet new wpf -n FamilySuper.Host.WPF -f net10.0-windows -o FamilySuper.Host.WPF
dotnet new razorclasslib -n FamilySuper.BlazorUI -f net10.0 -o FamilySuper.BlazorUI
dotnet new classlib -n FamilySuper.Core -f net10.0 -o FamilySuper.Core
# ... 其余5个 classlib 同理

# 2. 添加项目引用(按依赖链)
dotnet add FamilySuper.Data reference FamilySuper.Core
dotnet add FamilySuper.Infrastructure reference FamilySuper.Core
dotnet add FamilySuper.Agent reference FamilySuper.Core FamilySuper.Data FamilySuper.Infrastructure
dotnet add FamilySuper.BlazorUI reference FamilySuper.Core
dotnet add FamilySuper.API reference FamilySuper.Core FamilySuper.Data FamilySuper.Agent FamilySuper.Infrastructure
dotnet add FamilySuper.Host.WPF reference FamilySuper.API FamilySuper.BlazorUI FamilySuper.Agent FamilySuper.Data FamilySuper.Infrastructure FamilySuper.Core

# 3. 添加 NuGet 包(各工程按需)
# Host.WPF: Microsoft.AspNetCore.Components.WebView.Wpf
# Data: Microsoft.EntityFrameworkCore.Sqlite
# Agent: Microsoft.SemanticKernel, Microsoft.SemanticKernel.Connectors.OpenAI
# Infrastructure: PaddleOCRSharp, BCrypt.Net-Next, Microsoft.ML.OnnxRuntime
# 全局: Serilog.Extensions.Logging

# 4. 编译
dotnet build

# 5. 运行
dotnet run --project FamilySuper.Host.WPF
```

**验证清单**:
- [ ] 解决方案 8 工程编译通过
- [ ] WPF 主窗口启动,Blazor 首页渲染
- [ ] 模式切换页:输入错误密码拒绝,正确密码后顶部显示当前模式
- [ ] 系统托盘:最小化到托盘,右键菜单退出
- [ ] 全局热键 Ctrl+Alt+H 唤出窗口
- [ ] Kestrel 监听 http://localhost:5000/health 返回 200
- [ ] 对话页:无 API Key 时返回 Mock 提示,有 Key 时真实对话
- [ ] 数据库文件 data/family.db 自动创建,种子数据可查询

## 风险点与注意事项

1. **BlazorWebView HostPage 路径** - wwwroot/index.html 必须在 Host.WPF 项目,div#app 与 RootComponent Selector 严格匹配,否则白屏
2. **API Key 安全** - appsettings.json 留空,开发用 `dotnet user-secrets`,生产用 DPAPI;启动校验+降级 Mock
3. **多 DbContext 独立事务** - 5 个 SQLite 库各自独立,禁止跨库共享事务
4. **API 与 WPF 同进程** - Kestrel 复用 Host 的 ServiceProvider,通过 ApiBootstrap.Build 静态方法装配
5. **原生依赖平台目标** - PaddleOCR/OnnxRuntime 需 `<PlatformTarget>x64</PlatformTarget>`,模型文件放 models/ 目录,首次启动异步预加载避免卡 UI
6. **BlazorWebView NuGet 版本** - `Microsoft.AspNetCore.Components.WebView.Wpf` 需 .NET 10 兼容版本,锁定版本号
7. **ModeManager 跨线程** - WPF UI 线程与 Blazor 渲染线程同步 ModeChanged 事件需 Dispatcher.Invoke
8. **路径含空格** - 工作目录 `D:\study\.net core\HomeAgent` 含空格,所有命令需正确引号
9. **中文项目名** - 解决方案名 `FamilySuper管家.sln` 含中文,确保文件系统编码正确(若出问题改用纯英文 FamilySuper.sln)

## 后续阶段(本次不实现)

- 第二阶段:Core MVP 功能(家庭信息管理 P0 + 财务 P0 + 健康档案 P0)
- 第三阶段:智能体编排完善(LanceDB 记忆 + 工具调用 + 主动推送)
- 第四阶段:教育/养老/工作模块
- 第五阶段:H5 移动端 + 内网穿透
- 第六阶段:家庭调研、3D 虚拟实景等 P2 功能
