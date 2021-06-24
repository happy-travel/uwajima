using System;
using System.Collections.Generic;
using HappyTravel.Uwajima.Services;
using HappyTravel.Uwajima.Infrastructure;
using HappyTravel.Uwajima.Infrastructure.Extensions;
using HappyTravel.StdOutLogger.Extensions;
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
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
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
            services.AddTracing();
        }


        public static void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseHttpContextLogging(
                options => options.IgnoredPaths = new HashSet<string> { "/health" }
            );
            
            app.UseHealthChecks("/health");
        }

        
        private readonly IConfiguration _configuration;
    }
}