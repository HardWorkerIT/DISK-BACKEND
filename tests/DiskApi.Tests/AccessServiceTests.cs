using Access.Application.Services;
using Access.Domain.Entities;
using Common.Abstracts;
using Common.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Xunit;

namespace DiskApi.Tests;

public class AccessServiceTests
{
    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options), IDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<AccessToken>();
        }
    }

    [Fact]
    public async Task LoginUpgradesLegacyHashAndLogoutRevokesToken()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Set<User>().Add(new User { Login = "legacy", Password = EncryptExtension.EncryptSHA1("password"), RoleId = 3 });
        await context.SaveChangesAsync();

        var service = new AccessService(context);
        var token = await service.AuthentificateAsync("legacy", "password", CancellationToken.None);
        var user = await context.Set<User>().SingleAsync();
        Assert.StartsWith("pbkdf2-sha256$", user.Password);
        Assert.True(PasswordHash.Verify("password", user.Password, out var needsUpgrade));
        Assert.False(needsUpgrade);

        await service.ChangePasswordAsync(user.Id, "password", "A much stronger new password 123!", CancellationToken.None);
        user = await context.Set<User>().SingleAsync();
        Assert.True(PasswordHash.Verify("A much stronger new password 123!", user.Password, out _));
        var savedToken = await context.Set<AccessToken>().SingleAsync();
        Assert.NotNull(savedToken.DeletedAt);

        await Assert.ThrowsAsync<Common.Exceptions.InvalidPasswordException>(
            () => service.AuthentificateAsync("legacy", "password", CancellationToken.None));
        var renewedToken = await service.AuthentificateAsync("legacy", "A much stronger new password 123!", CancellationToken.None);
        Assert.NotEqual(token.Token, renewedToken.Token);
    }
}
