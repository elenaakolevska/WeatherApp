using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Service.Interface
{
    public interface ILocationService
    {
        List<Location> GetAll();
        Location? GetById(int id);
        Location Insert(Location location);
        Location Update(Location location);
        Location DeleteById(int id);
    }
}
