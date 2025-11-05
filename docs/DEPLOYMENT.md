# NiceWebCode 部署指南

## 部署方式概览

NiceWebCode支持多种部署方式：

1. **本地开发部署** - 用于开发和测试
2. **Docker部署** - 推荐用于生产环境
3. **Windows Server部署** - 企业内网环境
4. **Linux服务器部署** - 云服务器或私有服务器

---

## 1. 本地开发部署

### 环境要求

- .NET 9.0 SDK
- Visual Studio 2022 / VS Code / Rider
- Claude Code CLI (可选，用于测试)

### 步骤

```bash
# 1. 克隆代码
git clone https://github.com/yourusername/NiceWebCode.git
cd NiceWebCode

# 2. 还原依赖
dotnet restore

# 3. 构建项目
dotnet build

# 4. 运行WebApi
cd src/NiceWebCode.WebApi
dotnet run

# 5. 访问Swagger
# 浏览器打开: https://localhost:5001/swagger
```

### 配置

编辑 `appsettings.json`:

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

---

## 2. Docker部署（推荐）

### 环境要求

- Docker 20.10+
- Docker Compose 1.29+

### 快速启动

```bash
# 1. 克隆代码
git clone https://github.com/yourusername/NiceWebCode.git
cd NiceWebCode

# 2. 使用Docker Compose启动
docker-compose up -d

# 3. 查看日志
docker-compose logs -f webapi

# 4. 访问服务
# HTTP: http://localhost:5000
# HTTPS: https://localhost:5001
# Swagger: http://localhost:5000/swagger
```

### 单独构建Docker镜像

```bash
# 构建镜像
docker build -t nicewebcode:latest -f src/NiceWebCode.WebApi/Dockerfile .

# 运行容器
docker run -d \
  -p 5000:8080 \
  -p 5001:8081 \
  -v nicewebcode-data:/data \
  --name nicewebcode \
  nicewebcode:latest
```

### 持久化数据

Docker Compose已配置数据卷：
- `nicewebcode-data`: 存储数据库和工作区文件

查看数据：
```bash
docker volume inspect nicewebcode-data
```

### 环境变量配置

在`docker-compose.yml`中修改：

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Data Source=/data/nicewebcode.db
  - WorkspaceBasePath=/data/workspaces
  - CliTools__ClaudeCode__Path=/usr/local/bin/claude-code
```

---

## 3. Windows Server部署

### 环境要求

- Windows Server 2019+
- .NET 9.0 Runtime (ASP.NET Core)
- IIS 10+ (可选)

### 方式A: 自托管服务

```bash
# 1. 发布项目
dotnet publish src/NiceWebCode.WebApi/NiceWebCode.WebApi.csproj `
  -c Release `
  -o C:\inetpub\nicewebcode

# 2. 安装Windows服务
sc create NiceWebCode `
  binPath="C:\inetpub\nicewebcode\NiceWebCode.WebApi.exe" `
  start=auto

# 3. 启动服务
sc start NiceWebCode
```

### 方式B: IIS部署

1. **安装ASP.NET Core托管捆绑包**
   - 下载: https://dotnet.microsoft.com/download/dotnet/9.0
   - 安装后重启IIS

2. **发布项目**
   ```bash
   dotnet publish -c Release -o C:\inetpub\nicewebcode
   ```

3. **配置IIS**
   - 创建应用程序池（无托管代码）
   - 创建网站，指向发布目录
   - 配置HTTPS证书
   - 设置应用程序池标识权限

4. **配置web.config**（自动生成）
   ```xml
   <aspNetCore processPath="dotnet"
               arguments=".\NiceWebCode.WebApi.dll"
               stdoutLogEnabled="true"
               stdoutLogFile=".\logs\stdout" />
   ```

---

## 4. Linux服务器部署

### 环境要求

- Ubuntu 20.04+ / CentOS 8+ / Debian 11+
- .NET 9.0 Runtime
- Nginx (反向代理)
- Systemd (服务管理)

### 步骤

#### 4.1 安装.NET Runtime

```bash
# Ubuntu
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0

# CentOS
sudo rpm -Uvh https://packages.microsoft.com/config/centos/8/packages-microsoft-prod.rpm
sudo dnf install aspnetcore-runtime-9.0
```

#### 4.2 部署应用

```bash
# 1. 创建部署目录
sudo mkdir -p /var/www/nicewebcode
sudo chown -R $USER:$USER /var/www/nicewebcode

