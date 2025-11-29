using Access.Domain.Entities;
using Common.Abstracts;
using Common.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Infrastructure.Repositories;

public class AccessTokenRepository : GenericRepository<AccessToken>
{
    public AccessTokenRepository(IDbContext context) : base(context)
    {
    }
}