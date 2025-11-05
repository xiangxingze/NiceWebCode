using Microsoft.AspNetCore.SignalR;
using NiceWebCode.Application.Models;

namespace NiceWebCode.WebApi.Hubs;

/// <summary>
/// 输出Hub - 用于实时推送CLI输出到前端
/// </summary>
public class OutputHub : Hub
{
    private readonly ILogger<OutputHub> _logger;

    public OutputHub(ILogger<OutputHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 客户端加入会话组
    /// </summary>
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        _logger.LogInformation("客户端 {ConnectionId} 加入会话组 {SessionId}",
            Context.ConnectionId, sessionId);
    }

    /// <summary>
    /// 客户端离开会话组
    /// </summary>
    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        _logger.LogInformation("客户端 {ConnectionId} 离开会话组 {SessionId}",
            Context.ConnectionId, sessionId);
    }

    /// <summary>
    /// 连接建立时调用
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("客户端已连接: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 连接断开时调用
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "客户端断开连接时发生错误: {ConnectionId}",
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("客户端已断开连接: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// 客户端可调用的方法（由Hub推送）
/// </summary>
public interface IOutputHubClient
{
    /// <summary>
    /// 接收输出块
    /// </summary>
    Task ReceiveOutput(OutputChunkDto chunk);

    /// <summary>
    /// 任务状态变更
    /// </summary>
    Task TaskStatusChanged(Guid taskId, string status);

    /// <summary>
    /// 任务完成通知
    /// </summary>
    Task TaskCompleted(Guid taskId, bool success, string? errorMessage);
}
