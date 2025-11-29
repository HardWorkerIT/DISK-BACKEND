using Common.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Domain.Entities;

public class AccessToken : BaseEntity
{
    public new long Id { get; set; }
    public long UserId { get; set; }
    public string? Token { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? ExpiredAt { get; set; }
}