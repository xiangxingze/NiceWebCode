using NiceWebCode.Application.Models;

namespace NiceWebCode.Application.Interfaces;

/// <summary>
/// 工作区服务接口 - 管理会话独立工作区
/// </summary>
public interface IWorkspaceService
{
    /// <summary>
    /// 为会话创建独立工作区
    /// </summary>
    Task<string> CreateWorkspaceAsync(Guid sessionId);

    /// <summary>
    /// 获取工作区路径
    /// </summary>
    string GetWorkspacePath(Guid sessionId);

    /// <summary>
    /// 列出工作区文件树
    /// </summary>
    Task<List<WorkspaceFileDto>> ListFilesAsync(Guid sessionId);

    /// <summary>
    /// 读取文件内容
    /// </summary>
    Task<string> ReadFileAsync(Guid sessionId, string relativePath);

    /// <summary>
    /// 删除工作区
    /// </summary>
    Task DeleteWorkspaceAsync(Guid sessionId);

    /// <summary>
    /// 清理过期工作区
    /// </summary>
    Task CleanupExpiredWorkspacesAsync(TimeSpan expirationTime);
}
