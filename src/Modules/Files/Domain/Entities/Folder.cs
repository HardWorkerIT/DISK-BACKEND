using Common.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Domain.Entities;

public class Folder : BaseEntity
{
    public long? ParentId { get; set; }
    public required string Name { get; set; }
}
