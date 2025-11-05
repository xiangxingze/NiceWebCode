using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using NiceWebCode.Application.Interfaces;
using NiceWebCode.Application.Models;
using ILogger = NiceWebCode.Application.Interfaces.ILogger;

namespace NiceWebCode.Infrastructure.CliAdapters;

/// <summary>
/// CLI适配器基类 - 提供通用的进程管理和输出处理
/// </summary>
public abstract class BaseCliAdapter : ICliToolExecutor
{
    protected readonly ILogger Logger;
    private int _sequenceNumber = 0;

    protected BaseCliAdapter(ILogger logger)
    {
        Logger = logger;
    }

    public abstract string ToolName { get; }

    /// <summary>
    /// 获取CLI可执行文件路径
    /// </summary>
    protected abstract string GetExecutablePath();

    /// <summary>
    /// 构建命令行参数
    /// </summary>
    protected abstract string BuildArguments(string prompt, string workspacePath);

    /// <summary>
    /// 解析CLI输出为OutputChunkDto
    /// </summary>
    protected abstract OutputChunkDto? ParseOutput(string line, Guid sessionId);

    public async IAsyncEnumerable<OutputChunkDto> ExecuteAsync(
        Guid sessionId,
        string prompt,
        string workspacePath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = GetExecutablePath(),
            Arguments = BuildArguments(prompt, workspacePath),
            WorkingDirectory = workspacePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        Process? process = null;

        try
        {
            process = new Process { StartInfo = processInfo };
            Logger.LogInformation("启动CLI工具: {ToolName}, 会话: {SessionId}", ToolName, sessionId);
            process.Start();

            // 异步读取标准输出
            await foreach (var chunk in ReadOutputStreamAsync(
                process.StandardOutput,
                sessionId,
                process,
                cancellationToken).ConfigureAwait(false))
            {
                yield return chunk;
            }
        }
        finally
        {
            if (process != null && !process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            process?.Dispose();
        }
    }

    /// <summary>
    /// 异步读取输出流
    /// </summary>
    private async IAsyncEnumerable<OutputChunkDto> ReadOutputStreamAsync(
        StreamReader reader,
        Guid sessionId,
        Process process,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = new char[4096];
        var lineBuilder = new StringBuilder();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var readCount = await reader.ReadAsync(buffer, 0, buffer.Length);
            if (readCount == 0) break;

            for (int i = 0; i < readCount; i++)
            {
                var ch = buffer[i];
                if (ch == '\n')
                {
                    var line = lineBuilder.ToString();
                    lineBuilder.Clear();

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var chunk = ParseOutput(line, sessionId);
                        if (chunk != null)
                        {
                            yield return chunk;
                        }
                    }
                }
                else if (ch != '\r')
                {
                    lineBuilder.Append(ch);
                }
            }
        }

        // 处理最后一行（如果没有换行符）
        if (lineBuilder.Length > 0)
        {
            var line = lineBuilder.ToString();
            var chunk = ParseOutput(line, sessionId);
            if (chunk != null)
            {
                yield return chunk;
            }
        }

        // 等待进程退出
        await process.WaitForExitAsync(cancellationToken);

        Logger.LogInformation("CLI工具执行完成: {ToolName}, 退出码: {ExitCode}",
            ToolName, process.ExitCode);

        // 如果有错误输出，读取并返回
        if (process.ExitCode != 0)
        {
            var errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            if (!string.IsNullOrEmpty(errorOutput))
            {
                yield return new OutputChunkDto
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    Type = "Error",
                    Content = errorOutput,
                    CreatedAt = DateTime.UtcNow,
                    SequenceNumber = Interlocked.Increment(ref _sequenceNumber)
                };
            }
        }
    }

    public virtual async Task<bool> IsAvailableAsync()
    {
        try
        {
            var executablePath = GetExecutablePath();
            if (string.IsNullOrEmpty(executablePath))
                return false;

            // 检查文件是否存在
            if (!File.Exists(executablePath) && !IsInPath(executablePath))
                return false;

            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查命令是否在系统PATH中
    /// </summary>
    private bool IsInPath(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return false;

        var paths = pathEnv.Split(Path.PathSeparator);
        return paths.Any(p => File.Exists(Path.Combine(p, command)) ||
                              File.Exists(Path.Combine(p, command + ".exe")));
    }
}

