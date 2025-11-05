namespace NiceWebCode.Domain.Entities;

/// <summary>
/// 输出块实体 - 代表CLI工具输出的一个片段
/// </summary>
public class OutputChunk
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? TaskId { get; set; }
    public OutputChunkType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SequenceNumber { get; set; }
    public string? Metadata { get; set; }  // JSON格式存储额外信息

    // 导航属性
    public Session Session { get; set; } = null!;
}

/// <summary>
/// 输出块类型枚举
/// </summary>
public enum OutputChunkType
{
    Text,           // 纯文本
    Code,           // 代码块
    Markdown,       // Markdown文档
    ToolUse,        // 工具使用
    ToolResult,     // 工具结果
    Error,          // 错误信息
    System,         // 系统消息
    JsonlEvent      // JSONL事件
}
