using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Selection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("recipes")]
        public List<string> Recipes { get; set; } = new List<string>();
    }
}
