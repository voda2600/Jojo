using DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public abstract class Repo
    {
        protected IMongoClient _client;
        protected MarketContext _context;

        protected Repo(IMongoClient client, MarketContext context)
        {
            _client = client;
            _context = context;
        }

        protected Repo()
        {
            _client = Services.CollectionServices.GetService<IMongoClient>();
            _context = (MarketContext) Services.CollectionServices.GetService(typeof(MarketContext));
        }
        
        
    }
}