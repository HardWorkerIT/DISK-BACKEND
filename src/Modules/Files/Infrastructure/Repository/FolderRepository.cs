using Common.Abstracts;
using Common.Infrastructure.Repositories;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Infrastructure.Repository;

public class FolderRepository(IDbContext context) : GenericRepository<Folder>(context) { }
