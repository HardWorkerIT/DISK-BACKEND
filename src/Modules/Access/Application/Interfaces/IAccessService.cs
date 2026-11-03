using Access.Application.DTOs.Base;
using Access.Application.DTOs.Requests;
using Access.Application.DTOs.Responses;
using Access.Domain.Entities;
using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.Interfaces;

public interface IAccessService
{
    Task<AccessToken> AuthentificateAsync(string? login, string? password, CancellationToken cancellationToken);
    Task<UserDTOWithoutPassword> IdentifyAsync(string token, CancellationToken cancellationToken);
    Task RevokeAsync(string token, CancellationToken cancellationToken);
    Task ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccessToken>> GetActiveSessionsAsync(long userId, CancellationToken cancellationToken);
    Task<bool> RevokeSessionAsync(long userId, long sessionId, CancellationToken cancellationToken);
    Task RevokeAllSessionsAsync(long userId, CancellationToken cancellationToken);
}
