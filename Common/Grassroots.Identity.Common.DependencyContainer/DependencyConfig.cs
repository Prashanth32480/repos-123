using System;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Microsoft.Extensions.Configuration;
using SimpleInjector;


namespace Grassroots.Identity.Common.DependencyContainer
{
    public static class DependencyConfig
    {
        public static Container Configuration(Container container,
            Func<Container, Container> postRegistration = null)
        {
            SetupFeatureFlags(container);
            SetupDatabaseOperations(container);
            //SetupClientOperations(container);
            container = postRegistration != null ? postRegistration(container) : container;

            container.Verify();

            return container;
        }

        private static void SetupFeatureFlags(Container container)
        {
            container.RegisterSingleton<IConfigProvider, ConfigProvider>();
            container.RegisterSingleton<IFeatureFlag, FeatureFlag>();
        }

        private static void SetupDatabaseOperations(Container container)
        {
            // SQL DB
            container.Register<IDatabaseConnectionFactory>(() => new SqlDatabaseConnectionFactory(new ConfigProvider(container.GetInstance<IConfiguration>()), AppSettingsKey.IdentityDbConnectionString));
            container.Register<IVersionOperations, VersionOperations>(Lifestyle.Transient);

        }

        private static void SetupClientOperations(Container container)
        {
            //container.Register<IRestClientFactory, RestClientFactory>(Lifestyle.Transient);
            //container.Register<IWebClient, WebClient>(Lifestyle.Transient);
        }
    }
}