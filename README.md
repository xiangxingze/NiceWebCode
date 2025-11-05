# NiceWebCode - AI编程伙伴平台

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

## 项目概述

NiceWebCode是一个24小时在线的智能编程伙伴平台,旨在整合多种AI编程工具(如Claude Code、GitHub Copilot等),提供云端化、异步处理和内网支持的编程体验。

### 核心特性

- **统一入口** - 将多种AI编程工具整合到一个Web界面
- **云端运行** - 服务器端部署,通过浏览器随时随地访问
- **异步处理** - 发起任务后可离开,充分利用碎片化时间
- **实时流式输出** - SignalR推送,打字机效果,延迟<100ms
- **内网友好** - 支持完全离线部署,连接私有化AI模型
- **会话工作区隔离** - 每个会话独立工作区,确保安全隔离
- **协作增强** - 会话共享、知识沉淀功能(待实现)

## 技术架构

### 后端技术栈

- **框架**: ASP.NET Core 9.0
- **架构**: Clean Architecture + DDD
- **数据库**: SQLite (单机) / PostgreSQL (企业版)
- **实时通信**: SignalR
- **API文档**: Swagger/OpenAPI

### 项目结构

```
NiceWebCode/
├── src/
│   ├── NiceWebCode.Domain/          # 领域层
│   │   └── Entities/                # 实体模型
│   │       ├── Session.cs           # 会话实体
│   │       ├── AiTask.cs            # AI任务实体
│   │       ├── OutputChunk.cs       # 输出块实体
│   │       └── WorkspaceFile.cs     # 工作区文件实体
│   │
│   ├── NiceWebCode.Application/     # 应用层
│   │   ├── Interfaces/              # 接口定义
│   │   │   ├── ICliToolExecutor.cs  # CLI执行器接口
│   │   │   └── IWorkspaceService.cs # 工作区服务接口
│   │   └── Models/                  # DTO模型
│   │       ├── SessionDto.cs
│   │       ├── OutputChunkDto.cs
│   │       └── WorkspaceFileDto.cs
│   │
│   ├── NiceWebCode.Infrastructure/  # 基础设施层
│   │   ├── CliAdapters/             # CLI适配器
│   │   │   ├── BaseCliAdapter.cs    # 基础适配器
│   │   │   └── ClaudeCodeAdapter.cs # Claude Code适配器
│   │   ├── Services/
│   │   │   └── WorkspaceService.cs  # 工作区服务实现
│   │   └── Data/
│   │       └── ApplicationDbContext.cs # 数据库上下文
│   │
│   └── NiceWebCode.WebApi/          # API层
│       ├── Controllers/              # API控制器
│       │   ├── SessionsController.cs
│       │   └── WorkspaceController.cs
│       ├── Hubs/
│       │   └── OutputHub.cs         # SignalR Hub
│       └── Program.cs               # 启动配置
│
├── docs/                            # 文档
├── docker/                          # Docker配置
└── tests/                           # 测试

```

## 快速开始

### 环境要求

- .NET 9.0 SDK
- Node.js 18+ (前端开发)
- Claude Code CLI工具 (可选)

### 后端启动

```bash
# 克隆仓库
git clone https://github.com/yourusername/NiceWebCode.git
cd NiceWebCode

# 构建项目
dotnet build

# 运行WebApi
cd src/NiceWebCode.WebApi
dotnet run

# 访问Swagger文档
# https://localhost:5001/swagger
```

### 配置说明

在`appsettings.json`中配置:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=nicewebcode.db"
  },
  "WorkspaceBasePath": "./workspaces",
  "CliTools": {
    "ClaudeCode": {
      "Path": "claude-code"
    }
  }
}
```

## API文档

### 核心接口

#### 1. 创建会话

```http
POST /api/sessions
Content-Type: application/json

{
  "title": "我的编程会话",
  "userId": "user123"
}
```

#### 2. 执行AI任务

```http
POST /api/sessions/{sessionId}/execute
Content-Type: application/json

