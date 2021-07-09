using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.StdOutLogger.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Uwajima
{
    internal static class Program
    {
        private static async Task Main()
        {
            await Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseKestrel()
                        .UseSentry(options =>
                        {
                            options.Dsn = Environment.GetEnvironmentVariable("HTDC_UWAJIMA_SENTRY_ENDPOINT");
                            options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                            options.IncludeActivityData = true;
                            options.BeforeSend = sentryEvent =>
                            {
                                foreach (var (key, value) in OpenTelemetry.Baggage.Current)
                                    sentryEvent.SetTag(key, value);
                                    
                                sentryEvent.SetTag("TraceId", Activity.Current?.TraceId.ToString() ?? string.Empty);
                                sentryEvent.SetTag("SpanId", Activity.Current?.SpanId.ToString() ?? string.Empty);

                                return sentryEvent;
                            };
                        })
                        .UseStartup<Startup>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders()
                        .AddConfiguration(context.Configuration.GetSection("Logging"));

                    var env = context.HostingEnvironment;
                    if (env.IsEnvironment("Local"))
                        logging.AddConsole();
                    else
                    {
                        logging.AddStdOutLogger(setup =>
                        {
                            setup.IncludeScopes = true;
                            setup.RequestIdHeader = Constants.DefaultRequestIdHeader;
                            setup.UseUtcTimestamp = true;
                        });
                        logging.AddSentry(c =>
                        {
                            c.Dsn = context.Configuration[context.Configuration["Logging:Sentry:Endpoint"]];
                            c.Environment = env.EnvironmentName;
                        });
                    }
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    var environment = context.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables()
                        .AddConsulKeyValueClient(Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? throw new InvalidOperationException("Consul endpoint is not set"),
                        "uwajima",
                        Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN") ?? throw new InvalidOperationException("Consul http token is not set"));
                })
                .Build()
                .RunAsync();
        }
    }
}