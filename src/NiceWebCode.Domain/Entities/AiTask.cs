namespace NiceWebCode.Domain.Entities;

/// <summary>
/// AI任务实体 - 代表一次具体的AI编程任务
/// </summary>
public class AiTask
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string CliToolName { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExitCode { get; set; }

    // 导航属性
    public Session Session { get; set; } = null!;
}

/// <summary>
/// 任务状态枚举
/// </summary>
public enum TaskStatus
{
    Pending,     // 待执行
    Running,     // 执行中
    Completed,   // 已完成
    Failed,      // 失败
    Cancelled    // 已取消
}
