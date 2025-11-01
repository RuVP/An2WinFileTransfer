using System;

namespace An2WinFileTransfer.Interfaces
{
    public interface ILoggingService
    {
        void Info(string message, bool includeUI = true);
        void Warn(string message, bool includeUI = true);
        void Error(string message, Exception ex = null, bool includeUI = true);
    }
}
