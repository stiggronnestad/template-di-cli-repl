# Template-DI-CLI-REPL

Template project with example implementation for DI and CLI/REPL applications.

## Features

- Dependency Injection
- Automatic service registration via reflection
- Command Line Interface (CLI)
- Read-Eval-Print Loop (REPL)
- Command Definitions injection via reflection
- Extended logging
- Extended configuration
- "EF" like model/dto handling

## Projects

`Usignert` is a fictional company name.

`App` is a stand-in for the actual application name.

| Project                   | Type        | Scope        | Description                                                           |
| ------------------------- | ----------- | ------------ | --------------------------------------------------------------------- |
| Usignert.App.Api          | Dll         | App          | The shared API for the application.                                   |
| Usignert.App.Cli          | Application | App          | The CLI application.                                                  |
| Usignert.App.Commands     | Dll         | App          | The command definitions used in CLI/REPL (invokes API).               |
| Usignert.App.Data.Context | Dll         | App          | The database/file-handling context.                                   |
| Usignert.App.Data.Models  | Dll         | App          | The database/file-handling models. POCO-class for stored data.        |
| Usignert.App.Dtos         | Dll         | App          | The data transfer objects / intermediate used in the app.             |
| Usignert.App.Repl         | Application | App          | The REPL application.                                                 |
| Usignert.App.Repositories | Dll         | App          | The database/file-handling repositories.                              |
| Usignert.App.Services     | Dll         | App          | The services for the application.                                     |
| Usignert.CommandLine      | Dll         | NuGet/Shared | The command-line parsing library.                                     |
| Usignert.Configuration    | Dll         | NuGet/Shared | The configuration library contains base implementation of app config. |
| Usignert.Di               | Dll         | NuGet/Shared | The dependency injection library.                                     |
| Usignert.Logging          | Dll         | NuGet/Shared | The logging library.                                                  |
| Usignert.Logging.Commands | Dll         | NuGet/Shared | The logging command definitions used in CLI/REPL.                     |

## Basic Usage

### DI Options

`DIOptions` is a helper class for setting default paths for configuration, logging, and data storage.

`AssemblyPath`, `RoamingPath` or specific paths can be set.

```csharp
var diOptions = DiOptions.Empty
	.WithAppDataLocation(DiOptions.AppDataLocation.RoamingPath)
	.WithConfigLocation(DiOptions.AppDataLocation.AssemblyPath)
;

var roamingPath = diOptions.RoamingDirectory;
var assemblyPath = diOptions.AssemblyDirectory;
var configPath = diOptions.ConfigPath;
var logPath = diOptions.LogPath;
```

### DI

Automatic service registration via reflection, (actual implementation in `Usignert.Di`).

```csharp
namespace Usignert.Di
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection RegisterServices(this IServiceCollection services, Type entryType) {}
		public static IServiceCollection RegisterServices<T>(this IServiceCollection services) {}
		public static IServiceCollection RegisterServices(this IServiceCollection services, params Type[] types) {}
		private static (Type[] type, DiServiceAttribute[] attribute) GetServices(Type entryType) {}
		private static (Type[] type, DiServiceAttribute[] attribute) GetServices<T>() {}
	}
}
```

### DiContainers

The `DiContainers` class is used to create a `HostBuilder` for the application.

```csharp
var builder = DiContainers.DefaultHostBuilder<Program, Config>(diOptions, args);
```

The `DefaultHostBuilder` is a helper method that sets up the `HostBuilder` with the shared implementations for CLI/REPL:

- Config registration with `config.json` as default; if external path is selected in DIOptions it will be used instead.
- AddCommandLine(args)
- Logging configuration (PrettyConsoleFormatter is used)
- Basic service registration

### REPL / CLI

Basic REPL Program implementation, the implementation for `CLI` is very similar.

```csharp
namespace Usignert.App.Repl
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var diOptions = DiOptions.Empty
                .WithAppDataLocation(DiOptions.AppDataLocation.RoamingPath)
                .WithConfigLocation(DiOptions.AppDataLocation.AssemblyPath)
            ;

            var builder = DiContainers.DefaultHostBuilder<Program, Config>(diOptions, args);

            builder.ConfigureServices((hostContext, services) =>
            {
                services.RegisterServices(typeof(ExampleService), typeof(CommandsHost));
                services.AddSingleton(provider => new FileSystemDbContext<PositionModel>(diOptions.RoamingDirectory));
                services.AddSingleton<IRepository<PositionModel>, FileRepository<PositionModel>>();
            });

            var app = builder.Build();
            await app.Services.GetRequiredService<AppRepl>().Run();
        }
    }
}
```

### Command Definition

Example implementation from `Logging.Commands` where the verbosity of logging can be configured.

```csharp
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
```

Services can be injected into commands via constructor injection. The `CommandsHost` class is using `ActivatorUtilities.CreateInstance` to create instances of the command classes.

```csharp
private Command CreateCommand(CommandAttribute commandAttribute, Type type)
{
    //  Metadata for Command
    var commandName = commandAttribute.Name;
    var commandDescription = commandAttribute.Description;
    var commandInstance = ActivatorUtilities.CreateInstance(_serviceProvider, type) as ICommand;

    var command = new Command(commandName, commandDescription);
	// [...]
}
```

Example implementation from `App.Commands` where the API can be called.

```csharp
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

		// [...]
	}
}
```

### Command Injection

The `CommandsHost` class is used to inject command definitions via reflection, and to parse the input.

Usage shown in the `AppRepl` example class (some code omitted).

```csharp
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

		// [...]

        public async Task Run()
        {
            // [...]

			var returnCode = await _commandsHost.Parse(line);

            // [...]
        }

        // [...]
    }
}
```