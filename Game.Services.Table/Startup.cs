using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: FunctionsStartup(typeof(Game.Services.Table.Startup))]

namespace Game.Services.Table
{
    public class Startup : FunctionsStartup
    { 
        public IConfigurationRefresher ConfigurationRefresher { get; private set; }

        public override void Configure(IFunctionsHostBuilder hostBuilder)
        {
            if (ConfigurationRefresher != null)
            {
                hostBuilder.Services.AddSingleton(ConfigurationRefresher);
            }
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder configurationBuilder)
        {  
            var hostBuilderContext = configurationBuilder.GetContext();
            var isDevelopment = ("Development" == hostBuilderContext.EnvironmentName);
            if (isDevelopment)
            {
                configurationBuilder.ConfigurationBuilder
                    .AddJsonFile(Path.Combine(hostBuilderContext.ApplicationRootPath, $"appsettings.{hostBuilderContext.EnvironmentName}.json"), optional: true, reloadOnChange: false);
            }
            base.ConfigureAppConfiguration(configurationBuilder);
            configurationBuilder.ConfigurationBuilder.Build();
        }
    }
}
