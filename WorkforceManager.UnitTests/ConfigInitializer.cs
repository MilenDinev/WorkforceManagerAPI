using Microsoft.Extensions.Configuration;

namespace WorkforceManager.UnitTests.Services
{
    public class ConfigInitializer
    {
        public static IConfigurationRoot InitConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.test.json", true, true);
            return builder.Build();
        }
    }
}

