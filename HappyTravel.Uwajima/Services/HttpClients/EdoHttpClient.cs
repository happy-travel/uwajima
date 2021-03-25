using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Uwajima.Services.HttpClients
{
    public class EdoHttpClient : IEdoHttpClient
    {
        public EdoHttpClient(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _httpClient = clientFactory.CreateClient(HttpClientNames.EdoApi);
            _configuration = configuration;
        }


        public async Task<List<int>> GetBookings()
        {
            var response = await _httpClient.GetAsync(_configuration.GetValue<string>("ListBookings"));
            if (!response.IsSuccessStatusCode)
                return new List<int>();

            var result = JsonSerializer.Deserialize<List<int>>(await response.Content.ReadAsStringAsync());
            return result ?? new List<int>();
        }

        
        public async Task UpdateBookings(IEnumerable<int> bookingIds)
        {
            var content = new StringContent(JsonSerializer.Serialize(bookingIds), Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"{_configuration.GetValue<string>("UpdateStatus")}", content);
        }
        

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
    }
}