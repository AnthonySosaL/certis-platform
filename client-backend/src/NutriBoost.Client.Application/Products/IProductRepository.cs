using NutriBoost.Client.Domain.Entities;

namespace NutriBoost.Client.Application.Products;

// The Application layer only knows this interface — Infrastructure
// provides the EF Core implementation. Keeps business logic testable
// without a real database.
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
}
