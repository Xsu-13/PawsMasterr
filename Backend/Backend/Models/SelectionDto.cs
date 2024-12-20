using MongoDB.Bson;

namespace Backend.Models
{
    public class SelectionDto
    {
        public string id { get; set; }
        public string? title { get; set; }
        public List<string> recipes { get; set; } = new List<string>();
    }
}
