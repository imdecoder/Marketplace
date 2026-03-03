using Microsoft.AspNetCore.Mvc;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformsController(PlatformService platformService) : ControllerBase
{
    /// <summary>
    /// Sistemde kayıtlı olan tüm e-ticaret platformlarını listeler.
    /// </summary>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/Platforms
    ///
    /// </remarks>
    /// <returns>Tüm platformların bir listesini döndürür.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var platforms = await platformService.GetAllAsync();
        return Ok(platforms);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip platformun detaylarını getirir.
    /// </summary>
    /// <param name="id">Platformun benzersiz kimliği (ID).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/Platforms/60a7b1...
    ///
    /// </remarks>
    /// <returns>Bulunan platform nesnesini döndürür, bulunamazsa NotFound (404) döner.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var platform = await platformService.GetByIdAsync(id);
        if (platform is null) return NotFound();
        return Ok(platform);
    }

    /// <summary>
    /// Yeni bir e-ticaret platformu oluşturur (örneğin: Trendyol, Hepsiburada).
    /// </summary>
    /// <param name="dto">Oluşturulacak platformun adı.</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     POST /api/Platforms
    ///     {
    ///        "name": "Trendyol"
    ///     }
    ///
    /// </remarks>
    /// <returns>Oluşturulan platformun erişim URL'sini ve bilgilerini döndürür.</returns>
    [HttpPost]
    public async Task<IActionResult> Post(PlatformCreateDto dto)
    {
        var newPlatform = new Platform { Name = dto.Name };
        await platformService.CreateAsync(newPlatform);
        return CreatedAtAction(nameof(GetById), new { id = newPlatform.Id }, newPlatform);
    }

    /// <summary>
    /// Var olan bir platformun bilgilerini günceller.
    /// </summary>
    /// <param name="id">Güncellenecek platformun benzersiz kimliği (ID).</param>
    /// <param name="dto">Platformun yeni adı.</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     PUT /api/Platforms/60a7b1...
    ///     {
    ///        "name": "Hepsiburada"
    ///     }
    ///
    /// </remarks>
    /// <returns>Güncellenen platformun son halini döndürür, bulunamazsa NotFound (404) döner.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, PlatformCreateDto dto)
    {
        var existing = await platformService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        existing.Name = dto.Name;
        await platformService.UpdateAsync(id, existing);
        return Ok(existing);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip platformu kalıcı olarak siler.
    /// </summary>
    /// <param name="id">Silinecek platformun benzersiz kimliği (ID).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     DELETE /api/Platforms/60a7b1...
    ///
    /// </remarks>
    /// <returns>İşlem başarılıysa NoContent (204) döner, bulunamazsa NotFound (404) döner.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await platformService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await platformService.DeleteAsync(id);
        return NoContent();
    }
}
