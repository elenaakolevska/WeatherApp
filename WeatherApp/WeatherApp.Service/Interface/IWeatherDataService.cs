using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Service.Interface
{
    public interface IWeatherDataService
    {
        List<WeatherData> GetAll();
        WeatherData? GetById(int id);
        WeatherData Insert(WeatherData data);
        WeatherData Update(WeatherData data);
        WeatherData DeleteById(int id);
    }
}
