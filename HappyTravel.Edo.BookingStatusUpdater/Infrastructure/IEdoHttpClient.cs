using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.BookingStatusUpdater.Infrastructure
{
    public interface IEdoHttpClient
    {
        Task<List<int>> GetBookings();
        Task UpdateBooking(int bookingId);
    }
}