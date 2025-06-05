// src/common/Shared/Logging/ILoggerAdapter.cs
namespace FoodDeliverySystem.Common.Logging;

public interface ILoggerAdapter<T>
{
    void LogInformation(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogCritical(Exception ex, string message, params object[] args);
}