# 2. 发布并上传
dotnet publish -c Release -o ./publish
scp -r ./publish/* user@server:/var/www/nicewebcode/

# 3. 设置权限
sudo chmod +x /var/www/nicewebcode/NiceWebCode.WebApi
```

#### 4.3 配置Systemd服务

创建 `/etc/systemd/system/nicewebcode.service`:

```ini
[Unit]
Description=NiceWebCode Web API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/nicewebcode
ExecStart=/usr/bin/dotnet /var/www/nicewebcode/NiceWebCode.WebApi.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nicewebcode
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

启动服务：

```bash
sudo systemctl daemon-reload
sudo systemctl enable nicewebcode
sudo systemctl start nicewebcode
sudo systemctl status nicewebcode
```

#### 4.4 配置Nginx反向代理

创建 `/etc/nginx/sites-available/nicewebcode`:

```nginx
upstream nicewebcode {
    server localhost:5000;
}

server {
    listen 80;
    server_name your-domain.com;

    # 重定向到HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;

    # SSL配置
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://nicewebcode;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR WebSocket支持
    location /hubs {
        proxy_pass http://nicewebcode;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 86400;
    }
}
```

启用站点：

```bash
sudo ln -s /etc/nginx/sites-available/nicewebcode /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 5. 生产环境配置

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "NiceWebCode": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/data/nicewebcode.db"
  },
  "WorkspaceBasePath": "/data/workspaces",
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend.com"
    ]
  }
}
```

### 数据库迁移

```bash
# 首次部署时创建数据库
dotnet ef database update --project src/NiceWebCode.Infrastructure

# 或在启动时自动创建（已配置）
# Program.cs: dbContext.Database.EnsureCreated();
```

### 定期清理工作区

添加到crontab（Linux）：

```bash
# 每天凌晨2点清理7天前的工作区
0 2 * * * find /data/workspaces -type d -mtime +7 -exec rm -rf {} +
```

---

## 6. 监控与日志

### 应用日志

```bash
# Docker
docker-compose logs -f webapi

# Systemd (Linux)
sudo journalctl -u nicewebcode -f

# 文件日志 (配置)
{
  "Logging": {
    "File": {
      "Path": "/var/log/nicewebcode.log",
      "MinLevel": "Warning"
    }
  }
}
```

### 健康检查

访问健康检查端点：
```bash
curl http://localhost:5000/health
```

### 性能监控

推荐工具：
- **Application Insights** (Azure)
- **Prometheus + Grafana**
- **ELK Stack** (Elasticsearch + Logstash + Kibana)

---

## 7. 安全建议

### 生产环境检查清单

- [ ] 使用HTTPS（配置SSL证书）
- [ ] 配置防火墙规则
- [ ] 限制CORS允许的源
- [ ] 使用强密码/密钥
- [ ] 定期更新.NET运行时
- [ ] 启用日志审计
- [ ] 配置速率限制
- [ ] 隐藏错误详情（不返回堆栈跟踪）
- [ ] 定期备份数据库
- [ ] 监控磁盘空间使用

### HTTPS证书

#### Let's Encrypt (免费)

```bash
sudo apt-get install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com
```

#### 自签名证书（开发环境）

```bash
dotnet dev-certs https --trust
```

---

## 8. 故障排查

### 常见问题

**1. 数据库连接失败**
```bash
# 检查数据库文件权限
ls -la /data/nicewebcode.db
sudo chown www-data:www-data /data/nicewebcode.db
```

**2. SignalR连接失败**
```bash
# 检查WebSocket是否启用
# Nginx: proxy_set_header Upgrade $http_upgrade;
# IIS: 安装WebSocket功能
```

**3. 工作区权限不足**
```bash
# 设置工作区目录权限
sudo chown -R www-data:www-data /data/workspaces
sudo chmod -R 755 /data/workspaces
```

**4. 端口被占用**
```bash
# 查看端口占用
netstat -tlnp | grep 5000
# 修改端口配置
export ASPNETCORE_URLS="http://+:5100"
```

---

## 9. 升级部署

### Docker升级

```bash
# 1. 拉取最新代码
git pull origin main

# 2. 重新构建
docker-compose build

# 3. 重启服务
docker-compose down
docker-compose up -d
```

### 无停机升级

```bash
# 1. 构建新版本
docker build -t nicewebcode:v2.0 .

# 2. 滚动更新
docker service update --image nicewebcode:v2.0 nicewebcode
```

---

## 10. 备份与恢复

### 备份

```bash
# 备份数据库
cp /data/nicewebcode.db /backup/nicewebcode-$(date +%Y%m%d).db

# 备份工作区
tar -czf /backup/workspaces-$(date +%Y%m%d).tar.gz /data/workspaces
```

### 恢复

```bash
# 停止服务
sudo systemctl stop nicewebcode

# 恢复数据库
cp /backup/nicewebcode-20250105.db /data/nicewebcode.db

# 恢复工作区
tar -xzf /backup/workspaces-20250105.tar.gz -C /data

# 启动服务
sudo systemctl start nicewebcode
```

---

需要帮助？请提交Issue: https://github.com/yourusername/NiceWebCode/issues
