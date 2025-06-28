using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Interface;

namespace WeatherApp.Service.Implementation
{
    public class WeatherAlertService : IWeatherAlertService
    {
        private readonly IRepository<WeatherAlert> _repository;
        private readonly IRepository<UserSettings> _userSettingsRepository;
        private readonly IWeatherApiService _weatherApiService;
        private readonly IOutfitRecommendationService _outfitRecommendationService;

        public WeatherAlertService(
            IRepository<WeatherAlert> repository,
            IWeatherApiService weatherApiService,
            IOutfitRecommendationService outfitRecommendationService)
        {
            _repository = repository;
            _weatherApiService = weatherApiService;
            _outfitRecommendationService = outfitRecommendationService;
        }
        public WeatherAlert DeleteById(int id)
        {
            var alert = GetById(id);
            if (alert == null) throw new ArgumentException("WeatherAlert not found");
            return _repository.Delete(alert);
        }

        public List<WeatherAlert> GetAll()
        {
            return _repository.GetAll(
                        selector: x => x,
                        include: x => x.Include(a => a.UserSettings)
                                      .ThenInclude(us => us.Location)
                    ).ToList();
        }

        public WeatherAlert? GetById(int id)
        {
            return _repository.Get(x => x, predicate: x => x.Id == id);
        }

        public WeatherAlert Insert(WeatherAlert alert)
        {
            if (alert.UserSettings == null || alert.AlertDate == default)
            {
                alert.AlertType = "✅ Normal Conditions - No severe weather expected";
                return _repository.Insert(alert);
            }

            try
            {
                var userSettings = _userSettingsRepository.Get(x => x, u => u.Id == alert.UserSettingsId);
                if (userSettings == null)
                {
                    alert.AlertType = "Invalid user settings";
                    return _repository.Insert(alert);
                }
                alert.UserSettings = userSettings;

                var weatherData = GetWeatherDataForAlert(alert).Result;

                if (weatherData == null)
                {
                    alert.AlertType = "No weather data available for selected date";
                    return _repository.Insert(alert);
                }

                alert.AlertType = GenerateWeatherAlert(weatherData);
                alert.RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(weatherData);
                Debug.WriteLine($"[DEBUG] ALERT DATE: {alert.AlertDate}");
                Debug.WriteLine($"[DEBUG] WEATHER DATA: {weatherData?.Date} - {weatherData?.TemperatureCelsius}°C - {weatherData?.Conditions}");

                return _repository.Insert(alert);
            }
            catch (Exception ex)
            {
                alert.AlertType = "Error generating weather alert";
                return _repository.Insert(alert);
            }
        }

        private async Task<WeatherData?> GetWeatherDataForAlert(WeatherAlert alert)
        {
            var forecast = await _weatherApiService.Get7DayForecastAsync(alert.UserSettings.LocationId);
            Debug.WriteLine($"[DEBUG] Looking for weather data for: {alert.AlertDate.Date}");
            foreach (var day in forecast)
            {
                Debug.WriteLine($"[DEBUG] Available forecast: {day.Date.Date} → {day.TemperatureCelsius}°C");
            }

            return forecast.FirstOrDefault(w => w.Date.Date == alert.AlertDate.Date);
        }

        private async Task<WeatherData?> GetWeatherDataForUserSettings(UserSettings userSettings)
        {

            if (userSettings.LocationId != 0)
            {
                return await _weatherApiService.GetCurrentWeatherAsync(userSettings.LocationId);
            }
            return null;
        }

        public WeatherAlert Update(WeatherAlert alert)
        {
            return _repository.Update(alert);
        }



        public string GenerateWeatherAlert(WeatherData weatherData)
        {
            var conditions = weatherData.Conditions?.ToLower() ?? "";
            double temp = weatherData.TemperatureCelsius;

            if (weatherData.TemperatureCelsius < -5)
                return "❄️ Extreme Cold Warning - Frostbite risk!";
            if (weatherData.TemperatureCelsius < 0)
                return "⚠️ Freezing Temperatures - Dress warmly!";
            if (weatherData.Conditions.Contains("Rain"))
                return "🌧️ Rain Alert - Expect wet conditions";
            if (weatherData.Conditions.Contains("Snow"))
                return "⛄ Snow Alert - Icy conditions possible";
            if (weatherData.TemperatureCelsius > 30)
                return "🔥 Heat Warning - Stay cool and hydrated";
            if (weatherData.Conditions.Contains("Thunderstorm"))
                return "⚡ Thunderstorm Alert - Seek shelter if outdoors";

            return "✅ Normal Conditions - No severe weather expected";
        }

        public WeatherAlert GetByIdWithUserSettings(int id)
        {
            var allAlerts = _repository.GetAll(
                selector: x => x,
                include: x => x.Include(a => a.UserSettings)
                              .ThenInclude(us => us.Location)
            );

            return allAlerts.FirstOrDefault(a => a.Id == id);
        }

        public void CleanInvalidAlerts()
        {
            var invalidAlerts = GetAll()
                .Where(a => string.IsNullOrEmpty(a.AlertType) || string.IsNullOrEmpty(a.RecommendationText))
                .ToList();

            foreach (var alert in invalidAlerts)
            {
                DeleteById(alert.Id);
            }
        }


    }
}
