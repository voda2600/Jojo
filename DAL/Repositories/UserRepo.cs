using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class UserRepo:Repo
    {
        public UserRepo():base(){}
        public UserRepo(MongoClient mongoClient, MarketContext marketContext):base(mongoClient, marketContext) { }
        
        public User AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User GetUserByPk(string name)
        {
            return _context.Users.Find(name);
        }

        public void EditUserMail(string name, string newEmail)
        {
            var user = GetUserByPk(name);
            user.Email = newEmail;
            _context.SaveChanges();
        }

        public async Task EditUserPassword(string name, string newPassword)
        {
            var user = GetUserByPk(name);
            user.Password = newPassword;
            await _context.SaveChangesAsync();
        }

        public User GetUserBy(int orderId)
        {
            var username =  _context.Orders.Find(orderId).Username;
            return GetUserByPk(username);
        }
    }
}