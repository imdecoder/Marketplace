using Microsoft.AspNetCore.Mvc;
using Marketplace.Services;

namespace Marketplace.Controllers;

[ApiController]
[Route("api/report")]
public class ReportsController(OrderAnalysisService orderAnalysisService) : ControllerBase
{

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await orderAnalysisService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpGet("platform")]
    public async Task<IActionResult> GetPlatformReport()
    {
        var result = await orderAnalysisService.GetPlatformReportAsync();
        return Ok(result);
    }

    [HttpGet("loss")]
    public async Task<IActionResult> GetLossReport()
    {
        var result = await orderAnalysisService.GetLossReportAsync();
        return Ok(result);
    }

    [HttpGet("anomaly")]
    public async Task<IActionResult> GetAnomalyReport()
    {
        var result = await orderAnalysisService.GetAnomalyReportAsync();
        return Ok(result);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrendReport()
    {
        var result = await orderAnalysisService.GetTrendReportAsync();
        return Ok(result);
    }

    [HttpGet("risk")]
    public async Task<IActionResult> GetRiskReport()
    {
        var result = await orderAnalysisService.GetRiskReportAsync();
        return Ok(result);
    }
}
