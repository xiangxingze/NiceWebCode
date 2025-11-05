namespace NiceWebCode.Domain.Entities;

/// <summary>
/// 工作区文件实体 - 代表会话工作区中的文件
/// </summary>
public class WorkspaceFile
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDirectory { get; set; }

    // 导航属性
    public Session Session { get; set; } = null!;
}
