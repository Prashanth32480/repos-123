namespace Grassroots.Identity.Common
{
    public static class AppSettingsKey
    {

        public static string IdentityDbConnectionString = "IdentityDbConnectionString";
        public static string ProductionFlag = "IdentityProductionFlag";
        public static bool ProductionFlagDefault = true;
        public static string UseCamelCaseFlag = "IdentityUseCamelCaseFlag";
        public static bool UseCamelCaseFlagDefault = false;
        public static string DisableSwaggerUiFlag = "IdentityDisableSwaggerUiFlag";
        public static bool DisableSwaggerUiFlagDefault = true;
        public static string IdentityIsLocal = "IdentityIsLocal";
        public static string KeyVaultBaseUrl = "IdentityKeyVaultBaseUrl";
        public static string IdentityEnvironment = "identityEnvironment";
    }
}