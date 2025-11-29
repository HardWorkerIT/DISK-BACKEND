using Access.Application.DTOs.Base;
using Access.Application.DTOs.Requests;
using Access.Application.DTOs.Responses;
using Access.Application.Interfaces;
using Access.Domain.Entities;
using Common.Abstracts;
using Common.DTOs;
using Common.Exceptions;
using Common.Extensions;
using Common.Infrastructure.Abstracts;
using Common.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Users.Application.Mappers;
using Users.Domain.Entities;
using Users.Infrastructure.Repositories;

namespace Access.Application.Services;

public sealed class AccessService(IDbContext context) : IAccessService
{
    private readonly IGenericRepository<AccessToken> _tokenRepository = new GenericRepository<AccessToken>(context);
    private readonly IGenericRepository<User> _userRepository = new GenericRepository<User>(context);

    private Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken)
    {
        return _userRepository.FirstOrDefaultAsync(
            x => x.Login == login && x.DeletedAt == default, cancellationToken);
    }

    public async Task<AccessToken> AuthentificateAsync(string? login, string? password, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // Сначала ищем пользователя по логину
        User? user = await GetUserByLoginAsync(login, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException("Пользователь не найден");
        }

        // Проверяем пароль
        if (string.IsNullOrWhiteSpace(user.Password))
        {
            throw new InvalidPasswordException("Неверный пароль");
        }

        string encryptedPassword;
        try
        {
            encryptedPassword = EncryptExtension.EncryptSHA1(password);
        }
        catch (Exception ex)
        {
            throw new InvalidPasswordException("Ошибка при проверке пароля", ex);
        }

        // Проверяем, что зашифрованный пароль не пустой
        if (string.IsNullOrWhiteSpace(encryptedPassword))
        {
            throw new InvalidPasswordException("Неверный пароль");
        }

        // Сравниваем хеши без учета регистра, так как в БД может быть разный регистр
        // Используем Trim() для удаления возможных пробелов
        string dbPassword = (user.Password ?? string.Empty).Trim();
        string inputPassword = encryptedPassword.Trim();

        if (!string.Equals(dbPassword, inputPassword, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidPasswordException("Неверный пароль");
        }

        // Если мы дошли сюда, значит пароль правильный
        AccessToken accessToken = new()
        {
            UserId = user.Id,
            Token = HashExtension.GenerateHash(),
            ExpiredAt = DateTimeOffset.UtcNow.AddMonths(6),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        try
        {
            return await _tokenRepository.CreateAsync(accessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            // Если ошибка при создании токена, это не ошибка пароля
            // Пробрасываем как есть, чтобы не маскировать реальную проблему
            throw;
        }
    }

    public async Task<UserDTOWithoutPassword> IdentifyAsync(string token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        AccessToken? accessToken = await _tokenRepository.FirstOrDefaultAsync(
           x => x.Token == token && x.ExpiredAt > DateTimeOffset.UtcNow && x.DeletedAt == default, cancellationToken);

        ArgumentNullException.ThrowIfNull(accessToken);

        User? user = await _userRepository.FindByIdAsync(accessToken.UserId, cancellationToken);

        ArgumentNullException.ThrowIfNull(user);

        return UserMapper.GetBodyWithoutPassword(user);
    }
}