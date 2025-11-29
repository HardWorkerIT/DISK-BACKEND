using Common.Constants;
using Common.Abstracts;
using Common.Attributes;
using Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Common.Infrastructure.Services;
using Files.Application.Interfaces;
using Files.Application.DTOs.Base;
using Files.Application.DTOs.Requests;
using Files.Domain.Entities;
using Files.Application.Mappers;
using Files.Application.DTOs.Responses;

namespace Standards.Presentation.Controllers;

[ApiController]
[Route("api/v1/files/folders")]
[ApiExplorerSettings(GroupName = "File Folders")]
public class FoldersController : BaseController
{
    private readonly IFolderService _service;
    private readonly ILogService _logService;

    public FoldersController(IFolderService service, ILogService logService)
    {
        _service = service;
        _logService = logService;
    }

    [HttpGet]
    [SwaggerResponse(200, "Список папок с файлами", typeof(PagedResponse<FolderDTO>))]
    [SwaggerResponse(400, "Ошибка валидации")]
    public async Task<ActionResult<ApiResponse<PagedResponse<FolderDTO>>>> GetAsync([FromQuery(Name = "per_in_page")] int perInPage = 20, [FromQuery(Name = "page")] int page = 1)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        GetFolderRequest request = new(perInPage, page);

        if (!ValidateRequest<GetFolderRequest, PagedResponse<FolderDTO>>(request, out var validationError))
            return validationError!;

        PagedResponse<Folder> foldersPaged = await _service.SelectAsync(request, cancellationTokenSource.Token);

        var pagedResponse = new PagedResponse<FolderDTO>(
            foldersPaged.Items.Select(FolderMapper.GetBody),
            foldersPaged.TotalCount,
            foldersPaged.Page,
            foldersPaged.PerInPage
        );

        return Success(pagedResponse, "Папки успешно получены");
    }

    [HttpPost]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    [SwaggerResponse(200, "Папка с файлами успешно создана", typeof(CreateFolderResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Ресурс не найден")]
    public async Task<ActionResult<ApiResponse<CreateFolderResponse>>> PostAsync([FromBody] CreateFolderRequest request)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        if (!ValidateRequest<CreateFolderRequest, CreateFolderResponse>(request, out var validationError))
            return validationError!;

        Folder? folder = await _service.CreateAsync(request, cancellationTokenSource.Token);

        if (folder is null)
        {
            return NotFound<CreateFolderResponse>();
        }

        FolderDTO folderDTO = FolderMapper.GetBody(folder);

        // Логирование создания группы стандартов
        await _logService.LogFromHttpContextAsync(
            HttpContext,
            "STANDARD_GROUP_CREATED",
            "Folder",
            folderDTO.Id,
            $"Создана папка с файлами: {folderDTO.Name}",
            cancellationTokenSource.Token);

        return Success(new CreateFolderResponse(folderDTO.ParentId, folderDTO.Name), "Папка с файлами успешно создана");
    }

    [HttpPut]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    [SwaggerResponse(200, "Папка с файлами успешно обновлена", typeof(UpdateFolderResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Ресурс не найден")]
    public async Task<ActionResult<ApiResponse<UpdateFolderResponse>>> PutAsync([FromBody] UpdateFolderRequest request)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        if (!ValidateRequest<UpdateFolderRequest, UpdateFolderResponse>(request, out var validationError))
            return validationError!;

        await _service.UpdateAsync(request, cancellationTokenSource.Token);

        // Логирование изменения группы стандартов
        await _logService.LogFromHttpContextAsync(
            HttpContext,
            "STANDARD_GROUP_UPDATED",
            "Folder",
            request.Id,
            $"Изменена папка с файлами: {request.Name}",
            cancellationTokenSource.Token);

        return Success(new UpdateFolderResponse(), "Папка с файлами успешно обновлена");
    }

    [HttpDelete]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    [SwaggerResponse(200, "Папка с файлами успешно удалена", typeof(DeleteFolderResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Ресурс не найден")]
    public async Task<ActionResult<ApiResponse<DeleteFolderResponse>>> DeleteAsync(long id)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        DeleteFolderRequest request = new(id);

        if (!ValidateRequest<DeleteFolderRequest, DeleteFolderResponse>(request, out var validationError))
            return validationError!;

        await _service.DeleteAsync(request, cancellationTokenSource.Token);

        // Логирование удаления группы стандартов
        await _logService.LogFromHttpContextAsync(
            HttpContext,
            "STANDARD_GROUP_DELETED",
            "Folder",
            id,
            $"Удалена папка с ID: {id}",
            cancellationTokenSource.Token);

        return Success(new DeleteFolderResponse(), "Папка успешно удалена");
    }
}