using Common;
using System.Text.Json.Serialization;
using System.Text.Json;
using StackExchange.Redis;
using API.Entities;
using Microsoft.Identity.Web;
using Common.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ApplicationInsights;
using Asp.Versioning;
using FluentValidation.AspNetCore;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.FeatureManagement;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ThreadPool.SetMinThreads(150, 150);
            
            var redisSettings = Configuration.GetSection(ConfigurationSections.Redis.RedisSettings).Get<ConfigurationOptions>();
            var serviceSettingsValidateCredentals = Configuration.GetSection(ConfigurationSections.Credentials.Demo).Get<ValidateServiceSettings>();
            var serviceSettingsUserDetails = Configuration.GetSection(ConfigurationSections.UserDetails.Demo).Get<UserDetailServiceSettings>();

            var identityProvider = Configuration.GetSection(ConfigurationSections.IdentityProviders.AzureAD).Get<AzureADIdentityProvider>();

            services.AddMicrosoftIdentityWebApiAuthentication(Configuration, "IdentityProviders:Demo:AzureAd", identityProvider.AuthSchemeName, subscribeToJwtBearerMiddlewareDiagnosticsEvents: true)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddDistributedTokenCaches();

            services.Configure<JwtBearerOptions>(identityProvider.AuthSchemeName, options => {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context => {

                        //Check for IDX10214: Audience validation failed. Invalid Audience from our other supported schemes 
                        if (context.Exception is SecurityTokenInvalidAudienceException aEx && identityProvider.AzureAD.ClientId == aEx.InvalidAudience)
                        {//we know and trust this audience, don't write the log
                            return Task.CompletedTask;
                        }

                        // get TelemetryClient from DI
                        var telemetryClient = context.HttpContext.RequestServices.GetRequiredService<TelemetryClient>();

                        if (telemetryClient is not null)
                        {
                            var props = new Dictionary<string, string>();
                            const string eventName = "API-OnAuthenticationFailed";

                            if (context.Exception is not null)
                            { 
                                props.Add("EventName", eventName);

                                telemetryClient.TrackException(context.Exception, props);
                            }

                            telemetryClient.TrackEvent(eventName, props);

                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersion.Default;
                options.ReportApiVersions = true;
            });

            var authSchemeNames = new string[] { identityProvider.AuthSchemeName };

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(authSchemeNames)
                  .RequireAuthenticatedUser()
                  .Build();
                
                options.AddPolicy("Users", policy =>
                {
                    policy.RequireRole("Users.Manage");
                    policy.AddAuthenticationSchemes(authSchemeNames);
                });
            });

            services.AddControllers().AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssemblyContaining<AssistedRequest>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddApplicationInsightsTelemetry();
            services.AddSingleton(identityProvider);
            
            var clientSecretCredential = new ClientSecretCredential(identityProvider.AzureAD.TenantId.ToString(), identityProvider.AzureAD.ClientId, identityProvider.AzureAD.ClientSecret);
            var scopes = new string[] { "https://graph.microsoft.com/.default" };
            services.AddSingleton(sp => { return new GraphServiceClient(clientSecretCredential, scopes); });
            services.AddSingleton<GraphApiService>();

            services.AddSingleton<IBearerTokenManager, BearerTokenManager>();

            services.AddSingleton<UserService, ValidateService>();
            services.AddSingleton<IOldUserService, OldUserService>();

            services.AddHttpClient(Common.Constants.ValidationServiceName, client =>
            {
                client.BaseAddress = new Uri(serviceSettingsValidateCredentals.APIEndpointUrl);
            });

            services.AddHttpClient(Common.Constants.UserDetailsServiceName, client =>
            {
                client.BaseAddress = new Uri(serviceSettingsUserDetails.APIEndpointUrl);
            });

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            };

            services.AddSingleton(options);

            var endpoints = Configuration.GetValue<string>("RedisSettings:EndPoints").Split(";");  
            services.AddStackExchangeRedisCache(o =>
            {
                o.InstanceName = Configuration.GetValue<string>("RedisSettings:RedisCacheInstance");
                o.ConfigurationOptions = redisSettings;
                o.ConfigurationOptions.ReconnectRetryPolicy = new LinearRetry(Configuration.GetValue<int>("RedisSettings:ReconnectRetryPolicy"));

                for (var i = 0; i < endpoints.Length; i++)
                {
                    if (!string.IsNullOrEmpty(endpoints[i]))
                        o.ConfigurationOptions.EndPoints.Add(endpoints[i]);
                }
            });

            services.AddMemoryCache();            
            services.AddAzureAppConfiguration();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IFeatureManager featureManager, ILogger<Startup> logger)
        {
            logger.LogInformation(EventCodes.General.Information.CONFIGURATION_LOADING, $"Loading Configuration for {env.EnvironmentName} environment");

            app.UseAzureAppConfiguration();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHttpsRedirection();


        }
    }
}
