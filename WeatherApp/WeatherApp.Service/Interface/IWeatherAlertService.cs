using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Service.Interface
{
    public interface IWeatherAlertService
    {
        List<WeatherAlert> GetAll();
        WeatherAlert? GetById(int id);
        WeatherAlert Insert(WeatherAlert alert);
        WeatherAlert Update(WeatherAlert alert);
        WeatherAlert DeleteById(int id);
        string GenerateWeatherAlert(WeatherData weatherData);
        WeatherAlert GetByIdWithUserSettings(int id);

    }
}
