namespace NiceWebCode.Application.Models;

/// <summary>
/// 会话数据传输对象
/// </summary>
public class SessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string? ShareToken { get; set; }
}

/// <summary>
/// 创建会话请求
/// </summary>
public class CreateSessionRequest
{
    public string Title { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// 执行任务请求
/// </summary>
public class ExecuteTaskRequest
{
    public Guid SessionId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string CliToolName { get; set; } = "claude-code";
}
