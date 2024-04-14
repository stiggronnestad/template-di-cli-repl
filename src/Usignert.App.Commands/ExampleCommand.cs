using Microsoft.Extensions.Logging;
using Usignert.App.Api;
using Usignert.CommandLine;
using Usignert.Logging;

namespace Usignert.App.Commands
{
    // Command with description
    [Command("api", "Get message from the API.")]
    public sealed class ExampleCommand : CommandBase
    {
        private readonly ILogger<ExampleCommand> _logger;

        // DI services can be injected into the constructor.
        public ExampleCommand(ILogger<ExampleCommand> logger)
        {
            _logger = logger;
        }

        // Executed when no subcommand is supplied
        public override void Execute()
        {
            System.Console.WriteLine(ExampleApi.GetMessage());
        }

        // Subcommand example
        [SubCommand("log", "Testing subcommand logging.")]
        public void Log(
            // Arguments, needed or optional
            [Argument("Type.")] LogLevel logLevel = LogLevel.Information
        )
        {
            if (logLevel == LogLevel.Information)
            {
                _logger.LogExtendedInformation("Information.");
            }

            if (logLevel == LogLevel.Warning)
            {
                _logger.LogExtendedWarning("Warning");
            }

            if (logLevel == LogLevel.Error)
            {
                _logger.LogExtendedError("Error");
            }
        }
    }
}
