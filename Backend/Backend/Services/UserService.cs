using AutoMapper;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Xml.Linq;

namespace Backend.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtProvider _jwtProvider;
        private readonly IMapper _mapper;

        public UserService(
            IOptions<MongoDBSettings> settings,
            PasswordHasher passwordHasher,
            JwtProvider jwtProvider,
            IMapper mapper)
        {
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;

            MongoClient client = new MongoClient(settings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>(settings.Value.UsersCollection);
            _mapper = mapper;
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _users.Find(user => user.Email == email).FirstOrDefaultAsync() ?? throw new Exception("Incorrect password or login, check it out");

            var result = _passwordHasher.Verify(password, user?.PasswordHash ?? "");

            if (user == null || !result)
            {
                throw new Exception("Incorrect password or login, check it out");
            }

            var token = _jwtProvider.GenerateToken(user!);

            return token;
        }

        public async Task SignUp(string username, string email, string password)
        {
            var hashedPassword = _passwordHasher.Generate(password);
            var user = new User(username, email, hashedPassword);
            user.Id = ObjectId.GenerateNewId().ToString();
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateRecipeImageAsync(string userId, string imageUrl)
        {
            var update = Builders<User>.Update.Set(r => r.ImageUrl, imageUrl);
            await _users.UpdateOneAsync(r => r.Id == userId, update);
        }
    }
}
