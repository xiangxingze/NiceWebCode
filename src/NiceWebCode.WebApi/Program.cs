using Microsoft.EntityFrameworkCore;
using NiceWebCode.Application.Interfaces;
using NiceWebCode.Infrastructure.CliAdapters;
using NiceWebCode.Infrastructure.Data;
using NiceWebCode.Infrastructure.Services;
using NiceWebCode.WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置数据库
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=nicewebcode.db";
    options.UseSqlite(connectionString);
});

// 配置SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// 配置CORS（允许前端跨域访问）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 注册应用服务
var workspaceBasePath = builder.Configuration["WorkspaceBasePath"]
    ?? Path.Combine(Directory.GetCurrentDirectory(), "workspaces");

builder.Services.AddSingleton<IWorkspaceService>(sp =>
    new WorkspaceService(
        workspaceBasePath,
        new ConsoleLogger()));

// 注册CLI适配器
builder.Services.AddSingleton<ICliToolExecutor>(sp =>
    new ClaudeCodeAdapter(
        new ConsoleLogger(),
        builder.Configuration["CliTools:ClaudeCode:Path"]));

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 启用CORS
app.UseCors("AllowAll");

app.UseAuthorization();

// 映射控制器
app.MapControllers();

// 映射SignalR Hub
app.MapHub<OutputHub>("/hubs/output");

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();

/// <summary>
/// 简单的控制台日志实现
/// </summary>
public class ConsoleLogger : NiceWebCode.Application.Interfaces.ILogger
{
    public void LogInformation(string message, params object[] args)
    {
        Console.WriteLine($"[INFO] {string.Format(message, args)}");
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        Console.WriteLine($"[ERROR] {string.Format(message, args)}");
        Console.WriteLine($"Exception: {ex.Message}");
    }
}
