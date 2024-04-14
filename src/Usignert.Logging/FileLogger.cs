using Microsoft.Extensions.Logging;

namespace Usignert.Logging
{
    public class FileLogger(string filePath) : ILogger
    {
        private readonly string _filePath = filePath;
        private readonly object _lock = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = formatter(state, exception);

            message = message.Replace("¨ic", "");
            message = message.Replace("¨cs", "");
            message = message.Replace("¨ce", "");

            WriteTextToFile(message);
        }

        private void WriteTextToFile(string message)
        {
            lock (_lock)
            {
                File.AppendAllText(_filePath, $"{DateTime.Now}: {message}\n");
            }
        }
    }
}
