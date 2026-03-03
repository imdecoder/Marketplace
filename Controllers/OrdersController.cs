using Microsoft.AspNetCore.Mvc;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderAnalysisService orderAnalysisService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await orderAnalysisService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var order = await orderAnalysisService.GetOrderByIdAsync(id);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Order newOrder)
    {
        newOrder.Id = null;
        await orderAnalysisService.CreateOrderAsync(newOrder);
        return CreatedAtAction(nameof(GetById), new { id = newOrder.Id }, newOrder);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Order updatedOrder)
    {
        var existing = await orderAnalysisService.GetOrderByIdAsync(id);
        if (existing is null) return NotFound();

        updatedOrder.Id = id;
        await orderAnalysisService.UpdateOrderAsync(id, updatedOrder);
        return Ok(updatedOrder);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await orderAnalysisService.GetOrderByIdAsync(id);
        if (existing is null) return NotFound();

        await orderAnalysisService.DeleteOrderAsync(id);
        return NoContent();
    }
}
