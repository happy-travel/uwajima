using System;
using System.Collections.Generic;
using HappyTravel.Uwajima.Services;
using HappyTravel.Uwajima.Infrastructure;
using HappyTravel.Uwajima.Infrastructure.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.Telemetry.Extensions;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace HappyTravel.Uwajima
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            using var vaultClient = new VaultClient.VaultClient(new VaultOptions
            {
                Engine = _configuration["Vault:Engine"],
                Role = _configuration["Vault:Role"],
                BaseUrl = new Uri(_configuration[_configuration["Vault:Endpoint"]])
            });
            vaultClient.Login(_configuration[_configuration["Vault:Token"]]).Wait();

            services.AddHttpClients(_configuration, vaultClient);
            services.AddHostedService<StatusUpdateService>();
            services.AddHealthChecks();
            services.AddMemoryCache();
            services.AddTracing(_configuration, options =>
            {
                options.ServiceName = $"{_environment.ApplicationName}-{_environment.EnvironmentName}";
                options.JaegerHost = _environment.EnvironmentName == "Local"
                    ? _configuration.GetValue<string>("Jaeger:AgentHost")
                    : _configuration.GetValue<string>(_configuration.GetValue<string>("Jaeger:AgentHost"));
                options.JaegerPort = _environment.EnvironmentName == "Local"
                    ? _configuration.GetValue<int>("Jaeger:AgentPort")
                    : _configuration.GetValue<int>(_configuration.GetValue<string>("Jaeger:AgentPort"));
            });
        }


        public static void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseHttpContextLogging(
                options => options.IgnoredPaths = new HashSet<string> { "/health" }
            );
            
            app.UseHealthChecks("/health");
        }

        
        private readonly IConfiguration _configuration;
        private IHostEnvironment _environment { get; }
    }
}