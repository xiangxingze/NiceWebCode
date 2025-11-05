using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NiceWebCode.Application.Interfaces;
using NiceWebCode.Application.Models;
using NiceWebCode.Domain.Entities;
using NiceWebCode.Infrastructure.Data;
using NiceWebCode.WebApi.Hubs;
using TaskStatus = NiceWebCode.Domain.Entities.TaskStatus;

namespace NiceWebCode.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWorkspaceService _workspaceService;
    private readonly ICliToolExecutor _cliExecutor;
    private readonly IHubContext<OutputHub> _hubContext;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        ApplicationDbContext dbContext,
        IWorkspaceService workspaceService,
        ICliToolExecutor cliExecutor,
        IHubContext<OutputHub> hubContext,
        ILogger<SessionsController> logger)
    {
        _dbContext = dbContext;
        _workspaceService = workspaceService;
        _cliExecutor = cliExecutor;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有会话列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SessionDto>>> GetSessions([FromQuery] string? userId = null)
    {
        var query = _dbContext.Sessions.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(s => s.UserId == userId);
        }

        var sessions = await query
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                Title = s.Title,
                UserId = s.UserId,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                Status = s.Status.ToString(),
                IsPublic = s.IsPublic,
                ShareToken = s.ShareToken
            })
            .ToListAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// 获取单个会话详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SessionDto>> GetSession(Guid id)
    {
        var session = await _dbContext.Sessions.FindAsync(id);

        if (session == null)
            return NotFound();

        return Ok(new SessionDto
        {
            Id = session.Id,
            Title = session.Title,
            UserId = session.UserId,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Status = session.Status.ToString(),
            IsPublic = session.IsPublic,
            ShareToken = session.ShareToken
        });
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        var session = new Session
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = SessionStatus.Active
        };

        // 创建工作区
        var workspacePath = await _workspaceService.CreateWorkspaceAsync(session.Id);
        session.WorkspacePath = workspacePath;

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("创建新会话: {SessionId}, 用户: {UserId}", session.Id, session.UserId);

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, new SessionDto
        {
            Id = session.Id,
            Title = session.Title,
            UserId = session.UserId,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Status = session.Status.ToString()
        });
    }

    /// <summary>
    /// 执行AI任务
    /// </summary>
    [HttpPost("{id}/execute")]
    public async Task<ActionResult<Guid>> ExecuteTask(Guid id, [FromBody] ExecuteTaskRequest request)
    {
        var session = await _dbContext.Sessions.FindAsync(id);
        if (session == null)
            return NotFound();

        var task = new AiTask
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Prompt = request.Prompt,
            CliToolName = request.CliToolName,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AiTasks.Add(task);
        await _dbContext.SaveChangesAsync();

        // 在后台执行任务
        _ = Task.Run(async () => await ExecuteTaskInBackground(task, session));

        return Ok(new { taskId = task.Id });
    }

    /// <summary>
    /// 获取会话的输出历史
    /// </summary>
    [HttpGet("{id}/outputs")]
    public async Task<ActionResult<List<OutputChunkDto>>> GetSessionOutputs(Guid id)
    {
        var outputs = await _dbContext.OutputChunks
            .Where(o => o.SessionId == id)
            .OrderBy(o => o.SequenceNumber)
            .Select(o => new OutputChunkDto
            {
                Id = o.Id,
                SessionId = o.SessionId,
                TaskId = o.TaskId,
                Type = o.Type.ToString(),
                Content = o.Content,
                CreatedAt = o.CreatedAt,
                SequenceNumber = o.SequenceNumber
            })
            .ToListAsync();

        return Ok(outputs);
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSession(Guid id)
    {
        var session = await _dbContext.Sessions.FindAsync(id);
        if (session == null)
            return NotFound();

        // 删除工作区
        await _workspaceService.DeleteWorkspaceAsync(id);

        _dbContext.Sessions.Remove(session);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("删除会话: {SessionId}", id);

        return NoContent();
    }

    /// <summary>
    /// 后台执行任务
    /// </summary>
    private async Task ExecuteTaskInBackground(AiTask task, Session session)
    {
        try
        {
            // 更新任务状态为执行中
            task.Status = TaskStatus.Running;
            task.StartedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // 通知前端任务开始
            await _hubContext.Clients.Group(session.Id.ToString())
                .SendAsync("TaskStatusChanged", task.Id, "Running");

            _logger.LogInformation("开始执行任务: {TaskId}, 会话: {SessionId}", task.Id, session.Id);

            var workspacePath = session.WorkspacePath ?? _workspaceService.GetWorkspacePath(session.Id);

            // 执行CLI命令并流式输出
            await foreach (var chunk in _cliExecutor.ExecuteAsync(
                session.Id,
                task.Prompt,
                workspacePath))
            {
                // 保存到数据库
                var outputChunk = new OutputChunk
                {
                    Id = chunk.Id,
                    SessionId = chunk.SessionId,
                    TaskId = task.Id,
                    Type = Enum.Parse<OutputChunkType>(chunk.Type),
                    Content = chunk.Content,
                    CreatedAt = chunk.CreatedAt,
                    SequenceNumber = chunk.SequenceNumber
                };

                _dbContext.OutputChunks.Add(outputChunk);
                await _dbContext.SaveChangesAsync();

                // 实时推送到前端
                await _hubContext.Clients.Group(session.Id.ToString())
                    .SendAsync("ReceiveOutput", chunk);
            }

            // 任务完成
            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.Group(session.Id.ToString())
                .SendAsync("TaskCompleted", task.Id, true, null);

            _logger.LogInformation("任务执行完成: {TaskId}", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务执行失败: {TaskId}", task.Id);

            task.Status = TaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.Group(session.Id.ToString())
                .SendAsync("TaskCompleted", task.Id, false, ex.Message);
        }
    }
}
