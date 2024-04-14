using Microsoft.Extensions.DependencyInjection;
using Usignert.App.Data.Context;
using Usignert.App.Models;
using Usignert.App.Repositories;
using Usignert.App.Services;
using Usignert.CommandLine;
using Usignert.Configuration;
using Usignert.Di;

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
