using NutriBoost.Client.Domain.Entities;

namespace NutriBoost.Client.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var product = new Product("Whey Protein 2lb", "whey-protein-2lb", 29.99m, 50);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Whey Protein 2lb", product.Name);
        Assert.Equal(50, product.StockQuantity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new Product(name, "slug", 10m, 1));
    }

    [Fact]
    public void Constructor_WithNegativePrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("Name", "slug", -1m, 1));
    }

    [Fact]
    public void Constructor_WithNegativeStock_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("Name", "slug", 10m, -1));
    }
}
