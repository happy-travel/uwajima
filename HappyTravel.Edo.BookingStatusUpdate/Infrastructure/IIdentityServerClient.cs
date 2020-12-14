using System.Threading.Tasks;

namespace HappyTravel.Edo.BookingStatusUpdate.Infrastructure
{
    public interface IIdentityServerClient
    {
        Task<string> RequestClientCredentialsTokenAsync();
    }
}