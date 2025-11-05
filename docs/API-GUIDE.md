# NiceWebCode API 使用指南

## 基础信息

- **Base URL**: `https://localhost:5001/api`
- **协议**: HTTPS
- **数据格式**: JSON
- **实时通信**: SignalR WebSocket

## API端点

### 1. 会话管理 (Sessions)

#### 1.1 创建会话

```http
POST /api/sessions
Content-Type: application/json

{
  "title": "我的编程会话",
  "userId": "user123"
}
```

**响应示例：**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "我的编程会话",
  "userId": "user123",
  "createdAt": "2025-01-05T10:00:00Z",
  "updatedAt": "2025-01-05T10:00:00Z",
  "status": "Active"
}
```

#### 1.2 获取所有会话

```http
GET /api/sessions?userId=user123
```

**响应示例：**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "我的编程会话",
    "userId": "user123",
    "createdAt": "2025-01-05T10:00:00Z",
    "updatedAt": "2025-01-05T10:00:00Z",
    "status": "Active"
  }
]
```

#### 1.3 获取单个会话

```http
GET /api/sessions/{sessionId}
```

#### 1.4 删除会话

```http
DELETE /api/sessions/{sessionId}
```

---

### 2. 任务执行

#### 2.1 执行AI任务

```http
POST /api/sessions/{sessionId}/execute
Content-Type: application/json

{
  "prompt": "创建一个简单的贪吃蛇游戏",
  "cliToolName": "claude-code"
}
```

**响应示例：**
```json
{
  "taskId": "8b3d7f92-1234-5678-9abc-def012345678"
}
```

**说明：**
- 任务将在后台异步执行
- 通过SignalR实时推送输出
- 可以通过`taskId`查询任务状态

#### 2.2 获取会话输出历史

```http
GET /api/sessions/{sessionId}/outputs
```

**响应示例：**
```json
[
  {
    "id": "output-1",
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "taskId": "8b3d7f92-1234-5678-9abc-def012345678",
    "type": "Text",
    "content": "正在创建贪吃蛇游戏...",
    "createdAt": "2025-01-05T10:01:00Z",
    "sequenceNumber": 1
  },
  {
    "id": "output-2",
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "taskId": "8b3d7f92-1234-5678-9abc-def012345678",
    "type": "Code",
    "content": "<!DOCTYPE html>\n<html>...",
    "createdAt": "2025-01-05T10:01:05Z",
    "sequenceNumber": 2
  }
]
```

---

### 3. 工作区管理

#### 3.1 获取工作区文件列表

```http
GET /api/workspace/{sessionId}/files
```

**响应示例：**
```json
[
  {
    "relativePath": "snake.html",
    "fileName": "snake.html",
    "fileSize": 5120,
    "contentType": "text/html",
    "updatedAt": "2025-01-05T10:05:00Z",
    "isDirectory": false
  },
  {
    "relativePath": "src",
    "fileName": "src",
    "fileSize": 0,
    "contentType": "directory",
    "updatedAt": "2025-01-05T10:04:00Z",
    "isDirectory": true,
    "children": [
      {
        "relativePath": "src/game.js",
        "fileName": "game.js",
        "fileSize": 2048,
        "contentType": "application/javascript",
        "updatedAt": "2025-01-05T10:04:30Z",
        "isDirectory": false
      }
    ]
  }
]
```

#### 3.2 读取文件内容

```http
GET /api/workspace/{sessionId}/files/content?path=snake.html
```

**响应示例：**
```json
{
  "content": "<!DOCTYPE html>\n<html>\n<head>...</head>\n<body>...</body>\n</html>"
}
```

---

## SignalR 实时通信

### 连接到Hub

```javascript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:5001/hubs/output')
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### 加入会话组

```javascript
await connection.invoke('JoinSession', sessionId);
```

### 监听事件

#### 1. 接收输出块

```javascript
connection.on('ReceiveOutput', (outputChunk) => {
  console.log('收到输出:', outputChunk);
  // outputChunk 结构与 OutputChunkDto 相同
});
```

#### 2. 任务状态变更

```javascript
connection.on('TaskStatusChanged', (taskId, status) => {
  console.log(`任务 ${taskId} 状态变更为: ${status}`);
  // status: "Pending" | "Running" | "Completed" | "Failed"
});
```

#### 3. 任务完成通知

```javascript
connection.on('TaskCompleted', (taskId, success, errorMessage) => {
  if (success) {
    console.log(`任务 ${taskId} 完成`);
  } else {
    console.error(`任务 ${taskId} 失败: ${errorMessage}`);
  }
});
```

### 离开会话组

```javascript
await connection.invoke('LeaveSession', sessionId);
```

---

## 完整示例：创建会话并执行任务

### 1. REST API 方式

```javascript
// 1. 创建会话
const createSessionResponse = await fetch('https://localhost:5001/api/sessions', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    title: '贪吃蛇开发',
    userId: 'user123'
  })
});
const session = await createSessionResponse.json();
const sessionId = session.id;

