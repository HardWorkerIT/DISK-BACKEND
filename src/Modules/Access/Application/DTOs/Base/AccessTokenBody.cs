using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.DTOs.Base;

public class AccessTokenBody
{
    public long UserId { get; set; }
    public string? Token { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? ExpiredAt { get; set; }
}
