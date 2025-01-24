using ProcessWeatherImageJob.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProcessWeatherImageJob.Services
{
    public class WeatherStationService
    {
        private const string BuienradarApiUrl = "https://api.buienradar.nl/data/public/2.0/jsonfeed";
        private readonly HttpClient _httpClient;

        public WeatherStationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WeatherStation>> GetWeatherStations()
        {
            var response = await _httpClient.GetAsync(BuienradarApiUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var weatherStations = new List<WeatherStation>();

            foreach (var station in root.GetProperty("actual").GetProperty("stationmeasurements").EnumerateArray())
            {
                var weatherStation = new WeatherStation
                {
                    StationId = station.GetProperty("stationid").GetInt32(),
                    StationName = station.GetProperty("stationname").GetString(),
                    Region = station.GetProperty("regio").GetString(),
                    Temperature = station.GetProperty("temperature").GetDouble(),
                    WindSpeed = station.GetProperty("windspeed").GetDouble(),
                    WindDirectionDegrees = station.GetProperty("winddirection").GetDouble()
                };
                weatherStations.Add(weatherStation);
            }

            return weatherStations;
        }
    }
}
