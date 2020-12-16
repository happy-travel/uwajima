using System.Threading.Tasks;

namespace HappyTravel.Edo.BookingStatusUpdater.Infrastructure
{
    public interface IIdentityServerClient
    {
        Task<string> RequestClientCredentialsTokenAsync();
    }
}