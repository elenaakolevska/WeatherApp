using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Interface;

namespace WeatherApp.Service.Implementation
{
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> _repository;

        public LocationService(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public Location DeleteById(int id)
        {
            var location = GetById(id);
            if (location == null) throw new ArgumentException("Location not found");
            return _repository.Delete(location);
        }

        public List<Location> GetAll()
        {
            return _repository.GetAll(x => x).ToList();
        }

        public Location? GetById(int id)
        {
            return _repository.Get(x => x, predicate: x => x.Id == id);
        }

        public Location Insert(Location location)
        {
            return _repository.Insert(location);
        }

        public Location Update(Location location)
        {
            return _repository.Update(location);
        }
    }
}
