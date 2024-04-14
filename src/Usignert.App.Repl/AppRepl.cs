using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Usignert.CommandLine;
using Usignert.Di;
using Usignert.Logging;

namespace Usignert.App.Repl
{
    [DiService(DiServiceAttribute.DiServiceType.Singleton)]
    internal sealed class AppRepl : IHostedService
    {
        private readonly ILogger _logger;
        private readonly CommandsHost _commandsHost;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public AppRepl(ILogger<AppRepl> logger, CommandsHost commandsHost)
        {
            _logger = logger;
            _commandsHost = commandsHost;
            _logger.BeginScope("App Repl 1.0");

            _commandsHost
                .AddAssembly(Assembly.GetExecutingAssembly())
                .AddAssembly(Assembly.GetAssembly(typeof(App.Commands.CommandBase))!)
                .AddAssembly(Assembly.GetAssembly(typeof(Logging.Commands.LoggingCommand))!)
                .Build("App example Repl.")
            ;
        }

        public static CancellationTokenSource GetCancellationTokenSource() => _cancellationTokenSource;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(Run, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public async Task Run()
        {
            _logger.LogExtendedInformation("App Repl 1.0");
            _logger.LogExtendedWarning("Type 'exit' to exit the REPL");

            var readLineTask = Task.Run(ReadLine);

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Check if the readLineTask has completed and got a line
                if (readLineTask.IsCompleted)
                {
                    var line = readLineTask.Result;

                    if (line != null && !string.IsNullOrEmpty(line))
                    {
                        var returnCode = await _commandsHost.Parse(line);

                        if (returnCode > 0)
                        {
                            _logger.LogExtendedError($"Error! Code ='{returnCode}'",
                                minimalVerbosity: LoggingExtensions.LogVerbosity.Verbose);
                        }
                        else
                        {
                            _logger.LogExtendedInformation("OK!",
                                minimalVerbosity: LoggingExtensions.LogVerbosity.Verbose);
                        }
                    }

                    // Reset the readLineTask to read next line
                    readLineTask = Task.Run(ReadLine);
                }

                // Avoid 100% CPU usage in the loop.
                await Task.Delay(100);
            }
        }

        private string? ReadLine()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("> ");
            var read = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            _logger.LogExtendedInformation("> " + (read ?? "null"), minimalVerbosity: LoggingExtensions.LogVerbosity.Verbose);
            return read;
        }

    }
}
