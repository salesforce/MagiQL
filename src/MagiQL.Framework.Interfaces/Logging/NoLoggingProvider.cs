using System;

namespace MagiQL.Framework.Interfaces.Logging
{
    public class NoLoggingProvider : ILoggingProvider
    {
        public void LogDebug(string message)
        {

        }

        public void LogInfo(string message)
        {

        }

        public void LogWarning(string message)
        {
            
        }

        public void LogError(string message = null, Exception exception = null)
        {
            
        }

        public void LogException(Exception ex)
        {
           
        }

        public void LogException(Exception ex, ILogInfo logInformation)
        {
            
        }

        public void LogException(Exception ex, int source, int? taskId)
        {
            
        }
    }
}
