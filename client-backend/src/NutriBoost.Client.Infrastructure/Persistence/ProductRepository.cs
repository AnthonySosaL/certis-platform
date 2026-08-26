using Microsoft.EntityFrameworkCore;
using NutriBoost.Client.Application.Products;
using NutriBoost.Client.Domain.Entities;

namespace NutriBoost.Client.Infrastructure.Persistence;

public class ProductRepository(NutriBoostDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);
}
