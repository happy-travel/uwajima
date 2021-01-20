using System;
using HappyTravel.Edo.BookingStatusUpdater.Services;
using HappyTravel.Edo.BookingStatusUpdater.Infrastructure;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddTransient<ProtectedApiBearerTokenHandler>();

            var jobsSettings = vaultClient.Get(_configuration["Identity:JobsOptions"]).GetAwaiter().GetResult();
            var clientSecret = jobsSettings[_configuration["Identity:Secret"]];

            var edoSettings = vaultClient.Get(_configuration["Edo:EdoOptions"]).GetAwaiter().GetResult();
            var authorityUrl = edoSettings[_configuration["Identity:Authority"]];
            var edoApiUrl = edoSettings[_configuration["Edo:Api"]];

            services.Configure<ClientCredentialsTokenRequest>(options =>
            {
                options.Address = $"{authorityUrl}connect/token";
                options.ClientId = _configuration["Identity:ClientId"];
                options.ClientSecret = clientSecret;
                options.Scope = "edo";
            });

            services.AddHttpClient<IIdentityServerClient, IdentityServerClient>(client =>
            {
                client.BaseAddress = new Uri(authorityUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<IEdoHttpClient, EdoHttpClient>(client =>
            {
                client.BaseAddress = new Uri(edoApiUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<ProtectedApiBearerTokenHandler>();
            
            services.AddHostedService<StatusUpdateService>();
            services.AddHealthChecks();
        }


        public static void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseHealthChecks("/health");
        }


        private readonly IConfiguration _configuration;
    }
}