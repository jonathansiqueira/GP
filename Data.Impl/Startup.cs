
using Enyim.Caching.Configuration;
using H2HGermPlasmProcessor.Data;
using H2HGermPlasmProcessor.Data.EntryMeans;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Amazon.ElastiCacheCluster;
using System;
using System.Collections.Generic;
using System.Text;
using Enyim.Caching;
using Microsoft.Extensions.Options;
using PingHelper;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddLogging();
            services.AddTransient<IQueue, SQSWrapper>();
            services.AddTransient<INotifier, SNSWrapper>();
            services.AddTransient<IProductAnalyticsAPIClient, ProductAnalyticsAPIClient>();
            services.AddTransient<IHeadtoHeadAPIClient, HeadtoHeadAPIClient>();
            services.AddTransient<ICache, CachePersister>();
            services.AddTransient<IUDRData, UDRAPIClient>();
            services.AddTransient<IHttpClientFactory, HttpClientFactory>();
            IEncryptedEnvVariable encryptedEnvVariable = new EncryptedEnvVariable();
            services.AddSingleton<IEncryptedEnvVariable>(encryptedEnvVariable);
            services.AddMemoryCache();

            services.AddOptions();

            services.Configure<PingSettingsOptions>(o =>
            {
                o.PingAPIUrl = Environment.GetEnvironmentVariable("OauthUrl");
                o.ClientID = Environment.GetEnvironmentVariable("ServiceClientID");
                o.ClientSecret = encryptedEnvVariable.DecodeEnvVarAsync("ServiceClientSecret").Result;
                o.PingTokenTimeout = TimeSpan.FromHours(Double.Parse(Environment.GetEnvironmentVariable("TokenTimeoutHoursDecimal")));
            });

            services.AddTransient<IPingSettings, PingSettings>();
            services.AddTransient<IOAuthClient, PingIDClient>();

            services.Configure<SlackAPIOptions>(o =>
            {
                o.SlackAppName = Environment.GetEnvironmentVariable("SlackAppName");
                o.SlackWebhookUrl = Environment.GetEnvironmentVariable("SlackWebhookUrl");
            });
            services.AddSingleton<ISlackAPI, SlackAPI>();
           
            var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();

            IConfiguration Configuration = builder.Build();
            IConfigurationSection serverSection = Configuration.GetSection("enyimMemcached_cfgserver");
            IConfigurationSection portSection = Configuration.GetSection("enyimMemcached_port");
            services.AddTransient<IMemcachedClient, MemcachedClient>((IServiceProvider) =>
            {
                var config = new ElastiCacheClusterConfig(serverSection.Value, Int32.Parse(portSection.Value));
                return new MemcachedClient(loggerFactory, config);
            });

        }
    }
}
