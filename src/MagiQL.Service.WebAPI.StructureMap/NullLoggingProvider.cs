using System;
using MagiQL.Framework.Interfaces.Logging;

namespace MagiQL.Service.WebAPI.StructureMap
{
    public class NullLoggingProvider : ILoggingProvider
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
    }
}