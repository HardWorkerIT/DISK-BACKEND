using Common.Abstracts;
using Common.Attributes;
using Common.Constants;
using Common.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using App.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using App.Infrastructure.Storage;

namespace App.Controllers;

[ApiController]
[Route("api/v1/upload/files")]
[ApiExplorerSettings(GroupName = "AppFiles")]
public class AppFilesController : BaseController
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorage _storage;

    public AppFilesController(AppDbContext dbContext, IFileStorage storage)
    {
        _dbContext = dbContext;
        _storage = storage;
    }


    [HttpGet]
    [RequireAuth]
    public async Task<IActionResult> DownloadAsync([FromQuery(Name = "uuid")] string uuid)
    {
        if (!Guid.TryParse(uuid, out var fileUuid))
        {
            return BadRequest(ApiResponse.ErrorResult("INVALID_UUID", "Неверный формат UUID"));
        }

        var appFile = await _dbContext.AppFiles
            .FirstOrDefaultAsync(f => f.Uuid == fileUuid && f.DeletedAt == null);

        if (appFile == null)
        {
            return NotFound(ApiResponse.ErrorResult("NOT_FOUND", "Файл не найден"));
        }

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cancellationTokenSource.CancelAfter(Constants.BaseTimeout);
        var storageKey = appFile.Path ?? $"{fileUuid}{Path.GetExtension(appFile.Filename)}";
        var stream = await _storage.OpenReadAsync(storageKey, cancellationTokenSource.Token);
        if (stream is null)
        {
            return NotFound(ApiResponse.ErrorResult("NOT_FOUND", "Файл не найден на диске"));
        }

        // Определяем MIME-тип на основе расширения файла
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        if (!contentTypeProvider.TryGetContentType(appFile.Filename ?? storageKey, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, contentType, Path.GetFileName(appFile.Filename), enableRangeProcessing: true);
    }

    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    public async Task<ActionResult<ApiResponse<List<Guid>>>> UploadAsync()
    {
        IFormFileCollection files = HttpContext.Request.Form.Files;
        var validationFailure = await UploadFileValidator.ValidateAsync(files, HttpContext.RequestAborted);
        if (validationFailure is not null)
            return Error<List<Guid>>(validationFailure.Code, validationFailure.Message);

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cancellationTokenSource.CancelAfter(Constants.BaseTimeout);
        var uploadedUuids = new List<Guid>();
        var storedKeys = new List<string>();
        try
        {
            foreach (var file in files)
            {
                // Генерируем UUID4
                var uuid = Guid.NewGuid();

                // Получаем расширение файла
                var displayName = UploadFileValidator.NormalizeFileName(file.FileName);
                var fileExtension = Path.GetExtension(displayName);

                // Сохраняем файл с именем UUID + расширение
                var uuidFileName = uuid.ToString() + fileExtension;
                storedKeys.Add(uuidFileName);
                await using (var fileStream = file.OpenReadStream())
                    await _storage.StoreAsync(uuidFileName, fileStream, cancellationTokenSource.Token);

                // Создаем запись в БД
                var appFile = new AppFile
                {
                    Uuid = uuid,
                    Filename = displayName,
                    Path = uuidFileName
                };

                _dbContext.AppFiles.Add(appFile);
                uploadedUuids.Add(uuid);
            }

            await _dbContext.SaveChangesAsync(cancellationTokenSource.Token);

            return Success(uploadedUuids, "Файлы успешно загружены");
        }
        catch
        {
            foreach (var storedKey in storedKeys)
                await _storage.DeleteAsync(storedKey, CancellationToken.None);
            throw;
        }
    }

}
