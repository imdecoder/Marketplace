using Microsoft.AspNetCore.Mvc;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformsController(PlatformService platformService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var platforms = await platformService.GetAllAsync();
        return Ok(platforms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var platform = await platformService.GetByIdAsync(id);
        if (platform is null) return NotFound();
        return Ok(platform);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Platform newPlatform)
    {
        newPlatform.Id = null;
        await platformService.CreateAsync(newPlatform);
        return CreatedAtAction(nameof(GetById), new { id = newPlatform.Id }, newPlatform);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Platform updatedPlatform)
    {
        var existing = await platformService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        updatedPlatform.Id = id;
        await platformService.UpdateAsync(id, updatedPlatform);
        return Ok(updatedPlatform);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await platformService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await platformService.DeleteAsync(id);
        return NoContent();
    }
}
