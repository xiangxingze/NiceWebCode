# NiceWebCode - 快速启动指南

## 前端完整实现完成 ✅

前端React应用已经完全实现，包含以下功能：

### 已实现功能

1. **会话管理**
   - 创建新会话
   - 选择会话
   - 删除会话
   - 会话列表显示

2. **实时聊天界面**
   - 输入提示词
   - 实时显示AI输出
   - 多模态输出渲染（文本、代码、Markdown、错误信息）
   - 自动滚动到最新输出

3. **SignalR实时通信**
   - WebSocket连接管理
   - 自动重连机制
   - 连接状态指示器
   - 会话组管理

4. **UI/UX**
   - 响应式布局
   - 现代化界面设计
   - 代码语法高亮
   - Markdown渲染支持

## 当前运行状态

### 后端 API
- 状态: ✅ **运行中**
- 地址:
  - HTTP: http://localhost:5000
  - HTTPS: https://localhost:5001
  - Swagger: https://localhost:5001/swagger
  - SignalR Hub: http://localhost:5000/hubs/output

### 前端 WebUI
- 状态: ✅ **运行中**
- 地址: http://localhost:3000
- 开发服务器: Vite (热重载已启用)

## 访问应用

打开浏览器访问: **http://localhost:3000**

## 使用说明

### 1. 创建会话
1. 点击左侧边栏的 "➕ 新建会话" 按钮
2. 输入会话标题
3. 点击 "创建"

### 2. 开始对话
1. 在左侧选择一个会话（会话会高亮显示）
2. 在底部输入框输入提示词
3. 点击 "发送" 或按 Enter
4. 实时查看AI响应输出

### 3. 查看输出
- **文本输出**: 普通文本显示
- **代码输出**: 带语法高亮的代码块
- **Markdown输出**: 格式化的Markdown内容
- **错误输出**: 红色背景显示错误信息
- **系统输出**: 蓝色背景显示系统消息

### 4. 连接状态
- 左上角状态指示器:
  - 🟢 绿色脉动 = 已连接到SignalR
  - 🔴 红色 = 未连接

## 项目结构

```
NiceWebCode/
├── src/
│   ├── NiceWebCode.Domain/          # 领域层（实体模型）
│   ├── NiceWebCode.Application/     # 应用层（业务逻辑）
│   ├── NiceWebCode.Infrastructure/  # 基础设施层（数据访问、CLI适配器）
│   ├── NiceWebCode.WebApi/          # Web API层（控制器、SignalR Hub）
│   └── NiceWebCode.WebUI/           # React前端
│       ├── src/
│       │   ├── components/          # React组件
│       │   │   ├── Sidebar.tsx      # 会话列表侧边栏
│       │   │   ├── ChatPanel.tsx    # 聊天面板
│       │   │   └── OutputRenderer.tsx # 输出渲染器
│       │   ├── services/            # 服务层
│       │   │   ├── api.ts           # REST API客户端
│       │   │   └── signalr.ts       # SignalR客户端
│       │   ├── store/               # 状态管理
│       │   │   └── useAppStore.ts   # Zustand store
│       │   ├── types/               # TypeScript类型定义
│       │   ├── App.tsx              # 主应用组件
│       │   └── App.css              # 样式文件
│       ├── .env                     # 环境变量配置
│       └── vite.config.ts           # Vite配置
└── workspaces/                      # 会话工作区目录
```

## 技术栈

### 后端
- .NET 9.0
- ASP.NET Core Web API
- SignalR (实时通信)
- Entity Framework Core (SQLite)
- Clean Architecture

### 前端
- React 18.3
- TypeScript
- Vite 5.4
- Zustand (状态管理)
- Axios (HTTP客户端)
- SignalR JavaScript客户端
- React-Markdown (Markdown渲染)
- Prism.js (代码语法高亮)

## 开发环境要求

- .NET SDK 9.0+
- Node.js 18.20+ (当前使用)
- npm 或 yarn

## 下一步计划

当前MVP已经完成基础功能实现，可以考虑以下增强：

1. **工作区文件管理器** (可选)
   - 文件树展示
   - 文件内容预览
   - 文件下载

2. **HTML预览功能** (可选)
   - 在iframe中预览生成的HTML文件
   - 实时刷新

3. **CLI工具集成** (需要)
   - 配置Claude Code CLI路径
   - 实际执行CLI命令并获取输出

4. **用户认证** (生产环境需要)
   - 用户登录/注册
   - JWT认证

5. **性能优化**
   - 虚拟滚动（长输出列表）
   - 输出分页加载

## 故障排查

### 前端无法连接到后端
1. 检查后端API是否在运行（端口5000/5001）
2. 检查浏览器控制台是否有CORS错误
3. 确认.env文件中的API URL配置正确

### SignalR连接失败
1. 检查浏览器开发者工具的Network标签
2. 查看是否有WebSocket连接错误
3. 确认后端SignalR Hub配置正确

### 会话创建失败
1. 检查数据库文件是否可写
2. 查看后端日志
3. 确认workspaces目录有写权限

## 停止服务

如需停止服务：

1. **停止后端**: 在运行`dotnet run`的终端按 `Ctrl+C`
2. **停止前端**: 在运行`npm run dev`的终端按 `Ctrl+C`

## 联系支持

如遇到问题，请查看：
- 后端日志: 终端输出
- 前端日志: 浏览器开发者工具控制台
- API文档: https://localhost:5001/swagger
