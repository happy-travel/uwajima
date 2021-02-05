using System;
using HappyTravel.Edo.BookingStatusUpdater.Services;
using HappyTravel.Edo.BookingStatusUpdater.Infrastructure;
using HappyTravel.Edo.BookingStatusUpdater.Infrastructure.Extensions;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace HappyTravel.Edo.BookingStatusUpdater
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
        }


        public static void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseHealthChecks("/health");
        }

        
        private readonly IConfiguration _configuration;
    }
}