namespace Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Update(string name, decimal price)
    {
        Name = name;
        Price = price;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
