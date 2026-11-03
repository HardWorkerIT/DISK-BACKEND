using Common.Abstracts;
using Common.Constants;
using Common.DTOs;
using App.Infrastructure.Data;
using Common.Infrastructure.Metrics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Standards.Presentation.Controllers;

[ApiController]
[Route("")]
[ApiExplorerSettings(GroupName = "Health")]
public class CHealth(AppDbContext dbContext, HttpMetricsRegistry metrics) : BaseController
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return "OK";
    }

    [HttpGet("/health/live")]
    public ActionResult GetLiveness() => Ok(new { status = "healthy" });

    [HttpGet("/metrics")]
    [Produces("text/plain")]
    public ContentResult GetMetrics() => Content(metrics.ExportPrometheus(), "text/plain; version=0.0.4; charset=utf-8");

    [HttpGet("/health/ready")]
    public async Task<ActionResult> GetReadiness(CancellationToken cancellationToken)
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync(cancellationToken))
                return Ok(new { status = "ready" });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "not_ready" });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "not_ready" });
    }
}
