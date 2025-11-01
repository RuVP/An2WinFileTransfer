using System;
using An2WinFileTransfer.Interfaces;
using Serilog;

namespace An2WinFileTransfer.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger _logger;
        private readonly Action<string> _uiLogAction;

        public LoggingService(ILogger logger, Action<string> uiLogAction = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uiLogAction = uiLogAction;
        }

        public void Info(string message, bool includeUI = true)
        {
            _logger.Information(message);

            if (includeUI)
            {
                _uiLogAction?.Invoke(message);
            }
        }

        public void Warn(string message, bool includeUI = true)
        {
            _logger.Warning(message);

            if (includeUI)
            {
                _uiLogAction?.Invoke("⚠️ " + message);
            }
        }

        public void Error(string message, Exception ex = null, bool includeUI = true)
        {
            _logger.Error(ex, message);

            if (includeUI)
            {
                _uiLogAction?.Invoke($"❌ {message}: {ex?.Message}");
            }
        }
    }
}
