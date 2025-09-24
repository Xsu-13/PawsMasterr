namespace Backend.Models
{
    public class YdbSettings
    {
        public string Endpoint { get; set; } = null!;
        public string Database { get; set; } = null!;
        public string AuthToken { get; set; } = null!;
    }
}
