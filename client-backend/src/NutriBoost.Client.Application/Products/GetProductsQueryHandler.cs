using NutriBoost.Client.Domain.Entities;

namespace NutriBoost.Client.Application.Products;

// CQRS-lite: a query handler as a plain class, no mediator library yet.
// Upgrade to a mediator (e.g. MediatR) only once there are enough
// commands/queries that manual DI wiring becomes the actual bottleneck.
public class GetProductsQueryHandler(IProductRepository repository)
{
    public Task<IReadOnlyList<Product>> HandleAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}
