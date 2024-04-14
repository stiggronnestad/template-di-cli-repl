using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Usignert.Logging;

namespace Usignert.Di
{
    public static class DiContainers
    {
        public static IHostBuilder DefaultHostBuilder<TProgram, TConfig>(DiOptions options, params string[] args)
            where TConfig : class, IConfig, new()
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("config.json");

                if (options.ConfigLocation != DiOptions.AppDataLocation.AssemblyPath)
                {
                    builder.AddJsonFile(options.ConfigPath, optional: true);
                }

                builder.AddCommandLine(args);
                //builder.AddUserSecrets
                //builder.AddEnvironmentVariables()
            });

            builder.ConfigureLogging((hostContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole((options) =>
                {
                    options.FormatterName = "custom";
                })
                .AddConsoleFormatter<PrettyConsoleFormatter, ConsoleFormatterOptions>();
            });

            builder.ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration.Get<TConfig>() ?? new TConfig();
                services.RegisterServices(typeof(TProgram));
                services.AddSingleton(config);

                LoggingFramework.Configure(services, options.LogPath);
                LoggingExtensions.ApplyConfig(config);
            });

            return builder;
        }
    }
}
