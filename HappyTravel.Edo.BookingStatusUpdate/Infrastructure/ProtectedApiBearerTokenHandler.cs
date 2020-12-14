using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace HappyTravel.Edo.BookingStatusUpdate.Infrastructure
{
    public class ProtectedApiBearerTokenHandler : DelegatingHandler
    {
        public ProtectedApiBearerTokenHandler(IIdentityServerClient identityServerClient)
        {
            _identityServerClient = identityServerClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await _identityServerClient.RequestClientCredentialsTokenAsync();
            request.SetBearerToken(accessToken);

            return await base.SendAsync(request, cancellationToken);
        }


        private readonly IIdentityServerClient _identityServerClient;
    }
}