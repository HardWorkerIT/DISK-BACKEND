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
}
