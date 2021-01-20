using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.BookingStatusUpdater.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.BookingStatusUpdater.Services
{
    public class StatusUpdateService : BackgroundService
    {
        public StatusUpdateService(IEdoHttpClient edoClient, IHostApplicationLifetime applicationLifetime)
        {
            _edoClient = edoClient;
            _applicationLifetime = applicationLifetime;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bookingList = await _edoClient.GetBookings();
            await _edoClient.UpdateBookings(bookingList);
            _applicationLifetime.StopApplication();
        }


        private readonly IEdoHttpClient _edoClient;
        private readonly IHostApplicationLifetime _applicationLifetime;
    }
}