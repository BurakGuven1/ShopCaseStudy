namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<OrderItem> Items { get; private set; } = new();

    public Order(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required", nameof(userId));
        UserId = userId;
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId is required", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("ProductName is required", nameof(productName));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

        Items.Add(new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        });

        Touch();
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Shipped orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;
        Touch();
    }

    public void MarkShipped()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cancelled orders cannot be shipped.");

        Status = OrderStatus.Shipped;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
