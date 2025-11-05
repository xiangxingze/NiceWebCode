@echo off
echo =========================================
echo   NiceWebCode - 测试启动
echo =========================================
echo.

cd src\NiceWebCode.WebApi

echo 正在启动应用...
echo.
echo 访问地址:
echo   - Swagger: http://localhost:5000/swagger
echo   - API:     http://localhost:5000/api
echo.
echo 按 Ctrl+C 停止服务
echo.

dotnet run --no-build
