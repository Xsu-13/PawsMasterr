

namespace Backend.Models
{
    public class RecipeDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public int Servings { get; set; }
        public string? Prep_time { get; set; }
        public string? Cook_time { get; set; }
        public List<string> Steps { get; set; }
    }
}
