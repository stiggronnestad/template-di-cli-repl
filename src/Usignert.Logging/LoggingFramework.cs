using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Usignert.Logging
{
    public static class LoggingFramework
    {
        public static void Configure(IServiceCollection services, string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "logs/log.txt";
            }

            var directory = Path.GetDirectoryName(filePath) ?? throw new DirectoryNotFoundException("Directory not found.");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var eventLoggerProvider = new ActionEventLoggerProvider();
            var eventLogger = eventLoggerProvider.CreateLogger("EventLogger") as ActionEventLogger;
            services.AddSingleton(eventLogger!);

            services.AddLogging(builder =>
            {
                builder.AddProvider(new FileLoggerProvider(filePath));
                builder.AddProvider(eventLoggerProvider);
            });
        }
    }
}
