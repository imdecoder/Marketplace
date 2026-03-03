using Microsoft.AspNetCore.Mvc;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/report")]
public class ReportsController(OrderAnalysisService orderAnalysisService) : ControllerBase
{

    /// <summary>
    /// Satışların genel bir özetini (toplam satış, kar vb.) getirir.
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel). Örnek: 2024-01-01</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel). Örnek: 2024-12-31</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/report/summary?startDate=2024-01-01&amp;endDate=2024-01-31
    ///
    /// </remarks>
    /// <returns>Genel satış ve kar özet verilerini döndürür.</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await orderAnalysisService.GetSummaryAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Platform bazlı satış ve performans raporunu getirir.
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel).</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/report/platform
    ///
    /// </remarks>
    /// <returns>Platform başına elde edilen gelir, komisyon ve gider gibi bilgileri döndürür.</returns>
    [HttpGet("platform")]
    public async Task<IActionResult> GetPlatformReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await orderAnalysisService.GetPlatformReportAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Zarar eden veya düşük kar marjlı ürünlerin/siparişlerin raporunu getirir.
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel).</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/report/loss
    ///
    /// </remarks>
    /// <returns>Zarar getiren sipariş ve ürün verilerini döndürür.</returns>
    [HttpGet("loss")]
    public async Task<IActionResult> GetLossReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await orderAnalysisService.GetLossReportAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Anormallik tespit edilen siparişlerin (örneğin aşırı yüksek fiyat veya kargo ücreti) raporunu getirir.
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel).</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/report/anomaly
    ///
    /// </remarks>
    /// <returns>Potansiyel hatalı veya anormal siparişlerin listesini döndürür.</returns>
    [HttpGet("anomaly")]
    public async Task<IActionResult> GetAnomalyReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await orderAnalysisService.GetAnomalyReportAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Zaman içindeki satış trendlerini (günlük/haftalık/aylık bazda) getirir.
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel).</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/report/trend
    ///
    /// </remarks>
    /// <returns>Satışların zaman içindeki değişimini gösteren trend analizini döndürür.</returns>
    [HttpGet("trend")]
    public async Task<IActionResult> GetTrendReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await orderAnalysisService.GetTrendReportAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Riskli görülen (yüksek iade oranı ihtimali, yüksek maliyet) siparişlerin/ürünlerin risk raporunu getirir.
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel).</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel).</param>
    /// <remarks>
    /// Örnek istek:
    ///
    ///     GET /api/report/risk
    ///
    /// </remarks>
    /// <returns>Siparişlere ait risk seviyelerini ve detaylarını döndürür.</returns>
    [HttpGet("risk")]
    public async Task<IActionResult> GetRiskReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await orderAnalysisService.GetRiskReportAsync(startDate, endDate);
        return Ok(result);
    }
}
