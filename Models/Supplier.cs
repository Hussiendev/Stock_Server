namespace Stock_Server.Models;
 public class Supplier
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // Navigation Property (One Supplier → Many Products)
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }