using System.Security.Cryptography;

namespace Common.Extensions;

public static class PasswordHash
{
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Prefix = "pbkdf2-sha256";

    public static string Create(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string? storedHash, out bool needsUpgrade)
    {
        needsUpgrade = false;
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        var parts = storedHash.Split('$');
        if (parts.Length == 4 && parts[0] == Prefix
            && int.TryParse(parts[1], out var iterations)
            && iterations is >= 100_000 and <= 2_000_000)
        {
            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                if (salt.Length != SaltSize || expectedHash.Length != HashSize)
                    return false;

                var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
                needsUpgrade = iterations < Iterations;
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Existing accounts use a SHA-1 hex digest; migrate them on their next successful login.
        try
        {
            var expectedLegacyHash = Convert.FromHexString(storedHash.Trim());
            var actualLegacyHash = SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(password));
            if (expectedLegacyHash.Length != actualLegacyHash.Length
                || !CryptographicOperations.FixedTimeEquals(actualLegacyHash, expectedLegacyHash))
                return false;

            needsUpgrade = true;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
