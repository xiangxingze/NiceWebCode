#!/bin/bash

# NiceWebCode 快速启动脚本

echo "========================================="
echo "  NiceWebCode - AI编程伙伴平台"
echo "========================================="
echo ""

# 检查.NET SDK
echo "[1/4] 检查.NET环境..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ 未检测到.NET SDK，请先安装.NET 9.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET版本: $DOTNET_VERSION"
echo ""

# 构建项目
echo "[2/4] 构建项目..."
if ! dotnet build --nologo; then
    echo "❌ 构建失败"
    exit 1
fi
echo "✅ 构建成功"
echo ""

# 创建数据目录
echo "[3/4] 初始化数据目录..."
mkdir -p workspaces
echo "✅ 工作区目录已创建: ./workspaces"
echo ""

# 启动应用
echo "[4/4] 启动WebApi..."
echo ""
echo "========================================="
echo "  应用正在启动..."
echo "========================================="
echo ""
echo "  Swagger文档: https://localhost:5001/swagger"
echo "  API地址:     https://localhost:5001/api"
echo "  SignalR Hub: wss://localhost:5001/hubs/output"
echo ""
echo "  按 Ctrl+C 停止服务"
echo ""

cd src/NiceWebCode.WebApi
dotnet run
