using Microsoft.AspNetCore.Mvc;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderAnalysisService orderAnalysisService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(Order newOrder)
    {
        newOrder.Id = null;
        await orderAnalysisService.CreateOrderAsync(newOrder);

        return CreatedAtAction(nameof(Post), new { id = newOrder.Id }, newOrder);
    }
}
