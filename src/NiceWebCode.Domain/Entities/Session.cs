namespace NiceWebCode.Domain.Entities;

/// <summary>
/// 会话实体 - 代表用户与AI的一次对话会话
/// </summary>
public class Session
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SessionStatus Status { get; set; }
    public string? WorkspacePath { get; set; }
    public bool IsPublic { get; set; }
    public string? ShareToken { get; set; }

    // 导航属性
    public ICollection<AiTask> Tasks { get; set; } = new List<AiTask>();
    public ICollection<OutputChunk> OutputChunks { get; set; } = new List<OutputChunk>();
}

/// <summary>
/// 会话状态枚举
/// </summary>
public enum SessionStatus
{
    Active,      // 活跃中
    Idle,        // 空闲
    Archived     // 已归档
}