// 2. 执行任务
const executeResponse = await fetch(`https://localhost:5001/api/sessions/${sessionId}/execute`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    prompt: '创建一个贪吃蛇游戏',
    cliToolName: 'claude-code'
  })
});
const { taskId } = await executeResponse.json();

// 3. 轮询获取输出（不推荐，建议使用SignalR）
setInterval(async () => {
  const outputsResponse = await fetch(`https://localhost:5001/api/sessions/${sessionId}/outputs`);
  const outputs = await outputsResponse.json();
  console.log('最新输出:', outputs[outputs.length - 1]);
}, 1000);
```

### 2. SignalR 实时方式（推荐）

```javascript
import * as signalR from '@microsoft/signalr';

// 1. 建立SignalR连接
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:5001/hubs/output')
  .withAutomaticReconnect()
  .build();

// 2. 监听输出
connection.on('ReceiveOutput', (chunk) => {
  console.log('实时输出:', chunk.content);
  // 渲染到UI
  appendToOutput(chunk);
});

connection.on('TaskCompleted', (taskId, success, error) => {
  if (success) {
    console.log('任务完成！');
  } else {
    console.error('任务失败:', error);
  }
});

await connection.start();

// 3. 创建会话
const session = await createSession('贪吃蛇开发', 'user123');

// 4. 加入会话组
await connection.invoke('JoinSession', session.id);

// 5. 执行任务
await executeTask(session.id, '创建一个贪吃蛇游戏');

// 6. 等待实时输出...（自动接收）
```

---

## 错误处理

### HTTP状态码

- `200 OK` - 请求成功
- `201 Created` - 资源创建成功
- `204 No Content` - 删除成功
- `400 Bad Request` - 请求参数错误
- `404 Not Found` - 资源不存在
- `500 Internal Server Error` - 服务器错误

### 错误响应格式

```json
{
  "error": "错误描述信息"
}
```

---

## 最佳实践

### 1. 使用SignalR进行实时通信
- 避免频繁轮询API
- 减少服务器负载
- 提供更好的用户体验

### 2. 错误处理
```javascript
try {
  await connection.start();
} catch (err) {
  console.error('SignalR连接失败:', err);
  // 降级到轮询方式
  fallbackToPolling();
}
```

### 3. 自动重连
```javascript
connection.onreconnecting(() => {
  console.log('正在重连...');
});

connection.onreconnected(() => {
  console.log('重连成功');
  // 重新加入会话组
  connection.invoke('JoinSession', currentSessionId);
});
```

### 4. 资源清理
```javascript
// 页面卸载时清理
window.addEventListener('beforeunload', async () => {
  await connection.invoke('LeaveSession', sessionId);
  await connection.stop();
});
```

---

## 开发工具

### Swagger UI

访问 `https://localhost:5001/swagger` 可以：
- 查看完整API文档
- 在线测试API接口
- 查看请求/响应模型

### Postman Collection

可以导入以下cURL命令到Postman：

```bash
# 创建会话
curl -X POST https://localhost:5001/api/sessions \
  -H "Content-Type: application/json" \
  -d '{"title":"测试会话","userId":"user123"}'

# 执行任务
curl -X POST https://localhost:5001/api/sessions/{sessionId}/execute \
  -H "Content-Type: application/json" \
  -d '{"prompt":"创建贪吃蛇","cliToolName":"claude-code"}'
```

---

## 常见问题

### Q: SignalR连接失败？
A: 检查CORS配置，确保前端域名在允许列表中。

### Q: 任务执行没有输出？
A: 确保已加入会话组 (`JoinSession`)，并正确监听 `ReceiveOutput` 事件。

### Q: 工作区文件无法访问？
A: 检查文件路径是否包含非法字符，确保路径在工作区范围内。

---

更多详细信息，请参阅主文档 [README.md](../README.md)
