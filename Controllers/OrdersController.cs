using Microsoft.AspNetCore.Mvc;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderAnalysisService orderAnalysisService) : ControllerBase
{
    /// <summary>
    /// Sistemde kayıtlı olan tüm siparişleri listeler.
    /// </summary>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/Orders
    ///
    /// </remarks>
    /// <returns>Tüm siparişlerin bir listesini döndürür.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await orderAnalysisService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip siparişin detaylarını getirir.
    /// </summary>
    /// <param name="id">Siparişin benzersiz kimliği (ID).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/Orders/65b3...
    ///
    /// </remarks>
    /// <returns>Bulunan sipariş nesnesini döndürür, bulunamazsa NotFound (404) döner.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var order = await orderAnalysisService.GetOrderByIdAsync(id);
        if (order is null) return NotFound();
        return Ok(order);
    }

    /// <summary>
    /// Yeni bir sipariş oluşturur.
    /// </summary>
    /// <param name="dto">Oluşturulacak siparişe ait platform, tarih ve ürün bilgileri.</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     POST /api/Orders
    ///     {
    ///        "platformId": "60a7b1...",
    ///        "date": "2024-02-01T12:00:00Z",
    ///        "items": [
    ///           {
    ///             "name": "Örnek Ürün",
    ///             "purchasePrice": 100.0,
    ///             "salePrice": 150.0,
    ///             "commissionRate": 0.15,
    ///             "shippingCost": 10.0,
    ///             "quantity": 2
    ///           }
    ///        ]
    ///     }
    ///
    /// </remarks>
    /// <returns>Oluşturulan siparişin erişim URL'sini ve bilgilerini döndürür.</returns>
    [HttpPost]
    public async Task<IActionResult> Post(OrderCreateDto dto)
    {
        var newOrder = new Order { PlatformId = dto.PlatformId, Date = dto.Date, Items = dto.Items };
        await orderAnalysisService.CreateOrderAsync(newOrder);
        return CreatedAtAction(nameof(GetById), new { id = newOrder.Id }, newOrder);
    }

    /// <summary>
    /// Var olan bir siparişi günceller.
    /// </summary>
    /// <param name="id">Güncellenecek siparişin benzersiz kimliği (ID).</param>
    /// <param name="dto">Siparişin yeni bilgileri.</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     PUT /api/Orders/65b3...
    ///     {
    ///        "platformId": "60a7b1...",
    ///        "date": "2024-02-05T12:00:00Z",
    ///        "items": [
    ///           {
    ///             "name": "Güncellenmiş Ürün",
    ///             "purchasePrice": 120.0,
    ///             "salePrice": 200.0,
    ///             "commissionRate": 0.15,
    ///             "shippingCost": 15.0,
    ///             "quantity": 1
    ///           }
    ///        ]
    ///     }
    ///
    /// </remarks>
    /// <returns>Güncellenen siparişin son halini döndürür, bulunamazsa NotFound (404) döner.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, OrderCreateDto dto)
    {
        var existing = await orderAnalysisService.GetOrderByIdAsync(id);
        if (existing is null) return NotFound();

        existing.PlatformId = dto.PlatformId;
        existing.Date = dto.Date;
        existing.Items = dto.Items;
        await orderAnalysisService.UpdateOrderAsync(id, existing);
        return Ok(existing);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip siparişi kalıcı olarak siler.
    /// </summary>
    /// <param name="id">Silinecek siparişin benzersiz kimliği (ID).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     DELETE /api/Orders/65b3...
    ///
    /// </remarks>
    /// <returns>İşlem başarılıysa NoContent (204) döner, bulunamazsa NotFound (404) döner.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await orderAnalysisService.GetOrderByIdAsync(id);
        if (existing is null) return NotFound();

        await orderAnalysisService.DeleteOrderAsync(id);
        return NoContent();
    }
}
