using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Interface;
using System.Net.Http;
using System.Diagnostics;

namespace WeatherApp.Service.Implementation
{
    public class WeatherApiService : IWeatherApiService
    {
        private readonly IRepository<WeatherData> _weatherDataRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "d212a917cea5c5b04e71bc16bc228e99";

        public WeatherApiService(IRepository<WeatherData> weatherDataRepository, IRepository<Location> locationRepository)
        {
            _weatherDataRepository = weatherDataRepository;
            _locationRepository = locationRepository;
            _httpClient = new HttpClient();
        }
        public async Task<IEnumerable<WeatherData>> Get7DayForecastAsync(int locationId)
        {
            var location = _locationRepository.Get(x => x, x => x.Id == locationId);
            if (location == null)
                return Enumerable.Empty<WeatherData>();

            var latitude = location.Latitude;
            var longitude = location.Longitude;

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=Europe%2FSkopje";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API error: {response.StatusCode} - {error}");
                return Enumerable.Empty<WeatherData>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var daily = data["daily"];

            if (daily == null)
            {
                Debug.WriteLine("No 'daily' data in response.");
                return Enumerable.Empty<WeatherData>();
            }

            var dates = daily["time"]?.ToObject<List<string>>() ?? new();
            var tempMax = daily["temperature_2m_max"]?.ToObject<List<double>>() ?? new();
            var tempMin = daily["temperature_2m_min"]?.ToObject<List<double>>() ?? new();
            var precipitation = daily["precipitation_sum"]?.ToObject<List<double>>() ?? new();

            var existingData = _weatherDataRepository.GetAll(
                selector: x => x,
                predicate: x => x.LocationId == locationId
            ).ToList();

            var result = new List<WeatherData>();
            var toInsert = new List<WeatherData>();

            for (int i = 0; i < dates.Count; i++)
            {
                var date = DateTime.Parse(dates[i]).Date;

                var weatherData = new WeatherData
                {
                    Date = date,
                    TemperatureCelsius = (tempMax[i] + tempMin[i]) / 2.0, 
                    Humidity = precipitation[i], 
                    Conditions = precipitation[i] > 0 ? "Rain" : "Clear", 
                    LocationId = locationId
                };

                var alreadyExists = existingData.Any(x => x.Date == date);
                if (!alreadyExists)
                {
                    toInsert.Add(weatherData);
                }

                result.Add(weatherData);
            }

            if (toInsert.Any())
            {
                foreach (var wd in toInsert)
                {
                    _weatherDataRepository.Insert(wd);
                }

                await _weatherDataRepository.SaveChangesAsync();
            }

            return result;
        }


        public async Task<WeatherData?> GetCurrentWeatherAsync(int locationId)
        {
            var location = _locationRepository.Get(x => x, x => x.Id == locationId);
            if (location == null) return null;

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={location.Latitude}&lon={location.Longitude}&units=metric&appid={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var weatherData = new WeatherData
            {
                Date = DateTime.UtcNow.Date,
                TemperatureCelsius = data["main"]?["temp"]?.Value<double>() ?? 0,
                Humidity = data["main"]?["humidity"]?.Value<double>() ?? 0,
                Conditions = data["weather"]?[0]?["main"]?.Value<string>() ?? "Unknown",
                LocationId = locationId,
                Location = location
            };

            var existing = _weatherDataRepository.Get(x => x,
                x => x.Date == weatherData.Date && x.LocationId == weatherData.LocationId);

            if (existing == null)
            {
                _weatherDataRepository.Insert(weatherData);
                await _weatherDataRepository.SaveChangesAsync();
                return weatherData;
            }
            else
            {
                return existing;
            }
        }
        public async Task<WeatherData> GetWeatherForDateAsync(int locationId, DateTime date)
        {
            var existingData = _weatherDataRepository.Get(
                x => x,
                x => x.LocationId == locationId && x.Date == date.Date
            );

            if (existingData != null)
            {
                return existingData;
            }

            var location = _locationRepository.Get(x => x, x => x.Id == locationId);
            if (location == null) return null;

            try
            {
                var url = $"https://api.open-meteo.com/v1/forecast?" +
                         $"latitude={location.Latitude}&" +
                         $"longitude={location.Longitude}&" +
                         $"start_date={date:yyyy-MM-dd}&" +
                         $"end_date={date:yyyy-MM-dd}&" +
                         "daily=temperature_2m_max,temperature_2m_min,precipitation_sum&" +
                         "timezone=Europe%2FSkopje";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(json);
                    var daily = data["daily"];

                    if (daily != null)
                    {
                        var dates = daily["time"]?.ToObject<List<string>>() ?? new();
                        var tempMax = daily["temperature_2m_max"]?.ToObject<List<double>>() ?? new();
                        var tempMin = daily["temperature_2m_min"]?.ToObject<List<double>>() ?? new();
                        var precipitation = daily["precipitation_sum"]?.ToObject<List<double>>() ?? new();

                        if (dates.Count > 0 && tempMax.Count > 0 && tempMin.Count > 0)
                        {
                            var weatherData = new WeatherData
                            {
                                Date = date.Date,
                                TemperatureCelsius = (tempMax[0] + tempMin[0]) / 2.0,
                                Humidity = precipitation.Count > 0 ? precipitation[0] : 0,
                                Conditions = precipitation.Count > 0 && precipitation[0] > 0 ? "Rain" : "Clear",
                                LocationId = locationId,
                                Location = location
                            };

                            _weatherDataRepository.Insert(weatherData);
                            await _weatherDataRepository.SaveChangesAsync();

                            return weatherData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Open-Meteo API error: {ex.Message}");
            }

            if (date.Date == DateTime.Today)
            {
                try
                {
                    var url = $"https://api.openweathermap.org/data/2.5/weather?" +
                             $"lat={location.Latitude}&" +
                             $"lon={location.Longitude}&" +
                             "units=metric&" +
                             $"appid={_apiKey}";

                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(json);

                        var weatherData = new WeatherData
                        {
                            Date = date.Date,
                            TemperatureCelsius = data["main"]?["temp"]?.Value<double>() ?? 0,
                            Humidity = data["main"]?["humidity"]?.Value<double>() ?? 0,
                            Conditions = data["weather"]?[0]?["main"]?.Value<string>() ?? "Unknown",
                            LocationId = locationId,
                            Location = location
                        };

                        _weatherDataRepository.Insert(weatherData);
                        await _weatherDataRepository.SaveChangesAsync();

                        return weatherData;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"OpenWeatherMap API error: {ex.Message}");
                }
            }

            return new WeatherData
            {
                Date = date.Date,
                TemperatureCelsius = 15, 
                Humidity = 50, 
                Conditions = "Clear",
                LocationId = locationId,
                Location = location
            };
        }
    }
}
