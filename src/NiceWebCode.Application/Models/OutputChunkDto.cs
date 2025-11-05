namespace NiceWebCode.Application.Models;

/// <summary>
/// 输出块数据传输对象
/// </summary>
public class OutputChunkDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? TaskId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SequenceNumber { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
