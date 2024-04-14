using Usignert.CommandLine;

namespace Usignert.Logging.Commands
{
    [Command("logging", "Commandset for handling logging.")]
    public sealed class LoggingCommand : ICommand
    {
        [SubCommand("configure", "Configure logging.")]
        public void Configure(
            [Argument("Set the verbosity.")] LoggingExtensions.LogVerbosity verbosity
                = LoggingExtensions.LogVerbosity.Minimal
        )
        {
            LoggingExtensions.Verbosity = verbosity;
        }

        public void Execute() { }
    }
}
