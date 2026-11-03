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

        if (!PasswordHash.Verify(password, user.Password, out var needsUpgrade))
        {
            throw new InvalidPasswordException("Неверный пароль");
        }

        if (needsUpgrade)
        {
            user.Password = PasswordHash.Create(password);
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        // Если мы дошли сюда, значит пароль правильный
        AccessToken accessToken = new()
        {
            UserId = user.Id,
            Token = HashExtension.GenerateHash(),
            ExpiredAt = DateTimeOffset.UtcNow.AddMonths(6),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return await _tokenRepository.CreateAsync(accessToken, cancellationToken)
            ?? throw new InvalidOperationException("Не удалось создать токен доступа");
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        var accessToken = await _tokenRepository.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
        if (accessToken is not null)
            await _tokenRepository.RemoveAsync(accessToken, cancellationToken);
    }

    public async Task ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken)
            ?? throw new UserNotFoundException("Пользователь не найден");
        if (!PasswordHash.Verify(currentPassword, user.Password, out _))
            throw new InvalidPasswordException("Текущий пароль неверен");

        user.Password = PasswordHash.Create(newPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var activeTokens = await _tokenRepository.GetAsync(x => x.UserId == userId, cancellationToken);
        foreach (var token in activeTokens)
            await _tokenRepository.RemoveAsync(token, cancellationToken);
    }

    public async Task<IReadOnlyList<AccessToken>> GetActiveSessionsAsync(long userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var sessions = await _tokenRepository.GetAsync(
            x => x.UserId == userId && x.ExpiredAt > now,
            cancellationToken);
        return sessions.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task<bool> RevokeSessionAsync(long userId, long sessionId, CancellationToken cancellationToken)
    {
        var session = await _tokenRepository.FirstOrDefaultAsync(
            x => x.Id == sessionId && x.UserId == userId,
            cancellationToken);
        if (session is null)
            return false;

        await _tokenRepository.RemoveAsync(session, cancellationToken);
        return true;
    }

    public async Task RevokeAllSessionsAsync(long userId, CancellationToken cancellationToken)
    {
        await _tokenRepository.RemoveWhereAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<UserDTOWithoutPassword> IdentifyAsync(string token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var now = DateTimeOffset.UtcNow;
        AccessToken? accessToken = await _tokenRepository.FirstOrDefaultAsync(
           x => x.Token == token && x.ExpiredAt > now && x.DeletedAt == default, cancellationToken);

        ArgumentNullException.ThrowIfNull(accessToken);

        User? user = await _userRepository.FindByIdAsync(accessToken.UserId, cancellationToken);

        ArgumentNullException.ThrowIfNull(user);

        return UserMapper.GetBodyWithoutPassword(user);
    }
}
