using NiceWebCode.Application.Models;

namespace NiceWebCode.Application.Interfaces;

/// <summary>
/// CLI工具执行器接口 - 所有CLI适配器的基础接口
/// </summary>
public interface ICliToolExecutor
{
    /// <summary>
    /// 工具名称（如：claude-code, copilot等）
    /// </summary>
    string ToolName { get; }

    /// <summary>
    /// 执行CLI命令并流式返回输出
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="prompt">用户输入的提示词</param>
    /// <param name="workspacePath">工作区路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>输出块的异步流</returns>
    IAsyncEnumerable<OutputChunkDto> ExecuteAsync(
        Guid sessionId,
        string prompt,
        string workspacePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查CLI工具是否可用
    /// </summary>
    Task<bool> IsAvailableAsync();
}
