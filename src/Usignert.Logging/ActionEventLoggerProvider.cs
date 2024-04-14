using Microsoft.Extensions.Logging;

namespace Usignert.Logging
{
    public class ActionEventLoggerProvider : ILoggerProvider
    {
        public ActionEventLogger Logger { get; } = new ActionEventLogger();

        public ILogger CreateLogger(string categoryName) => Logger;

        public void Dispose() { }
    }
}
