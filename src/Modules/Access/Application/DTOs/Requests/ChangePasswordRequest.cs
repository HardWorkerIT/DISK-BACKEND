using Common.Abstracts;
using System.ComponentModel.DataAnnotations;

namespace Access.Application.DTOs.Requests;

public sealed class ChangePasswordRequest : BaseRequest
{
    [Required, MinLength(8), MaxLength(128)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(12), MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
