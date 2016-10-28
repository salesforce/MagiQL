using System;

namespace MagiQL.Framework.Interfaces.Logging
{
    public interface ILoggingProvider
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message = null, Exception exception = null); 
        void LogException(Exception ex); 
    }
}
