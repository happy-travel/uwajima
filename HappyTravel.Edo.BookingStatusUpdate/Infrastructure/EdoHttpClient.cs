using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Edo.BookingStatusUpdate.Infrastructure
{
    public class EdoHttpClient : IEdoHttpClient
    {
        public EdoHttpClient(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }


        public async Task<List<int>> GetBookings()
        {
            var response = await _client.GetAsync(_configuration.GetValue<string>("ListBookings"));
            if (!response.IsSuccessStatusCode)
                return new List<int>();

            var result = JsonSerializer.Deserialize<List<int>>(await response.Content.ReadAsStringAsync());
            return result ?? new List<int>();
        }

        public async Task UpdateBooking(int bookingId)
        {
            await _client.PostAsync($"{_configuration.GetValue<string>("UpdateStatus")}/{bookingId}", null!);
        }


        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
    }
}