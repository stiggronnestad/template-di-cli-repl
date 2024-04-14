using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Usignert.Logging
{
    public class PrettyConsoleFormatter : ConsoleFormatter
    {
        public PrettyConsoleFormatter() : base("custom") { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            var colorStart = string.Empty;

            switch (logEntry.LogLevel)
            {
                case LogLevel.Debug:
                case LogLevel.Information:
                    colorStart = "\u001b[32m";
                    break;
                case LogLevel.Warning:
                    colorStart = "\u001b[33m";
                    break;
                case LogLevel.Trace:
                case LogLevel.Error:
                case LogLevel.Critical:
                    colorStart = "\u001b[31m";
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }

            // Set to white
            string? colorInit = "\u001b[0m";
            string? colorEnd = "\u001b[0m";

            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            message = message.Replace("¨ic", colorInit);
            message = message.Replace("¨cs", colorStart);
            message = message.Replace("¨ce", colorEnd);

            if (!string.IsNullOrEmpty(message))
            {
                // Customize the output format here
                textWriter.WriteLine($"{message}");
            }
        }
    }
}
