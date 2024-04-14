using Microsoft.Extensions.DependencyInjection;
using Usignert.App.Data.Context;
using Usignert.App.Models;
using Usignert.App.Repositories;
using Usignert.App.Services;
using Usignert.CommandLine;
using Usignert.Configuration;
using Usignert.Di;

namespace Usignert.App.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var diOptions = DiOptions.Empty
                .WithAppDataLocation(DiOptions.AppDataLocation.RoamingPath)
                .WithConfigLocation(DiOptions.AppDataLocation.RoamingPath)
            ;

            var builder = DiContainers.DefaultHostBuilder<Program, Config>(diOptions, args);

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<CommandsHost>();
                services.AddSingleton<AppCli>();
                services.AddSingleton(provider => new FileSystemDbContext<PositionModel>(diOptions.RoamingDirectory));
                services.AddSingleton<IRepository<PositionModel>, FileRepository<PositionModel>>();
                services.AddTransient<ExampleService>();
            });

            var app = builder.Build();
            return await app.Services.GetRequiredService<AppCli>().Run(args);
        }
    }
}
