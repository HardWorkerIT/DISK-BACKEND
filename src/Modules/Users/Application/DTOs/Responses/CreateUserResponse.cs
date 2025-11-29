using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs.Responses;

[SwaggerSchema(Title = "CreateUserResponse", Description = "Ответ при создании пользователя")]
public class CreateUserResponse(
    long id,
    string? login,
    string? email,
    string? firstName,
    string? middleName,
    string? lastName,
    int roleId)
    : IResponse
{
    public long Id { get; set; } = id;
    public string? Login { get; set; } = login;
    public string? Email { get; set; } = email;
    public string? FirstName { get; set; } = firstName;
    public string? MiddleName { get; set; } = middleName;
    public string? LastName { get; set; } = lastName;
    public int RoleId { get; set; } = roleId;
}