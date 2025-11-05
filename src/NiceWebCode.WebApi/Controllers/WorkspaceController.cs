using Microsoft.AspNetCore.Mvc;
using NiceWebCode.Application.Interfaces;
using NiceWebCode.Application.Models;

namespace NiceWebCode.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<WorkspaceController> _logger;

    public WorkspaceController(
        IWorkspaceService workspaceService,
        ILogger<WorkspaceController> logger)
    {
        _workspaceService = workspaceService;
        _logger = logger;
    }

    /// <summary>
    /// 获取会话工作区文件列表
    /// </summary>
    [HttpGet("{sessionId}/files")]
    public async Task<ActionResult<List<WorkspaceFileDto>>> GetFiles(Guid sessionId)
    {
        try
        {
            var files = await _workspaceService.ListFilesAsync(sessionId);
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取工作区文件列表失败: {SessionId}", sessionId);
            return StatusCode(500, new { error = "获取文件列表失败" });
        }
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    [HttpGet("{sessionId}/files/content")]
    public async Task<ActionResult<string>> GetFileContent(Guid sessionId, [FromQuery] string path)
    {
        try
        {
            var content = await _workspaceService.ReadFileAsync(sessionId, path);
            return Ok(new { content });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "文件不存在" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取文件失败: {SessionId}, {Path}", sessionId, path);
            return StatusCode(500, new { error = "读取文件失败" });
        }
    }
}
