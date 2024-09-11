using MongoDB.Bson;

namespace Backend.Models
{
    public class Recipe
    {
        public ObjectId id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public List<Ingredient> ingredients { get; set; }
        public int servings { get; set; }
        public string? prep_time { get; set; }
        public string? cook_time { get; set; }
        public List<string> steps { get; set; }
    }
}
