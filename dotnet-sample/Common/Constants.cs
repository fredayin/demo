
namespace Common
{
    public class Constants
    {
        public const string ValidationServiceName = "ValidationService";
        public const string UserDetailsServiceName = "UserDetailsService";
        public const string AppClientID = "32900db0-d08a-4258-93be-3e5ef5c0e5f5";
        public const string ServiceBusIdentifier = "ServiceBus";
        public const string EncryptionKey = "hsjkjf273(KFJOIWJEK:G&*QDKMEW1sPS";
    }

    public class ConfigurationSections
    {

        public static class IdentityProviders
        {
            public const string AzureAD = "IdentityProviders";
        }


        public static class Redis
        {
            public const string RedisSettings = "RedisSettings";
        }

        public static class Storage
        {
            public const string TableStorage = "TableStorage";
        }

        public static class Credentials
        {
            public const string Demo = "DemoValidateCredentialsSettings";
        }

        public static class UserDetails
        {
            public const string Demo = "DemoUserDetailsSettings";
        }


        public static class Error
        {
            public const string UserExceptionMessages = "UserExceptionMessages";
        }

    }

    public class UserExtensionKeys
    {
        public const string FirstName = "firstName";
        public const string LastName = "lastName";
        public const string ContactNumber = "contactNumber";
        public const string pwdLastUpdated = "pwdLastUpdatedDtime";
    }
}
