using Common.Abstracts;
using Common.Attributes;
using Common.Constants;
using Common.DTOs;
using Common.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using Users.Application.DTOs.Requests;
using Users.Application.DTOs.Responses;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Standards.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
[ApiExplorerSettings(GroupName = "Users")]
[RequireAuth] // Требует авторизации для всех действий контроллера
[RequireRole(EUserRoles.Admin)] // Требует роль Admin для всех действий контроллера
public class UsersController : BaseController
{
    private readonly IUserService _service;
    private readonly ILogService _logService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService service, ILogService logService, ILogger<UsersController> logger)
    {
        _service = service;
        _logService = logService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerResponse(200, "Список пользователей", typeof(PagedResponse<UserDTOWithoutPassword>))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserDTOWithoutPassword>>>> GetAsync([FromQuery(Name = "per_in_page")] int perInPage = 20, [FromQuery(Name = "page")] int page = 1)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            GetUserRequest request = new(perInPage, page);

            if (!ValidateRequest<GetUserRequest, PagedResponse<UserDTOWithoutPassword>>(request, out var validationError))
                return validationError!;

            _logger.LogDebug("Запрос списка пользователей. Page: {Page}, PerInPage: {PerInPage}", page, perInPage);

            PagedResponse<User> usersPaged = await _service.SelectAsync(request, cancellationTokenSource.Token);

            var pagedResponse = new PagedResponse<UserDTOWithoutPassword>(
                usersPaged.Items.Select(UserMapper.GetBodyWithoutPassword),
                usersPaged.TotalCount,
                usersPaged.Page,
                usersPaged.PerInPage
            );

            _logger.LogDebug("Список пользователей получен. Всего: {TotalCount}, На странице: {Count}", 
                pagedResponse.TotalCount, pagedResponse.Items.Count());

            return Success(pagedResponse, "Пользователи успешно получены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка пользователей. Page: {Page}, PerInPage: {PerInPage}", page, perInPage);
            
            try
            {
                await _logService.LogFromHttpContextAsync(
                    HttpContext,
                    "USER_LIST_ERROR",
                    "User",
                    null,
                    $"Ошибка при получении списка пользователей: {ex.Message}",
                    CancellationToken.None);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
            
            return Error<PagedResponse<UserDTOWithoutPassword>>("INTERNAL_ERROR", "Произошла ошибка при получении списка пользователей");
        }
    }

    [HttpPost]
    [SwaggerResponse(200, "Пользователь успешно создан", typeof(CreateUserResponse))]
    [SwaggerResponse(400, "Ошибка валидации или бизнес-логики")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> PostAsync([FromBody] CreateUserRequest request)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            if (!ValidateRequest<CreateUserRequest, CreateUserResponse>(request, out var validationError))
                return validationError!;

            _logger.LogInformation("Попытка создания пользователя с логином: {Login}", request.Login);

            User? user = await _service.CreateAsync(request, cancellationTokenSource.Token);

            if (user is null)
            {
                _logger.LogWarning("Не удалось создать пользователя. Сервис вернул null для логина: {Login}", request.Login);
                return Error<CreateUserResponse>("CREATE_FAILED", "Не удалось создать пользователя. Попробуйте позже.");
            }

            UserDTOWithoutPassword userDTO = UserMapper.GetBodyWithoutPassword(user);

            _logger.LogInformation("Пользователь успешно создан. ID: {UserId}, Login: {Login}", userDTO.Id, userDTO.Login);

            return Success(new CreateUserResponse(
                userDTO.Id, userDTO.Login, userDTO.Email, userDTO.FirstName, userDTO.MiddleName, userDTO.LastName, userDTO.RoleId), "Пользователь успешно создан");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ошибка валидации при создании пользователя. Login: {Login}", request?.Login);
            return Error<CreateUserResponse>("VALIDATION_ERROR", ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка базы данных при создании пользователя. Login: {Login}", request?.Login);
            
            // Проверяем, является ли это ошибкой уникальности
            var innerException = ex.InnerException;
            if (innerException?.Message?.Contains("duplicate") == true || 
                innerException?.Message?.Contains("unique") == true ||
                ex.Message.Contains("duplicate") || 
                ex.Message.Contains("unique"))
            {
                return Error<CreateUserResponse>("DUPLICATE_ENTRY", "Пользователь с таким логином или email уже существует");
            }

            return Error<CreateUserResponse>("DATABASE_ERROR", "Ошибка при сохранении данных. Попробуйте позже.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка операции при создании пользователя. Login: {Login}", request?.Login);
            return Error<CreateUserResponse>("OPERATION_ERROR", ex.Message);
        }
        catch (NullReferenceException ex)
        {
            _logger.LogError(ex, "Ошибка: null ссылка при создании пользователя. Login: {Login}, StackTrace: {StackTrace}", request?.Login, ex.StackTrace);
            
            try
            {
                await _logService.LogFromHttpContextAsync(
                    HttpContext,
                    "USER_CREATE_NULL_ERROR",
                    "User",
                    null,
                    $"Ошибка null ссылки при создании пользователя: {ex.Message}. StackTrace: {ex.StackTrace}",
                    CancellationToken.None);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
            
            return Error<CreateUserResponse>("VALIDATION_ERROR", "Ошибка валидации данных. Проверьте, что все обязательные поля заполнены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при создании пользователя. Login: {Login}, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, InnerException: {InnerException}", 
                request?.Login, 
                ex.GetType().Name, 
                ex.Message, 
                ex.StackTrace, 
                ex.InnerException?.Message);
            
            // Логируем в систему логирования с полными деталями
            try
            {
                await _logService.LogFromHttpContextAsync(
                    HttpContext,
                    "USER_CREATE_ERROR",
                    "User",
                    null,
                    $"Неожиданная ошибка при создании пользователя. Тип: {ex.GetType().Name}, Сообщение: {ex.Message}, StackTrace: {ex.StackTrace}, InnerException: {ex.InnerException?.Message}",
                    CancellationToken.None);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
            
            return Error<CreateUserResponse>("INTERNAL_ERROR", $"Произошла внутренняя ошибка при создании пользователя: {ex.GetType().Name} - {ex.Message}");
        }
    }

    [HttpPut]
    [SwaggerResponse(200, "Пользователь успешно обновлен", typeof(UpdateUserResponse))]
    [SwaggerResponse(400, "Ошибка валидации или бизнес-логики")]
    [SwaggerResponse(404, "Пользователь не найден")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ApiResponse<UpdateUserResponse>>> PutAsync([FromBody] UpdateUserRequest request)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            if (!ValidateRequest<UpdateUserRequest, UpdateUserResponse>(request, out var validationError))
                return validationError!;

            _logger.LogInformation("Попытка обновления пользователя. ID: {UserId}, Login: {Login}", request.Id, request.Login);

            await _service.UpdateAsync(request, cancellationTokenSource.Token);

            // Логирование изменения пользователя
            await _logService.LogFromHttpContextAsync(
                HttpContext,
                "USER_UPDATED",
                "User",
                request.Id,
                $"Изменен пользователь: {request.Login}",
                cancellationTokenSource.Token);

            _logger.LogInformation("Пользователь успешно обновлен. ID: {UserId}", request.Id);

            return Success(new UpdateUserResponse(), "Пользователь успешно обновлен");
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Пользователь не найден при обновлении. ID: {UserId}", request?.Id);
            return NotFound<UpdateUserResponse>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ошибка валидации при обновлении пользователя. ID: {UserId}", request?.Id);
            return Error<UpdateUserResponse>("VALIDATION_ERROR", ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка базы данных при обновлении пользователя. ID: {UserId}", request?.Id);
            
            var innerException = ex.InnerException;
            if (innerException?.Message?.Contains("duplicate") == true || 
                innerException?.Message?.Contains("unique") == true ||
                ex.Message.Contains("duplicate") || 
                ex.Message.Contains("unique"))
            {
                return Error<UpdateUserResponse>("DUPLICATE_ENTRY", "Пользователь с таким логином или email уже существует");
            }

            return Error<UpdateUserResponse>("DATABASE_ERROR", "Ошибка при сохранении данных. Попробуйте позже.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при обновлении пользователя. ID: {UserId}", request?.Id);
            
            try
            {
                await _logService.LogFromHttpContextAsync(
                    HttpContext,
                    "USER_UPDATE_ERROR",
                    "User",
                    request?.Id,
                    $"Неожиданная ошибка при обновлении пользователя: {ex.Message}",
                    CancellationToken.None);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
            
            return Error<UpdateUserResponse>("INTERNAL_ERROR", "Произошла внутренняя ошибка при обновлении пользователя");
        }
    }

    [HttpDelete]
    [SwaggerResponse(200, "Пользователь успешно удален", typeof(DeleteUserResponse))]
    [SwaggerResponse(400, "Ошибка валидации или бизнес-логики")]
    [SwaggerResponse(404, "Пользователь не найден")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> DeleteAsync(long id)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            DeleteUserRequest request = new(id);

            if (!ValidateRequest<DeleteUserRequest, DeleteUserResponse>(request, out var validationError))
                return validationError!;

            _logger.LogInformation("Попытка удаления пользователя. ID: {UserId}", id);

            await _service.DeleteAsync(request, cancellationTokenSource.Token);

            // Логирование удаления пользователя
            await _logService.LogFromHttpContextAsync(
                HttpContext,
                "USER_DELETED",
                "User",
                id,
                $"Удален пользователь с ID: {id}",
                cancellationTokenSource.Token);

            _logger.LogInformation("Пользователь успешно удален. ID: {UserId}", id);

            return Success(new DeleteUserResponse(), "Пользователь успешно удален");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ошибка при удалении пользователя. ID: {UserId}, Сообщение: {Message}", id, ex.Message);
            return Error<DeleteUserResponse>("FORBIDDEN", ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка базы данных при удалении пользователя. ID: {UserId}", id);
            return Error<DeleteUserResponse>("DATABASE_ERROR", "Ошибка при удалении данных. Попробуйте позже.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при удалении пользователя. ID: {UserId}", id);
            
            try
            {
                await _logService.LogFromHttpContextAsync(
                    HttpContext,
                    "USER_DELETE_ERROR",
                    "User",
                    id,
                    $"Неожиданная ошибка при удалении пользователя: {ex.Message}",
                    CancellationToken.None);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
            
            return Error<DeleteUserResponse>("INTERNAL_ERROR", "Произошла внутренняя ошибка при удалении пользователя");
        }
    }
}
