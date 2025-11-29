using Common.Abstracts;
using Common.Infrastructure.Repositories;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Files.Domain.Entities.File;

namespace Files.Infrastructure.Repository;

public class FileRepository(IDbContext context) : GenericRepository<File>(context) { }
