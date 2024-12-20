namespace Backend.Models
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string RecipesCollection { get; set; } = null!;
        public string UsersCollection { get; set; } = null!;
        public string SelectionCollection { get; set; } = null!;
    }
}
