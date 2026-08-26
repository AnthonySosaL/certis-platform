namespace NutriBoost.Client.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public decimal PriceUsd { get; private set; }
    public int StockQuantity { get; private set; }

    private Product() { }

    public Product(string name, string slug, decimal priceUsd, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (priceUsd < 0)
            throw new ArgumentOutOfRangeException(nameof(priceUsd), "Price cannot be negative.");
        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock cannot be negative.");

        Id = Guid.NewGuid();
        Name = name;
        Slug = slug;
        PriceUsd = priceUsd;
        StockQuantity = stockQuantity;
    }
}
