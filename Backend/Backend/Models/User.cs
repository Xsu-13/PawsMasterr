using MongoDB.Bson;

namespace Backend.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string Email {  get; set; }  
        public string PasswordHash { get; set; }

        public List<ObjectId> FavoriteRecipes { get; set; } = new List<ObjectId>();
        public string? ImageUrl { get; set; }

        public User(string name, string email, string password)
        {
            UserName = name;
            Email = email;
            PasswordHash = password;
        }
    }
}
