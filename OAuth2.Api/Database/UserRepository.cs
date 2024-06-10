using MongoDB.Driver;
using OAuth2.Api.Models;

namespace OAuth2.Api.Database
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017"); // MongoDB bağlantı stringini burada değiştirin
            var database = client.GetDatabase("oAuth2Test"); // Veritabanı adını burada değiştirin
            _users = database.GetCollection<User>("users"); // Koleksiyon adını burada değiştirin
        }

        public async Task<User> GetUserByUserNameAsync(string userName)
        {
            return await _users.Find(user => user.UserName == userName).FirstOrDefaultAsync();
        }
    }
}
