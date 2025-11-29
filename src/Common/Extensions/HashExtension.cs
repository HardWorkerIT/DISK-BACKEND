using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions;

public static class HashExtension
{
    public static string GenerateHash()
    {
        return Guid.NewGuid().ToString();
    }
}
