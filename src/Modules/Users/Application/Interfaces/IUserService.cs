using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs.Requests;
using Users.Domain.Entities;

namespace Users.Application.Interfaces;

public interface IUserService
{
    Task<User?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<User>> SelectAsync(GetUserRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(DeleteUserRequest request, CancellationToken cancellationToken);
}