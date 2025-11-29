using Common.Abstracts;
using Users.Domain.Enums;

namespace Users.Domain.Entities;

public class User : BaseEntity
{
    public new long Id { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int RoleId { get; set; }
}