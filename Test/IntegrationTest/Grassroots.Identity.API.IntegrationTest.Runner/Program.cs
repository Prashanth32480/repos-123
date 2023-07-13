using System;
using System.Threading.Tasks;
using Grassroots.IntegrationTest.Common.Helpers;
using Grassroots.IntegrationTest.Common.Models;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Database.Infrastructure.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using Grassroots.Identity.Common;

namespace Grassroots.Identity.API.IntegrationTest.Runner
{
    internal class Program
    {
        private static async Task Main()
        {
            var container = new Container();
            var host = new HostBuilder()
                .ConfigureHostConfiguration((config) =>
                {
                    config.AddJsonFile("integrationTestSettings.json", optional: false, true);
                    config.AddJsonFile("environmentSettings.json", optional: false, true);
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                }).
                ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(hostContext.Configuration);
                    services.AddSimpleInjector(container, options =>
                    {
                        options.AddHostedService<Startup>();
                        options.AddLogging();
                    }); 
                })
                .UseConsoleLifetime()
                .Build()
                .UseSimpleInjector(container);

            ILogger logger = host.Services.GetService<ILogger<Program>>();

            ConfigureSimpleInjector(container);

            try
            {
                await host.RunAsync();
            }
            catch (Exception exception)
            {
                logger.LogInformation(LoggingEvents.MainMethod, " Problem Occured Running Integration Tests: " + exception);
            }
        }

        private static void ConfigureSimpleInjector(Container container)
        {
            container.Register<DataFeeder>();
            container.Register<IDatabaseConnectionFactory>(() => new SqlDatabaseConnectionFactory(new ConfigProvider(container.GetInstance<IConfiguration>()), AppSettingsKey.IdentityDbConnectionString), Lifestyle.Singleton);
            container.Register<IConfigProvider, ConfigProvider>(Lifestyle.Singleton);
        }
    }
}