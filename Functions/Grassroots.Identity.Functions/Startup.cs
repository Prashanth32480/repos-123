using Azure.Identity;
using Grassroots.Common.BlobStorage.ServiceInterface;
using Grassroots.Common.BlobStorage.ServiceInterface.Storage;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Common.PublishEvents.ChangeTrack.Listener;
using Grassroots.Common.PublishEvents.EventGrid;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Functions;
using Grassroots.Identity.Functions.Common;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Grassroots.Identity.Functions.PlayHQ.Registration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Grassroots.Identity.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            AddSharedConnections(builder.Services);
            AddTelemetry(builder.Services);
            AddDatabaseOperations(builder.Services);
            AddFeedStorageFunctionServices(builder.Services);
            AddHttpConnection(builder.Services);
            AddCommonFunctionServices(builder.Services);
            AddFunctionServices(builder.Services);
            AddFeatureFlag(builder.Services);
            AddPushEventFeedFunctionServices(builder.Services);
        }

        private static void AddDatabaseOperations(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDatabaseConnectionFactory>(x =>
                new SqlDatabaseConnectionFactory(x.GetRequiredService<IConfigProvider>(), AppSettingsKey.IdentityDbConnectionString));
            serviceCollection.AddTransient<IFeedOperations, FeedOperations>();
            serviceCollection.AddTransient<IFeedEventOperations, FeedEventOperations>();
            serviceCollection.AddTransient<IParticipantOperations, ParticipantOperations>();
            serviceCollection.AddTransient<IParticipantMappingOperations, ParticipantMappingOperations>();
        }
        private static void AddCommonFunctionServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRawFeedProcessor, RawFeedProcessor>();
            serviceCollection.AddTransient<IFeedEventProcessor, FeedEventProcessor>();
        }
        private static void AddFeatureFlag(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IFeatureFlag, FeatureFlag>();
        }
        private static void AddPushEventFeedFunctionServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IChangeTrack, ChangeTrack>();
            serviceCollection.AddTransient<IEventPushListener, EventPushListener>();
            serviceCollection.AddTransient<IEventGridClient, EventGridClient>();
        }
        private static void AddHttpConnection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient();
        }

        private static void AddFunctionServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IPlayHQProfileFeedHandler, PlayHQProfileFeedHandler>();
            serviceCollection.AddTransient<IPlayHQProfileFeedProcessor, PlayHQProfileFeedProcessor>();
            serviceCollection.AddTransient<IPlayHQRegistrationFeedHandler, PlayHQRegistrationFeedHandler>();
            serviceCollection.AddTransient<IPlayHQRegistrationFeedProcessor, PlayHQRegistrationFeedProcessor>();
        }

        private static void AddFeedStorageFunctionServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IFeedConverter, FeedConverter>();
            serviceCollection.AddTransient<IStoreFeed, StoreFeed>();
        }

        private static void AddTelemetry(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ITelemetryHandler, TelemetryHandler>();
        }

        private static void AddSharedConnections(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IConfigProvider, ConfigProvider>();
            serviceCollection.AddTransient<IBlobStorage, BlobStorage>();
            serviceCollection.AddSingleton<IBlobStorageClientHelper, BlobStorageClientHelper>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);

            var builtConfig = builder.ConfigurationBuilder.Build();

            bool.TryParse(builtConfig["IdentityIsLocal"], out var isLocal);
            var keyVaultUrl = builtConfig["GrassrootsKeyVaultBaseUrl"];

            if (isLocal || string.IsNullOrWhiteSpace(keyVaultUrl))
                return;

            builder.ConfigurationBuilder.AddAzureKeyVault(new System.Uri(keyVaultUrl), new DefaultAzureCredential());
        }
    }
}

