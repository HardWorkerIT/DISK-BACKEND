using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions;

public static class EncryptExtension
{
    public static string EncryptSHA1(string text)
    {
        return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(text)));
    }
}