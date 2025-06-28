using System;
using System.Collections.Generic;
using System.Linq;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Interface;

namespace WeatherApp.Service.Implementation
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IRepository<UserSettings> _repository;

        public UserSettingsService(IRepository<UserSettings> repository)
        {
            _repository = repository;
        }

        public UserSettings DeleteById(int id)
        {
            var settings = GetById(id);
            if (settings == null) throw new ArgumentException("UserSettings not found");
            return _repository.Delete(settings);
        }

        public List<UserSettings> GetAll()
        {
            return _repository.GetAll(x => x).ToList();
        }

        public UserSettings? GetById(int id)
        {
            return _repository.Get(x => x, predicate: x => x.Id == id);
        }


        public List<UserSettings> GetByUserId(string userId)
        {

            return _repository.GetAll(
                selector: x => x,           
                predicate: x => x.UserId == userId  
            ).ToList();
        }




        public UserSettings Insert(UserSettings userSettings)
        {
            return _repository.Insert(userSettings);
        }

        public UserSettings Update(UserSettings userSettings)
        {
            return _repository.Update(userSettings);
        }
    }
}
