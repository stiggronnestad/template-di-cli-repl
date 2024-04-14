using Microsoft.Extensions.Logging;

namespace Usignert.Logging
{
    public class ActionEventLogger : ILogger
    {
        public event Action<LogLevel, string>? MessageLogged;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            OnMessageLogged(logLevel, message);
        }

        protected virtual void OnMessageLogged(LogLevel logLevel, string message)
        {
            MessageLogged?.Invoke(logLevel, message);
        }
    }
}
