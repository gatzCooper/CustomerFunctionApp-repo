using azChallengeFA_Customer.services;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using static System.Net.WebRequestMethods;

[assembly: FunctionsStartup(typeof(azChallengeFA_Customer.StartUp))]
namespace azChallengeFA_Customer
{
    public class StartUp : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            var context = builder.GetContext();

            var serviceProvider = services
                .BuildServiceProvider();

            var hostingEnvironment = serviceProvider
               .GetService<IHostingEnvironment>();

            var configurationBuilder = new ConfigurationBuilder()
               .SetBasePath(context.ApplicationRootPath)
               .AddEnvironmentVariables();

            //var configurationBuilder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("local.settings.json");

            var newConfiguration = configurationBuilder.Build();
            services.AddSingleton<IConfiguration>(newConfiguration);

            var instrumentationKey = newConfiguration["APPINSIGHTS_INSTRUMENTATIONKEY"];

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();

            var appInsightsConnectionString = newConfiguration["APPINSIGHTS_CONNECTIONSTRING"];
            telemetryConfiguration.ConnectionString = appInsightsConnectionString;
            var module = new DependencyTrackingTelemetryModule();
            module.Initialize(telemetryConfiguration);

            services
                .AddTransient<ICustomerService, CustomerService>();

            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var cosmosDbConnectionString = configuration["azNoSqlConnectionString"];

                var cosmosClientBuilder = new CosmosClientBuilder(cosmosDbConnectionString);
                return cosmosClientBuilder
                    .WithConnectionModeDirect()
                    .Build();
            });

            services.AddSingleton(serviceProvider =>
            {
                var cosmosClient = serviceProvider.GetService<CosmosClient>();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var cosmosDbDatabaseName = configuration["CosmosDbDatabaseName"];

                return cosmosClient.GetDatabase(cosmosDbDatabaseName);
            });
        }
    }
}
