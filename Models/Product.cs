namespace Stock_Server.Models;

public class Product
{
     public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        // Foreign Key
        public string SupplierId { get; set; } = string.Empty;

        // Navigation Property
        public Supplier Supplier { get; set; } = null!;
}
