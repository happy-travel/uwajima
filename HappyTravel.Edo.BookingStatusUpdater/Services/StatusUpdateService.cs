using System;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.BookingStatusUpdater.Infrastructure;
using HappyTravel.Edo.BookingStatusUpdater.Services.HttpClients;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.BookingStatusUpdater.Services
{
    public class StatusUpdateService : BackgroundService
    {
        public StatusUpdateService(IEdoHttpClient edoClient, IHostApplicationLifetime applicationLifetime, ILogger<StatusUpdateService> logger)
        {
            _edoClient = edoClient;
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var bookingList = await _edoClient.GetBookings();
                await _edoClient.UpdateBookings(bookingList);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error occurred: '{e.Message}'");
            }
            
            _applicationLifetime.StopApplication();
        }


        private readonly IEdoHttpClient _edoClient;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<StatusUpdateService> _logger;
    }
}