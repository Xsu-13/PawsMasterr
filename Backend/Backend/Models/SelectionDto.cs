using System;

namespace Backend.Models
{
    public class SelectionDto
    {
        public string id { get; set; }
        public string? title { get; set; }
        public List<Guid> recipes { get; set; } = new List<Guid>();
    }
}
