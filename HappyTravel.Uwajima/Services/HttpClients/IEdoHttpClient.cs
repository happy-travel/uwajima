using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Uwajima.Services.HttpClients
{
    public interface IEdoHttpClient
    {
        Task<List<int>> GetBookings();
        Task UpdateBookings(IEnumerable<int> bookingIds);
    }
}