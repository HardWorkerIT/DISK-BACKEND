using Common.Abstracts;
using System;

namespace App.Infrastructure.Data;

public class AppFile : BaseEntity
{
    public new long Id { get; set; }
    public Guid Uuid { get; set; }
    public string? Filename { get; set; }
    public string? Path { get; set; }
}