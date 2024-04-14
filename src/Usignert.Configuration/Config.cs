using Usignert.Di;
using Usignert.Logging;

namespace Usignert.Configuration
{
    public sealed class Config : IConfig, ILoggingConfig
    {
        public LoggingExtensions.LogVerbosity Verbosity { get; set; } = LoggingExtensions.LogVerbosity.Normal;
    }
}
