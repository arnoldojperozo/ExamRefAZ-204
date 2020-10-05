//C# .NET Core

using Microsoft.Extensions.Configuration;

namespace ch2_2_2
{
    public class AppSettings
    {
        public string SASToken { get; set; }
        public string AccountName { get; set; }
        public string ContainerName { get; set; }

        public static AppSettings LoadAppSettings()
        {
            IConfigurationRoot configRoot = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json",false)
                .Build();
            AppSettings appSettings = configRoot.Get<AppSettings>();
            return appSettings;
        }
    }
}
