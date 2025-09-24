using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Recipe
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("ingredients")]
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        
        [JsonPropertyName("servings")]
        public int Servings { get; set; }
        
        [JsonPropertyName("prepTime")]
        public string? PrepTime { get; set; }
        
        [JsonPropertyName("cookTime")]
        public string? CookTime { get; set; }
        
        [JsonPropertyName("steps")]
        public List<string> Steps { get; set; } = new List<string>();
        
        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
