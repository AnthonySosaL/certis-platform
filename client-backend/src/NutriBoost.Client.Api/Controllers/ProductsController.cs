using Microsoft.AspNetCore.Mvc;
using NutriBoost.Client.Application.Products;

namespace NutriBoost.Client.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(GetProductsQueryHandler getProducts) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await getProducts.HandleAsync(cancellationToken);
        return Ok(products);
    }
}
