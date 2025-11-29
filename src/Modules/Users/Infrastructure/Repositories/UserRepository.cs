using Common.Abstracts;
using Common.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Users.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>
{
    public UserRepository(IDbContext context) : base(context)
    {
    }
}
