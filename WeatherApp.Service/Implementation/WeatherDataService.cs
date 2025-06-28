using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Interface;

namespace WeatherApp.Service.Implementation
{
    public class WeatherDataService : IWeatherDataService
    {
        private readonly IRepository<WeatherData> _repository;

        public WeatherDataService(IRepository<WeatherData> repository)
        {
            _repository = repository;
        }
        public WeatherData DeleteById(int id)
        {
            var data = GetById(id);
            if (data == null) throw new ArgumentException("WeatherData not found");
            return _repository.Delete(data);
        }

        public List<WeatherData> GetAll()
        {
            return _repository.GetAll(x => x).ToList();
        }

        public WeatherData? GetById(int id)
        {
            return _repository.Get(selector: x => x,
                 predicate: x => x.Id == id,
                 include: x => x.Include(y => y.Location));
        }


        public WeatherData Insert(WeatherData data)
        {
            return _repository.Insert(data);
        }

        public WeatherData Update(WeatherData data)
        {
            return _repository.Update(data);
        }
    }
}
