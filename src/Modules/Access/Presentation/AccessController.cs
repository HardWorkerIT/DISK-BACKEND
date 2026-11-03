using Access.Application.DTOs.Requests;
using Access.Application.DTOs.Responses;
using Access.Application.Interfaces;
using Access.Domain.Entities;
using Common.Abstracts;
using Common.Attributes;
using Common.Constants;
using Common.DTOs;
using Common.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Access.Presentation;

[ApiController]
[Route("api/v1/access")]
[ApiExplorerSettings(GroupName = "Access")]
public class AccessController : BaseController
{
    private readonly IAccessService _service;
    public AccessController(IAccessService service)
    {
        _service = service;
    }

    private async Task AppendCookiesAsync(IEnumerable<Claim> claims)
    {
        ClaimsIdentity claimsIdentity = new(claims, "Cookies");
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    }

    [HttpPost("change-password")]
    [RequireAuth]
    [SwaggerResponse(200, "Пароль изменён")]
    [SwaggerResponse(400, "Текущий пароль неверен или запрос невалиден")]
    [SwaggerResponse(401, "Требуется авторизация")]
    public async Task<ActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        if (!ValidateRequest<ChangePasswordRequest, object>(request, out var validationError))
            return validationError!.Result!;
        if (!long.TryParse(User.FindFirst("UserId")?.Value, out var userId))
            return Unauthorized("Сессия недействительна").Result!;

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cancellationTokenSource.CancelAfter(Constants.BaseTimeout);
        try
        {
            await _service.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationTokenSource.Token);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Success(message: "Пароль изменён. Войдите с новым паролем.").Result!;
        }
        catch (InvalidPasswordException)
        {
            return Error("INVALID_CURRENT_PASSWORD", "Текущий пароль неверен").Result!;
        }
    }

    [HttpGet("sessions")]
    [RequireAuth]
    public async Task<ActionResult<ApiResponse<object?>>> GetSessionsAsync()
    {
        if (!long.TryParse(User.FindFirst("UserId")?.Value, out var userId))
            return Unauthorized<object?>("Сессия недействительна");
        var currentToken = User.FindFirst("User")?.Value;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cts.CancelAfter(Constants.BaseTimeout);
        var sessions = await _service.GetActiveSessionsAsync(userId, cts.Token);
        var result = sessions.Select(session => new
        {
            session.Id,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiredAt,
            IsCurrent = session.Token == currentToken
        });
        return Success<object?>(result, "Активные сессии");
    }

    [HttpDelete("sessions/{sessionId:long}")]
    [RequireAuth]
    public async Task<ActionResult<ApiResponse<object?>>> RevokeSessionAsync(long sessionId)
    {
        if (!long.TryParse(User.FindFirst("UserId")?.Value, out var userId))
            return Unauthorized<object?>("Сессия недействительна");
        var currentToken = User.FindFirst("User")?.Value;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cts.CancelAfter(Constants.BaseTimeout);
        var sessions = await _service.GetActiveSessionsAsync(userId, cts.Token);
        var revokesCurrentSession = sessions.Any(session => session.Id == sessionId && session.Token == currentToken);
        if (!await _service.RevokeSessionAsync(userId, sessionId, cts.Token))
            return NotFound<object?>("Сессия не найдена");
        if (revokesCurrentSession)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Success<object?>(null, "Сессия завершена");
    }

    [HttpDelete("sessions")]
    [RequireAuth]
    public async Task<ActionResult<ApiResponse<object?>>> RevokeAllSessionsAsync()
    {
        if (!long.TryParse(User.FindFirst("UserId")?.Value, out var userId))
            return Unauthorized<object?>("Сессия недействительна");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cts.CancelAfter(Constants.BaseTimeout);
        await _service.RevokeAllSessionsAsync(userId, cts.Token);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Success<object?>(null, "Все сессии завершены");
    }

    [HttpPost]
    [Route("identify")]
    [SwaggerResponse(200, "Идентификация успешна", typeof(IdentifyResponse))]
    [SwaggerResponse(401, "Сессия недействительна или отсутствует")]
    public async Task<ActionResult<ApiResponse<IdentifyResponse>>> IdentifyAsync()
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result.Succeeded is false || result.Principal is null)
            {
                return Unauthorized<IdentifyResponse>("Требуется авторизация");
            }

            var userClaim = result.Principal.FindFirst("User");

            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            {
                return Unauthorized<IdentifyResponse>("Сессия недействительна");
            }

            UserDTOWithoutPassword? userBody = await _service.IdentifyAsync(userClaim.Value, cancellationTokenSource.Token);

            if (userBody is null)
            {
                return NotFound<IdentifyResponse>("User is not found");
            }

            return Success(new IdentifyResponse
                (userBody.Id, userBody.Login, userBody.Email, userBody.FirstName, userBody.MiddleName, userBody.LastName, userBody.RoleId), "Identify is successfull");
        }

        catch (ArgumentNullException)
        {
            return Unauthorized<IdentifyResponse>("Сессия недействительна");
        }
    }

    [HttpPost]
    [Route("authentificate")]
    [EnableRateLimiting("authentication")]
    [SwaggerResponse(200, "Аутентификация успешна", typeof(AuthentificateResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(401, "Неверный логин или пароль")]
    public async Task<ActionResult<ApiResponse<AuthentificateResponse>>> AuthentificateAsync([FromBody] AuthentificateRequest request)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            if (!ValidateRequest<AuthentificateRequest, AuthentificateResponse>(request, out var validationError))
                return validationError!;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            AccessToken accessToken = await _service.AuthentificateAsync(request.Login, request.Password, cancellationTokenSource.Token);

            // Получаем информацию о пользователе для сохранения RoleId в claims
            UserDTOWithoutPassword? user = await _service.IdentifyAsync(accessToken.Token!, cancellationTokenSource.Token);

            if (user == null)
            {
                return NotFound<AuthentificateResponse>("User not found after authentication");
            }

            // Сохраняем claims: User (токен) и RoleId (для проверки прав)
            await AppendCookiesAsync(
            [
                new Claim("User", accessToken.Token!),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Login", user.Login ?? string.Empty),
            ]);

            return Success(new AuthentificateResponse(), "Authentificate is successfull");
        }

        catch (UserNotFoundException)
        {
            return Unauthorized<AuthentificateResponse>("Неверный логин или пароль");
        }
        catch (InvalidPasswordException)
        {
            return Unauthorized<AuthentificateResponse>("Неверный логин или пароль");
        }
    }

    [HttpDelete]
    [Route("logout")]
    [SwaggerResponse(200, "Выход выполнен успешно", typeof(LogOutResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult<ApiResponse<LogOutResponse>>> LogOutAsync([FromBody] LogOutRequest request)
    {
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cancellationTokenSource.CancelAfter(Constants.BaseTimeout);

        if (!ValidateRequest<LogOutRequest, LogOutResponse>(request, out var validationError))
            return validationError!;

        var token = HttpContext.User.FindFirst("User")?.Value;
        if (!string.IsNullOrEmpty(token))
            await _service.RevokeAsync(token, cancellationTokenSource.Token);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Success(new LogOutResponse(), "Выход выполнен успешно");
    }
}
