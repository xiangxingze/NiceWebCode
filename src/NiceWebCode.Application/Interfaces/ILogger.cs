namespace NiceWebCode.Application.Interfaces;

/// <summary>
/// 简单的日志接口
/// </summary>
public interface ILogger
{
    void LogInformation(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
}
