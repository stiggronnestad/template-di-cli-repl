using Microsoft.Extensions.Logging;
using System.Reflection;
using Usignert.CommandLine;
using Usignert.Logging;

namespace Usignert.App.Cli
{
    internal sealed class AppCli
    {
        private readonly ILogger _logger;
        private readonly CommandsHost _commandsHost;

        public AppCli(ILogger<AppCli> logger, CommandsHost commandsHost)
        {
            _logger = logger;
            _logger.BeginScope("App");
            _commandsHost = commandsHost;

            _commandsHost
                .AddAssembly(Assembly.GetExecutingAssembly())
                .AddAssembly(Assembly.GetAssembly(typeof(Commands.CommandBase))!)
                .Build("App example Repl.")
            ;
        }

        public async Task<int> Run(string[] args)
        {
            _logger.LogExtendedInformation("App 1.0");
            int returnCode = await _commandsHost.Parse(args);

            if (returnCode > 0)
            {
                _logger.LogExtendedError($"Error! Code ='{returnCode}'");
            }
            else
            {
                _logger.LogExtendedInformation("OK!");
            }

            return returnCode;
        }
    }
}
