using DAL.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories
{
    public class CityRepo:Repo
    {
        public CityRepo() : base() { }
        public CityRepo(IMongoClient client, MarketContext marketContext) : base(client, marketContext) { }
        public List<State> GetListOfStates()
        {

                var ListOfState = new List<State>(_context.States.OrderBy(c => c.Name));
          
            return ListOfState;
       
        }
        public List<City> GetListOfCities()
        {
            return  new List<City>(_context.Cities.Where(c => c.StateId == 0).OrderBy(c => c.Name));
           
        }
        public List<City> GetListOfCitiesById(int id)
        {
            return new List<City>(_context.Cities.Where(c => c.StateId == id).OrderBy(c => c.Name).ToList());

        }
    }
}
