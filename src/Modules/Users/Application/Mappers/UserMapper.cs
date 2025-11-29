using Common.DTOs;
using Users.Application.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Users.Application.Mappers;

public static class UserMapper
{
    public static User GetModelWithoutPassword(UserDTOWithoutPassword body)
    {
        return new User()
        {
            Id = body.Id,
            Login = body.Login,
            Email = body.Email,
            Username = $"{body.LastName} {body.FirstName} {body.MiddleName}",
            RoleId = body.RoleId,
        };
    }

    public static User GetModelWithoutPassword(UpdateUserRequest request)
    {
        return new User()
        {
            Id = request.Id,
            Login = request.Login,
            Email = request.Email,
            Username = $"{request.LastName} {request.FirstName} {request.MiddleName}",
            RoleId = request.RoleId,
        };
    }

    public static User GetModelWithPassword(UserDTOWithPassword body)
    {
        return new User()
        {
            Id = body.Id,
            Login = body.Login,
            Password = body.Password,
            Email = body.Email,
            Username = $"{body.LastName} {body.FirstName} {body.MiddleName}",
            RoleId = body.RoleId,
        };
    }

    public static User GetModelWithPassword(CreateUserRequest request)
    {
        return new User()
        {
            Id = 0,
            Login = request.Login,
            Password = request.Password,
            Email = request.Email,
            Username = $"{request.LastName} {request.FirstName} {request.MiddleName}",
            RoleId = request.RoleId,
        };
    }

    public static UserDTOWithoutPassword GetBodyWithoutPassword(User model)
    {
        IEnumerable<string> parts = model.Username?.Split() ?? [];

        return new UserDTOWithoutPassword()
        {
            Id = model.Id,
            LastName = parts.ElementAtOrDefault(0),
            FirstName = parts.ElementAtOrDefault(1),
            MiddleName = parts.ElementAtOrDefault(2),
            Email = model.Email,
            Login = model.Login,
            RoleId = model.RoleId,
        };
    }

    public static UserDTOWithPassword GetBodyWithPassword(User model)
    {
        IEnumerable<string> parts = model.Username?.Split() ?? [];

        return new UserDTOWithPassword()
        {
            Id = model.Id,
            LastName = parts.ElementAtOrDefault(0),
            FirstName = parts.ElementAtOrDefault(1),
            MiddleName = parts.ElementAtOrDefault(2),
            Email = model.Email,
            Login = model.Login,
            Password = model.Password,
            RoleId = model.RoleId,
        };
    }
}