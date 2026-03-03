using Microsoft.AspNetCore.Mvc;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/report")]
public class ReportsController : ControllerBase
{
    private readonly OrderAnalysisService _orderAnalysisService;

    public ReportsController(OrderAnalysisService orderAnalysisService)
    {
        _orderAnalysisService = orderAnalysisService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _orderAnalysisService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpGet("platform")]
    public async Task<IActionResult> GetPlatformReport()
    {
        var result = await _orderAnalysisService.GetPlatformReportAsync();
        return Ok(result);
    }

    [HttpGet("loss")]
    public async Task<IActionResult> GetLossReport()
    {
        var result = await _orderAnalysisService.GetLossReportAsync();
        return Ok(result);
    }

    [HttpGet("anomaly")]
    public async Task<IActionResult> GetAnomalyReport()
    {
        var result = await _orderAnalysisService.GetAnomalyReportAsync();
        return Ok(result);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrendReport()
    {
        var result = await _orderAnalysisService.GetTrendReportAsync();
        return Ok(result);
    }

    [HttpGet("risk")]
    public async Task<IActionResult> GetRiskReport()
    {
        var result = await _orderAnalysisService.GetRiskReportAsync();
        return Ok(result);
    }
}
