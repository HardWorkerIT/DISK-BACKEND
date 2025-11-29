using Common.Abstracts;
using Common.Attributes;
using Common.Constants;
using Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using App.Application.DTOs;
using App.Application.DTOs.Requests;
using App.Application.DTOs.Responses;
using App.Application.Mappers;
using Common.Infrastructure.Services;
using Common.Infrastructure.Data;

namespace App.Controllers;

[ApiController]
[Route("api/v1/logs")]
[ApiExplorerSettings(GroupName = "Logs")]
[RequireAuth]
[RequireRole(EUserRoles.Admin)]
public class CLogs : BaseController
{
    private readonly ILogService _logService;

    public CLogs(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    [SwaggerResponse(200, "Список логов", typeof(PagedResponse<LogDTO>))]
    [SwaggerResponse(400, "Ошибка валидации")]
    public async Task<ActionResult<ApiResponse<PagedResponse<LogDTO>>>> GetAsync(
        [FromQuery(Name = "per_in_page")] int perInPage = 20,
        [FromQuery(Name = "page")] int page = 1)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        GetLogsRequest request = new(perInPage, page);

        if (!ValidateRequest<GetLogsRequest, PagedResponse<LogDTO>>(request, out var validationError))
            return validationError!;

        PagedResponse<Log> logsPaged = await _logService.GetLogsAsync(
            request.PerInPage,
            request.Page,
            cancellationTokenSource.Token);

        var pagedResponse = new PagedResponse<LogDTO>(
            logsPaged.Items.Select(LogMapper.GetDTO),
            logsPaged.TotalCount,
            logsPaged.Page,
            logsPaged.PerInPage
        );

        return Success(pagedResponse, "Логи успешно получены");
    }
}