using Common.Constants;
using Common.Abstracts;
using Common.Attributes;
using Common.DTOs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Common.Infrastructure.Services;
using Files.Application.Interfaces;
using Files.Application.DTOs.Base;
using Files.Application.DTOs.Requests;
using Files.Domain.Entities;
using File = Files.Domain.Entities.File;
using Files.Application.Mappers;
using Files.Application.DTOs.Responses;

namespace Files.Presentation.Controllers;

[ApiController]
[Route("api/v1/files")]
[ApiExplorerSettings(GroupName = "Files")]
public class FilesController : BaseController
{
    private readonly IFileService _service;
    private readonly ILogService _logService;

    public FilesController(IFileService service, ILogService logService)
    {
        _service = service;
        _logService = logService;
    }

    [HttpGet]
    [SwaggerResponse(200, "Список файлов", typeof(PagedResponse<FileDTO>))]
    [SwaggerResponse(400, "Ошибка валидации")]
    public async Task<ActionResult<ApiResponse<PagedResponse<FileDTO>>>> GetAsync([FromQuery(Name = "group_id")] long groupId, [FromQuery(Name = "per_in_page")] int perInPage = 20, [FromQuery(Name = "page")] int page = 1)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        GetFileRequest request = new(groupId, perInPage, page);

        if (!ValidateRequest<GetFileRequest, PagedResponse<FileDTO>>(request, out var validationError))
        {
            return validationError!;
        }

        PagedResponse<File> filesPaged = await _service.SelectAsync(request, cancellationTokenSource.Token);

        var pagedResponse = new PagedResponse<FileDTO>(
            filesPaged.Items.Select(FileMapper.GetBody),
            filesPaged.TotalCount,
            filesPaged.Page,
            filesPaged.PerInPage
        );

        return Success(pagedResponse, "Файлы успешно получены");
    }

    [HttpPost]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    [SwaggerResponse(200, "Файл успешно создан", typeof(CreateFileResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Ресурс не найден")]
    public async Task<ActionResult<ApiResponse<CreateFileResponse>>> PostAsync([FromBody] CreateFileRequest request)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        if (!ValidateRequest<CreateFileRequest, CreateFileResponse>(request, out var validationError))
            return validationError!;

        File? File = await _service.CreateAsync(request, cancellationTokenSource.Token);

        if (File is null)
        {
            return NotFound<CreateFileResponse>();
        }

        FileDTO fileDTO = FileMapper.GetBody(File);

        // Логирование создания стандарта
        await _logService.LogFromHttpContextAsync(
            HttpContext,
            "File_CREATED",
            "File",
            fileDTO.Id,
            $"Создан файл: {fileDTO.Name} (№{fileDTO.Id})",
            cancellationTokenSource.Token);

        return Success(new CreateFileResponse(fileDTO.FolderId, fileDTO.Name, fileDTO.CreatedAt, fileDTO.UpdatedAt), "Файл успешно создан");
    }

    [HttpPut]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    [SwaggerResponse(200, "файл успешно обновлен", typeof(UpdateFileResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Ресурс не найден")]
    public async Task<ActionResult<ApiResponse<UpdateFileResponse>>> PutAsync([FromBody] UpdateFileRequest request)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        if (!ValidateRequest<UpdateFileRequest, UpdateFileResponse>(request, out var validationError))
            return validationError!;

        await _service.UpdateAsync(request, cancellationTokenSource.Token);

        // Логирование изменения стандарта
        await _logService.LogFromHttpContextAsync(
            HttpContext,
            "File_UPDATED",
            "File",
            request.Id,
            $"Изменен файл: {request.Name} (№{request.Id})",
            cancellationTokenSource.Token);

        return Success(new UpdateFileResponse(), "Файл успешно обновлен");
    }

    [HttpDelete]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    [SwaggerResponse(200, "файл успешно удален", typeof(DeleteFileResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Ресурс не найден")]
    public async Task<ActionResult<ApiResponse<DeleteFileResponse>>> DeleteAsync(long id)
    {
        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        DeleteFileRequest request = new DeleteFileRequest(id);

        if (!ValidateRequest<DeleteFileRequest, DeleteFileResponse>(request, out var validationError))
            return validationError!;

        await _service.DeleteAsync(request, cancellationTokenSource.Token);

        // Логирование удаления стандарта
        await _logService.LogFromHttpContextAsync(
            HttpContext,
            "File_DELETED",
            "File",
            id,
            $"Удален файл с ID: {id}",
            cancellationTokenSource.Token);

        return Success(new DeleteFileResponse(), "Файл успешно удален");
    }
}