@echo off
REM NiceWebCode 快速启动脚本 (Windows)

echo =========================================
echo   NiceWebCode - AI编程伙伴平台
echo =========================================
echo.

REM 检查.NET SDK
echo [1/4] 检查.NET环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 未检测到.NET SDK，请先安装.NET 9.0
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET版本: %DOTNET_VERSION%
echo.

REM 构建项目
echo [2/4] 构建项目...
dotnet build --nologo
if errorlevel 1 (
    echo ❌ 构建失败
    pause
    exit /b 1
)
echo ✅ 构建成功
echo.

REM 创建数据目录
echo [3/4] 初始化数据目录...
if not exist "workspaces" mkdir workspaces
echo ✅ 工作区目录已创建: .\workspaces
echo.

REM 启动应用
echo [4/4] 启动WebApi...
echo.
echo =========================================
echo   应用正在启动...
echo =========================================
echo.
echo   Swagger文档: https://localhost:5001/swagger
echo   API地址:     https://localhost:5001/api
echo   SignalR Hub: wss://localhost:5001/hubs/output
echo.
echo   按 Ctrl+C 停止服务
echo.

cd src\NiceWebCode.WebApi
dotnet run
