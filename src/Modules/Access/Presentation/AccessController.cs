using Access.Application.DTOs.Requests;
using Access.Application.DTOs.Responses;
using Access.Application.Interfaces;
using Access.Domain.Entities;
using Common.Abstracts;
using Common.Constants;
using Common.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    [Route("identify")]
    [SwaggerResponse(200, "Идентификация успешна", typeof(IdentifyResponse))]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult<ApiResponse<IdentifyResponse>>> IdentifyAsync()
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result.Succeeded is false || result.Principal is null)
            {
                return NotFound<IdentifyResponse>("User is not authenticated");
            }

            var userClaim = result.Principal.FindFirst("User");

            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            {
                return NotFound<IdentifyResponse>("User claim not found");
            }

            UserDTOWithoutPassword? userBody = await _service.IdentifyAsync(userClaim.Value, cancellationTokenSource.Token);

            if (userBody is null)
            {
                return NotFound<IdentifyResponse>("User is not found");
            }

            return Success(new IdentifyResponse
                (userBody.Id, userBody.Login, userBody.Email, userBody.FirstName, userBody.MiddleName, userBody.LastName, userBody.RoleId), "Identify is successfull");
        }

        catch (Exception)
        {
            return NotFound<IdentifyResponse>("User is not found");
        }
    }

    [HttpPost]
    [Route("authentificate")]
    [SwaggerResponse(200, "Аутентификация успешна", typeof(AuthentificateResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Пользователь не найден")]
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

        catch (Exception)
        {
            return NotFound<AuthentificateResponse>("User is not found");
        }
    }

    [HttpDelete]
    [Route("logout")]
    [SwaggerResponse(200, "Выход выполнен успешно", typeof(LogOutResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult<ApiResponse<LogOutResponse>>> LogOutAsync([FromBody] LogOutRequest request)
    {
        try
        {
            using CancellationTokenSource cancellationTokenSource = new(Constants.BaseTimeout);

            if (!ValidateRequest<LogOutRequest, LogOutResponse>(request, out var validationError))
                return validationError!;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Success(new LogOutResponse(), "LogOut is successfull");
        }

        catch (Exception)
        {
            return NotFound<LogOutResponse>("User is not found");
        }
    }
}