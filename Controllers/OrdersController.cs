using Microsoft.AspNetCore.Mvc;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderAnalysisService _orderAnalysisService;

    public OrdersController(OrderAnalysisService orderAnalysisService)
    {
        _orderAnalysisService = orderAnalysisService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Order newOrder)
    {
        // Force Id to null so MongoDB auto-generates a new ObjectId instead of trying to parse a client-provided dummy value (like Swagger's "string")
        newOrder.Id = null;
        await _orderAnalysisService.CreateOrderAsync(newOrder);

        return CreatedAtAction(nameof(Post), new { id = newOrder.Id }, newOrder);
    }
}
