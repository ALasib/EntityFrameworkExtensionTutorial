namespace EntityFrameworkExtensionTutorial.Domain.Entities;

public class OrderItem : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Foreign key
    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
}
