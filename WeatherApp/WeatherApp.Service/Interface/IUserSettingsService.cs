using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Service.Interface
{
    public interface IUserSettingsService
    {
        List<UserSettings> GetAll();
        UserSettings? GetById(int id);
        List<UserSettings> GetByUserId(string userId);
        UserSettings Insert(UserSettings userSettings);
        UserSettings Update(UserSettings userSettings);
        UserSettings DeleteById(int id);
    }
}
