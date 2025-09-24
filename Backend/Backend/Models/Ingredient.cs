using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Ingredient
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        
        [JsonPropertyName("quantity")]
        public string Quantity { get; set; } = null!;
    }
}
