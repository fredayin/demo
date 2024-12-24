using Azure.Identity;
using Common;
using Common.BLL;
using Common.Entities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.IO.Pipelines;
using WebApp1.BLL;
using WebApp1.Model;
using static Common.ConfigurationSections;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration(config =>
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
        })
        .Select($"{prefix}*", label).TrimKeyPrefix($"{prefix}") //Configuration keys based on prefix & label
        .Select($"functionalApps*", label) //Configuration keys based on prefix & label
        .Select("translationApi*", label)
        .Select("configurableUrls*", label).TrimKeyPrefix("configurableUrls/")
        .Select("serviceBus*", label)
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
.ConfigureServices(services =>
{
    services.AddControllersWithViews();
})
.ConfigureLogging((context, builder) =>
{
    builder.AddApplicationInsights(
        configureTelemetryConfiguration: (config) => config.ConnectionString = context.Configuration["ApplicationInsights:ConnectionString"],
        configureApplicationInsightsLoggerOptions: (options) => { }
    );

    // Capture all log-level entries from Program
    builder.AddFilter<ApplicationInsightsLoggerProvider>(
        typeof(Program).FullName, LogLevel.Trace);
});

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

var identityProvider = builder.Configuration.GetSection(ConfigurationSections.IdentityProviders.AzureAD).Get<AzureADIdentityProvider>();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var tableOptions = builder.Configuration.GetSection(ConfigurationSections.Storage.TableStorage).Get<TableStorageServiceOptions>();
var webApp1Options = builder.Configuration.GetSection("Options").Get<WebApp1Options>() ?? new WebApp1Options();

builder.Services.AddTransient<IUserService, WebApp1.BLL.UserService>();
builder.Services.AddScoped<IFileProcessor, FileProcessor>();
builder.Services.AddScoped<IStreamReader, FileStreamReader>();

builder.Services.AddSingleton(tableOptions);
builder.Services.AddSingleton(identityProvider);
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();

builder.Services.AddSingleton<IFileInfoTableEntityMapper, FileInfoTableEntityMapper>();
builder.Services.AddSingleton<ISuccessTableEntityMapper, SuccessTableEntityMapper>();
builder.Services.AddSingleton<ITableClientFactory, TableClientFactory>();

builder.Services.AddSingleton(webApp1Options);
var clientSecretCredential = new ClientSecretCredential(identityProvider.AzureAD.TenantId.ToString(), identityProvider.AzureAD.ClientId, identityProvider.AzureAD.ClientSecret);
var scopes = new string[] { "https://graph.microsoft.com/.default" };
builder.Services.AddSingleton(sp => { return new GraphServiceClient(clientSecretCredential, scopes); });
builder.Services.AddSingleton<GraphApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithRedirects("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
