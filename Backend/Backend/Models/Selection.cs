using MongoDB.Bson;

namespace Backend.Models
{
    public class Selection
    {
        public ObjectId id { get; set; }
        public string? title { get; set; }
        public List<ObjectId> recipes { get; set; } = new List<ObjectId>();
    }
}
