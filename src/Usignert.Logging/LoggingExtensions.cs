using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Usignert.Logging
{
    public static class LoggingExtensions
    {
        public enum LogVerbosity : uint
        {
            Verbose = 0,
            Normal = 1,
            Minimal = 2
        }

        public static LogVerbosity Verbosity { get; set; } = LogVerbosity.Minimal;

        public const string LogMessageTemplateMinimal = "{¨ic}{colorStart}{message}{colorEnd}";
        public const string LogMessageTemplateNormal = "{¨ic}[{timeNow}] {colorStart}[{logLevel}] {message}{colorEnd}";
        public const string LogMessageTemplateVerbose = "{¨ic}[{dateNow} {timeNow}] {colorStart}[{system}:{logLevel}]{colorEnd} [{fileName}:{memberName}:{lineNumber}] {colorStart}{message}{colorEnd}";

        public static void ApplyConfig(ILoggingConfig config)
        {
            Verbosity = config.Verbosity;
        }

        public static void LogExtended(this ILogger logger, string message, string system = "main",
            LogLevel logLevel = LogLevel.Information, LogVerbosity minimalVerbosity = LogVerbosity.Minimal,
            [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var dateNow = DateTime.Now.ToShortDateString();
            var timeNow = DateTime.Now.ToShortTimeString();
            var fileName = Path.GetFileName(filePath);

            var colorInit = "¨ic";
            var colorStart = "¨cs";
            var colorEnd = "¨ce";

            if (minimalVerbosity < Verbosity)
                return;

            switch (Verbosity)
            {
                case LogVerbosity.Minimal:
                    logger.Log(logLevel, LogMessageTemplateMinimal, colorInit, colorStart, message, colorEnd);
                    return;

                case LogVerbosity.Normal:
                    logger.Log(logLevel, LogMessageTemplateNormal,
                                               colorInit, timeNow, colorStart, logLevel, message, colorEnd);
                    return;
                case LogVerbosity.Verbose:
                    logger.Log(logLevel, LogMessageTemplateVerbose,
                               colorInit, dateNow, timeNow, colorStart, system, logLevel, colorEnd,
                               fileName, memberName, lineNumber, colorStart, message, colorEnd);
                    break;
                default:
                    break;
            }
        }

        public static void LogExtendedInformation(this ILogger logger, string message,
            string system = "main", LogVerbosity minimalVerbosity = LogVerbosity.Minimal,
            [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            logger.LogExtended(message, system, LogLevel.Information, minimalVerbosity, memberName, filePath, lineNumber);
        }

        public static void LogExtendedWarning(this ILogger logger, string message,
            string system = "main", LogVerbosity minimalVerbosity = LogVerbosity.Minimal,
            [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            logger.LogExtended(message, system, LogLevel.Warning, minimalVerbosity, memberName, filePath, lineNumber);
        }

        public static void LogExtendedError(this ILogger logger, string message,
            string system = "main", LogVerbosity minimalVerbosity = LogVerbosity.Minimal,
            [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            logger.LogExtended(message, system, LogLevel.Error, minimalVerbosity, memberName, filePath, lineNumber);
        }
    }
}
