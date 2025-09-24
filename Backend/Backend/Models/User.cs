using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class User
    {
        public List<string> AddedRecipes { get; set; } = new();
        public List<string> FavoriteRecipes { get; set; } = new();
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User() {}

        public User(string name, string email, string password)
        {
            Username = name;
            Email = email;
            PasswordHash = password;
        }
    }
}
