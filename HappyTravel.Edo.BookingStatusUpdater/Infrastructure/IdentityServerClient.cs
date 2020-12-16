using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.BookingStatusUpdater.Infrastructure
{
    public class IdentityServerClient : IIdentityServerClient
    {
        public IdentityServerClient(HttpClient httpClient,
            IOptions<ClientCredentialsTokenRequest> tokenRequest,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _tokenRequest = tokenRequest.Value;
            _cache = cache;
        }

        public async Task<string> RequestClientCredentialsTokenAsync()
        {
            if (!_cache.TryGetValue<string>("accessToken", out var accessToken))
            {
                var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(_tokenRequest);
                if (tokenResponse.IsError)
                    throw new HttpRequestException("Something went wrong while requesting the access token");

                _cache.Set("accessToken", accessToken, DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn));
                accessToken = tokenResponse.AccessToken;
            }

            return accessToken;
        }


        private readonly HttpClient _httpClient;
        private readonly ClientCredentialsTokenRequest _tokenRequest;
        private readonly IMemoryCache _cache;
    }
}