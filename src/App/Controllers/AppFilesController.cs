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

namespace App.Controllers;

[ApiController]
[Route("api/v1/upload/files")]
[ApiExplorerSettings(GroupName = "AppFiles")]
public class AppFilesController : BaseController
{
    private readonly string _uploadPath;
    private readonly AppDbContext _dbContext;

    public AppFilesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        
        // Определяем корневую директорию проекта - ищем директорию с файлом StandardBackend.sln
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);
        var projectRoot = currentDir;
        
        // Поднимаемся вверх по директориям, пока не найдем файл StandardBackend.sln
        while (dir != null)
        {
            var slnPath = Path.Combine(dir.FullName, "App.sln");
            if (System.IO.File.Exists(slnPath))
            {
                projectRoot = dir.FullName;
                break;
            }
            dir = dir.Parent;
        }
        
        // Формируем путь к папке uploads/files в корне проекта
        _uploadPath = Path.Combine(projectRoot, "uploads", "files");
    }


    [HttpGet]
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

        // Используем путь из БД, если он есть, иначе вычисляем по старой логике
        string filePath = !string.IsNullOrEmpty(appFile.Path) 
            ? appFile.Path 
            : $"{_uploadPath}/{fileUuid}{Path.GetExtension(appFile.Filename)}";

        using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

        if (System.IO.File.Exists(filePath) is false)
        {
            return NotFound(ApiResponse.ErrorResult("NOT_FOUND", "Файл не найден на диске"));
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath, cancellationTokenSource.Token);

        // Определяем MIME-тип на основе расширения файла
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        if (!contentTypeProvider.TryGetContentType(appFile.Filename ?? filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return File(new MemoryStream(fileBytes), contentType, appFile.Filename);
    }

    [HttpPost]
    [RequireAuth]
    [RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]
    public async Task<ActionResult<ApiResponse<List<Guid>>>> UploadAsync()
    {
        IFormFileCollection files = HttpContext.Request.Form.Files;

        if (files.Count == 0)
        {
            return Error<List<Guid>>("NO_FILES", "Файлы не были загружены");
        }

        Directory.CreateDirectory(_uploadPath);

        var uploadedUuids = new List<Guid>();

        foreach (var file in files)
        {
            // Генерируем UUID4
            var uuid = Guid.NewGuid();
            
            // Получаем расширение файла
            var fileExtension = Path.GetExtension(file.FileName);
            
            // Сохраняем файл с именем UUID + расширение
            var uuidFileName = uuid.ToString() + fileExtension;
            string fullPath = $"{_uploadPath}/{uuidFileName}";

            using (var fileStream = new FileStream(fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(fileStream);
            }

            // Создаем запись в БД
            var appFile = new AppFile
            {
                Uuid = uuid,
                Filename = file.FileName,
                Path = fullPath
            };

            _dbContext.AppFiles.Add(appFile);
            uploadedUuids.Add(uuid);
        }

        await _dbContext.SaveChangesAsync();

        return Success(uploadedUuids, "Файлы успешно загружены");
    }
}