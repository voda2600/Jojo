using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DAL
{
    public static class Services
    {
        public static ServiceProvider CollectionServices = ServiceProvider();
        private static ServiceProvider ServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMongoClient>(provider =>
                new MongoClient("mongodb+srv://mongo:mongo@b3ielectronics.ykxpf.mongodb.net/myFirstDatabase?retryWrites=true&w=majority"));
            services.AddDbContext<MarketContext>(options =>
                options.UseNpgsql("Host=ec2-52-19-164-214.eu-west-1.compute.amazonaws.com;Port=5432;Database=dd4r6kpbqg47jk;Username=gfdbznyfxbcdes;Password=5a0f3f35273184ecd8fd51874c82db8e5cd3435074e26cdcd9c0cc129edc78e3;SslMode=Require;Trust Server Certificate=true"));
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}