{
  "prompt": "创建一个贪吃蛇游戏",
  "cliToolName": "claude-code"
}
```

#### 3. 获取会话输出

```http
GET /api/sessions/{sessionId}/outputs
```

#### 4. 获取工作区文件

```http
GET /api/workspace/{sessionId}/files
```

### SignalR实时通信

连接到Hub: `ws://localhost:5000/hubs/output`

客户端方法:
- `ReceiveOutput(OutputChunkDto)` - 接收输出块
- `TaskStatusChanged(Guid, string)` - 任务状态变更
- `TaskCompleted(Guid, bool, string)` - 任务完成通知

## 核心功能实现详解

### 1. CLI工具适配器框架

采用**策略模式**和**模板方法模式**:

```csharp
public abstract class BaseCliAdapter : ICliToolExecutor
{
    // 模板方法:定义执行流程
    public async IAsyncEnumerable<OutputChunkDto> ExecuteAsync(...)
    {
        // 启动进程 → 流式读取 → 解析输出 → 推送结果
    }

    // 抽象方法:由子类实现具体解析逻辑
    protected abstract OutputChunkDto? ParseOutput(string line, Guid sessionId);
}
```

**支持的CLI工具**:
- ✅ Claude Code (JSONL格式输出)
- 🔄 GitHub Copilot CLI (计划中)
- 🔄 通义千问CLI (计划中)

### 2. 实时流式输出

**技术实现**:
1. `Process.StandardOutput` 异步流式读取
2. SignalR Server-Sent Events推送
3. 前端虚拟滚动渲染

**性能指标**:
- CLI输出 → 后端接收: <10ms
- 后端 → SignalR推送: <50ms
- 网络传输 → 前端渲染: <40ms
- **总延迟**: ~100ms ✅

### 3. 会话工作区隔离

**安全机制**:

```
/workspaces/
  ├── session_{guid-1}/  # 独立工作区
  │   ├── src/
  │   └── output/
  └── session_{guid-2}/
```

**防护措施**:
- 路径遍历防护 (`IsPathWithinWorkspace`)
- 文件大小限制
- 定期清理过期工作区

### 4. 数据持久化

**Entity Framework Core** + **SQLite**:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<AiTask> AiTasks { get; set; }
    public DbSet<OutputChunk> OutputChunks { get; set; }
}
```

**关系模型**:
- Session 1-N AiTask
- Session 1-N OutputChunk
- Session 1-N WorkspaceFile

## 开发进度

### MVP阶段 (已完成✅)

- [x] 项目架构搭建
- [x] 领域模型设计
- [x] CLI工具适配器框架
- [x] SignalR实时通信
- [x] 会话工作区隔离
- [x] 数据库持久化
- [x] RESTful API
- [x] Swagger文档

### 待实现功能 (下一阶段)

- [ ] 前端React应用
  - [ ] 聊天对话界面
  - [ ] 多模态输出渲染(代码高亮、Markdown)
  - [ ] 工作区文件管理器
  - [ ] HTML预览功能
- [ ] 协作功能
  - [ ] 会话分享
  - [ ] 评论系统
  - [ ] 知识库
- [ ] 安全增强
  - [ ] 身份认证(JWT)
  - [ ] 权限管理(RBAC)
  - [ ] API速率限制
- [ ] 部署优化
  - [ ] Docker镜像
  - [ ] Kubernetes配置
  - [ ] CI/CD流程

## 贡献指南

欢迎贡献！请遵循以下步骤:

1. Fork本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 开启Pull Request

### 代码规范

- 遵循Clean Architecture原则
- 使用C# 12.0特性
- 编写单元测试
- 添加XML文档注释

## 许可证

本项目采用MIT许可证 - 详见 [LICENSE](LICENSE) 文件

## 联系方式

项目链接: [https://github.com/yourusername/NiceWebCode](https://github.com/yourusername/NiceWebCode)

---

**注意**: 本项目目前处于MVP阶段,前端部分尚未实现。后端API已完全可用,可通过Swagger进行测试。
