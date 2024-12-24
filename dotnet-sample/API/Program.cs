using API;
using Ardalis.GuardClauses;
using Azure.Identity;
using Common;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Text.Json.Serialization;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            ////https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation(EventCodes.General.Information.HOSTPROCESS_STARTING, "API: Starting host process.");

            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.Configure<HostOptions>(hostOptions =>
                    {
                        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                })
                .ConfigureAppConfiguration(config =>
                {
                    var settings = config.Build();
                    var connection = settings.GetConnectionString("AppConfig");
                    var prefix = settings.GetValueWithSuffix("Prefix", "/");
                    var label = settings["Label"] ?? "";

                    if (string.IsNullOrEmpty(connection))
                    {
                        throw new ArgumentNullException(nameof(connection), "The connection parameter to Azure App Configuration is not defined.");
                    }

                    _ = double.TryParse(settings["refresh"]?.ToString(), out var AppConfigurationCacheExpirationTimeout);


                    config.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(new Uri(connection), credential: new ChainedTokenCredential(new AzureCliCredential(), new ManagedIdentityCredential()))
                                .UseFeatureFlags(featureFlagOptions =>
                                {
                                    featureFlagOptions.Select($"*", label);
                                    featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(AppConfigurationCacheExpirationTimeout);
                                }) //Configuration keys based on prefix & label
                                .Select($"{prefix}*", label).TrimKeyPrefix($"{prefix}")
                                .Select("configurableUrls*", label).TrimKeyPrefix("configurableUrls/")
                                .ConfigureRefresh(refresh =>
                                {
                                    refresh.Register(key: $"{prefix}Sentinel", refreshAll: true)
                                           .SetCacheExpiration(TimeSpan.FromSeconds(AppConfigurationCacheExpirationTimeout));
                                }).ConfigureKeyVault(kv =>
                                {
                                    kv.SetCredential(credential: new ChainedTokenCredential(new AzureCliCredential(), new ManagedIdentityCredential()));
                                });
                    });
                })
                .ConfigureLogging((context, builder) =>
                {
                    string applicationInsightsConnectionStringKey = "ApplicationInsights:ConnectionString";
                    var applicationInsightsConnectionString = context.Configuration[applicationInsightsConnectionStringKey];
                    Guard.Against.NullOrEmpty(applicationInsightsConnectionString, nameof(applicationInsightsConnectionString), $"The {applicationInsightsConnectionStringKey} configuration parameter is not defined.");

                    builder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) =>
                        {
                            config.ConnectionString = applicationInsightsConnectionString;
                        },
                        configureApplicationInsightsLoggerOptions: (options) => { }
                    );

                    // Capture all log-level entries from Program
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(
                        typeof(Program).FullName, LogLevel.Trace);

                    // Capture all log-level entries from Startup
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(
                        typeof(Startup).FullName, LogLevel.Trace);
                });

    }
}
