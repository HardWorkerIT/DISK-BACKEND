using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Base;

public class FolderDTO
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string? Name { get; set; }
}
