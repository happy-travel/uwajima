using System;
using HappyTravel.Uwajima.Services.HttpClients;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Uwajima.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            var jobsSettings = vaultClient.Get(configuration["Identity:JobsOptions"]).GetAwaiter().GetResult();
            var edoSettings = vaultClient.Get(configuration["Edo:EdoOptions"]).GetAwaiter().GetResult();

            var identityUrl = edoSettings[configuration["Identity:Authority"]];
            var edoUrl = edoSettings[configuration["Edo:Api"]];
            var clientId = configuration["Identity:ClientId"];
            var clientSecret = jobsSettings[configuration["Identity:Secret"]];
            
            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add(HttpClientNames.Identity,
                    new ClientCredentialsTokenRequest
                    {
                        Address = identityUrl + "connect/token",
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    });
            });
            services.AddClientAccessTokenClient(HttpClientNames.EdoApi, HttpClientNames.Identity,
                client => { client.BaseAddress = new Uri(edoUrl); });

            services.AddTransient<IEdoHttpClient, EdoHttpClient>();
        }
    }
}