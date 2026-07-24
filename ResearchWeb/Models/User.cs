namespace ResearchWeb.Models
{
    public class User
    {
        public int ID { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? FullName { get; set; }

        public string? Role { get; set; }
    }
}