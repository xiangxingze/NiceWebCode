using System.Text.Json;
using NiceWebCode.Application.Interfaces;
using NiceWebCode.Application.Models;
using ILogger = NiceWebCode.Application.Interfaces.ILogger;

namespace NiceWebCode.Infrastructure.CliAdapters;

/// <summary>
/// Claude Code CLI适配器
/// </summary>
public class ClaudeCodeAdapter : BaseCliAdapter
{
    private readonly string _executablePath;

    public ClaudeCodeAdapter(ILogger logger, string? executablePath = null)
        : base(logger)
    {
        _executablePath = executablePath ?? "claude-code";
    }

    public override string ToolName => "claude-code";

    protected override string GetExecutablePath() => _executablePath;

    protected override string BuildArguments(string prompt, string workspacePath)
    {
        // Claude Code命令格式: claude-code --prompt "your prompt"
        return $"--prompt \"{EscapeArgument(prompt)}\"";
    }

    protected override OutputChunkDto? ParseOutput(string line, Guid sessionId)
    {
        try
        {
            // Claude Code输出JSONL格式
            // 示例: {"type":"text","content":"Hello"}
            if (line.StartsWith("{"))
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                if (root.TryGetProperty("type", out var typeElement))
                {
                    var type = typeElement.GetString() ?? "Text";
                    var content = root.TryGetProperty("content", out var contentElement)
                        ? contentElement.GetString() ?? ""
                        : line;

                    return new OutputChunkDto
                    {
                        Id = Guid.NewGuid(),
                        SessionId = sessionId,
                        Type = MapChunkType(type),
                        Content = content,
                        CreatedAt = DateTime.UtcNow,
                        SequenceNumber = 0, // 将由BaseCliAdapter设置
                        Metadata = ExtractMetadata(root)
                    };
                }
            }

            // 如果不是JSON格式，作为纯文本处理
            return new OutputChunkDto
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Type = "Text",
                Content = line,
                CreatedAt = DateTime.UtcNow,
                SequenceNumber = 0
            };
        }
        catch (JsonException)
        {
            // JSON解析失败，作为纯文本处理
            return new OutputChunkDto
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Type = "Text",
                Content = line,
                CreatedAt = DateTime.UtcNow,
                SequenceNumber = 0
            };
        }
    }

    /// <summary>
    /// 映射块类型
    /// </summary>
    private string MapChunkType(string type) => type switch
    {
        "text" => "Text",
        "code" => "Code",
        "tool_use" => "ToolUse",
        "tool_result" => "ToolResult",
        "error" => "Error",
        "thinking" => "System",
        _ => "Text"
    };

    /// <summary>
    /// 提取元数据
    /// </summary>
    private Dictionary<string, object>? ExtractMetadata(JsonElement root)
    {
        var metadata = new Dictionary<string, object>();

        foreach (var property in root.EnumerateObject())
        {
            if (property.Name != "type" && property.Name != "content")
            {
                metadata[property.Name] = property.Value.GetRawText();
            }
        }

        return metadata.Count > 0 ? metadata : null;
    }

    /// <summary>
    /// 转义命令行参数
    /// </summary>
    private string EscapeArgument(string arg)
    {
        return arg.Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
