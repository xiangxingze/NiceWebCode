namespace NiceWebCode.Application.Models;

/// <summary>
/// 工作区文件数据传输对象
/// </summary>
public class WorkspaceFileDto
{
    public string RelativePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public bool IsDirectory { get; set; }
    public List<WorkspaceFileDto>? Children { get; set; }
}
