using NiceWebCode.Application.Interfaces;
using NiceWebCode.Application.Models;
using ILogger = NiceWebCode.Application.Interfaces.ILogger;

namespace NiceWebCode.Infrastructure.Services;

/// <summary>
/// 工作区服务实现 - 管理会话独立工作区
/// </summary>
public class WorkspaceService : IWorkspaceService
{
    private readonly string _baseWorkspacePath;
    private readonly ILogger _logger;

    public WorkspaceService(string baseWorkspacePath, ILogger logger)
    {
        _baseWorkspacePath = baseWorkspacePath;
        _logger = logger;

        // 确保基础工作区目录存在
        if (!Directory.Exists(_baseWorkspacePath))
        {
            Directory.CreateDirectory(_baseWorkspacePath);
        }
    }

    public Task<string> CreateWorkspaceAsync(Guid sessionId)
    {
        var workspacePath = GetWorkspacePath(sessionId);

        if (!Directory.Exists(workspacePath))
        {
            Directory.CreateDirectory(workspacePath);
            _logger.LogInformation("创建工作区: {WorkspacePath}", workspacePath);
        }

        return Task.FromResult(workspacePath);
    }

    public string GetWorkspacePath(Guid sessionId)
    {
        return Path.Combine(_baseWorkspacePath, $"session_{sessionId}");
    }

    public async Task<List<WorkspaceFileDto>> ListFilesAsync(Guid sessionId)
    {
        var workspacePath = GetWorkspacePath(sessionId);

        if (!Directory.Exists(workspacePath))
        {
            return new List<WorkspaceFileDto>();
        }

        return await Task.Run(() => BuildFileTree(workspacePath, workspacePath));
    }

    public async Task<string> ReadFileAsync(Guid sessionId, string relativePath)
    {
        var workspacePath = GetWorkspacePath(sessionId);
        var fullPath = Path.Combine(workspacePath, relativePath);

        // 安全检查：防止路径遍历攻击
        if (!IsPathWithinWorkspace(fullPath, workspacePath))
        {
            throw new UnauthorizedAccessException("拒绝访问工作区外的文件");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("文件不存在", relativePath);
        }

        return await File.ReadAllTextAsync(fullPath);
    }

    public async Task DeleteWorkspaceAsync(Guid sessionId)
    {
        var workspacePath = GetWorkspacePath(sessionId);

        if (Directory.Exists(workspacePath))
        {
            await Task.Run(() =>
            {
                Directory.Delete(workspacePath, recursive: true);
                _logger.LogInformation("删除工作区: {WorkspacePath}", workspacePath);
            });
        }
    }

    public async Task CleanupExpiredWorkspacesAsync(TimeSpan expirationTime)
    {
        var directories = Directory.GetDirectories(_baseWorkspacePath, "session_*");

        var tasks = directories
            .Where(dir => IsExpired(dir, expirationTime))
            .Select(async dir =>
            {
                try
                {
                    await Task.Run(() => Directory.Delete(dir, recursive: true));
                    _logger.LogInformation("清理过期工作区: {Dir}", dir);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理工作区失败: {Dir}", dir);
                }
            });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 构建文件树
    /// </summary>
    private List<WorkspaceFileDto> BuildFileTree(string currentPath, string basePath)
    {
        var result = new List<WorkspaceFileDto>();

        try
        {
            // 添加文件
            foreach (var file in Directory.GetFiles(currentPath))
            {
                var fileInfo = new FileInfo(file);
                result.Add(new WorkspaceFileDto
                {
                    RelativePath = GetRelativePath(file, basePath),
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    ContentType = GetContentType(fileInfo.Extension),
                    UpdatedAt = fileInfo.LastWriteTimeUtc,
                    IsDirectory = false
                });
            }

            // 递归添加子目录
            foreach (var directory in Directory.GetDirectories(currentPath))
            {
                var dirInfo = new DirectoryInfo(directory);
                var dirDto = new WorkspaceFileDto
                {
                    RelativePath = GetRelativePath(directory, basePath),
                    FileName = dirInfo.Name,
                    FileSize = 0,
                    ContentType = "directory",
                    UpdatedAt = dirInfo.LastWriteTimeUtc,
                    IsDirectory = true,
                    Children = BuildFileTree(directory, basePath)
                };
                result.Add(dirDto);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "访问目录失败: {Path}", currentPath);
        }

        return result;
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    private string GetRelativePath(string fullPath, string basePath)
    {
        return Path.GetRelativePath(basePath, fullPath);
    }

    /// <summary>
    /// 根据扩展名获取内容类型
    /// </summary>
    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".md" => "text/markdown",
            ".cs" => "text/x-csharp",
            ".py" => "text/x-python",
            ".java" => "text/x-java",
            ".cpp" or ".c" or ".h" => "text/x-c",
            ".rs" => "text/x-rust",
            ".go" => "text/x-go",
            ".ts" => "text/typescript",
            ".tsx" => "text/tsx",
            ".jsx" => "text/jsx",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// 检查路径是否在工作区内（防止路径遍历攻击）
    /// </summary>
    private bool IsPathWithinWorkspace(string fullPath, string workspacePath)
    {
        var fullPathNormalized = Path.GetFullPath(fullPath);
        var workspacePathNormalized = Path.GetFullPath(workspacePath);
        return fullPathNormalized.StartsWith(workspacePathNormalized, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查目录是否过期
    /// </summary>
    private bool IsExpired(string directoryPath, TimeSpan expirationTime)
    {
        try
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            return DateTime.UtcNow - dirInfo.LastAccessTimeUtc > expirationTime;
        }
        catch
        {
            return false;
        }
    }
}
