using Common.Extensions;
using Xunit;

namespace DiskApi.Tests;

public class PasswordHashTests
{
    [Fact]
    public void NewHashesUseDistinctSaltsAndVerifyOnlyTheCorrectPassword()
    {
        var first = PasswordHash.Create("correct horse battery staple");
        var second = PasswordHash.Create("correct horse battery staple");

        Assert.NotEqual(first, second);
        Assert.True(PasswordHash.Verify("correct horse battery staple", first, out var needsUpgrade));
        Assert.False(needsUpgrade);
        Assert.False(PasswordHash.Verify("wrong password", first, out _));
    }

    [Fact]
    public void LegacySha1IsAcceptedAndMarkedForUpgrade()
    {
        var legacy = EncryptExtension.EncryptSHA1("legacy password").ToLowerInvariant();
        Assert.True(PasswordHash.Verify("legacy password", legacy, out var needsUpgrade));
        Assert.True(needsUpgrade);
        Assert.False(PasswordHash.Verify("wrong", legacy, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("pbkdf2-sha256$210000$bad$bad")]
    [InlineData("pbkdf2-sha256$999999999$YWJj$YWJj")]
    public void InvalidHashesAreRejected(string storedHash)
    {
        Assert.False(PasswordHash.Verify("password", storedHash, out _));
    }
}
