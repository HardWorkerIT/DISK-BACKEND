using Access.Application.Services;
using Access.Domain.Entities;
using Common.Abstracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DiskApi.Tests;

public class SessionManagementTests
{
    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options), IDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessToken>();
            modelBuilder.Entity<AccessToken>().Property(x => x.ExpiredAt).HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.Ticks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        }
    }

    [Fact]
    public async Task ActiveSessionsExcludeExpiredAndDeletedTokens()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Set<AccessToken>().AddRange(
            new AccessToken { Id = 1, UserId = 9, Token = "active", ExpiredAt = DateTimeOffset.UtcNow.AddHours(1) },
            new AccessToken { Id = 2, UserId = 9, Token = "expired", ExpiredAt = DateTimeOffset.UtcNow.AddHours(-1) },
            new AccessToken { Id = 3, UserId = 9, Token = "deleted", ExpiredAt = DateTimeOffset.UtcNow.AddHours(1), DeletedAt = DateTimeOffset.UtcNow },
            new AccessToken { Id = 4, UserId = 10, Token = "other-user", ExpiredAt = DateTimeOffset.UtcNow.AddHours(1) });
        await context.SaveChangesAsync();

        var sessions = await new AccessService(context).GetActiveSessionsAsync(9, CancellationToken.None);

        Assert.Equal("active", Assert.Single(sessions).Token);
    }

    [Fact]
    public async Task RevokeSessionIsScopedToItsOwnerAndRevokeAllRemovesRemainingSessions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Set<AccessToken>().AddRange(
            new AccessToken { Id = 1, UserId = 9, Token = "first", ExpiredAt = DateTimeOffset.UtcNow.AddHours(1) },
            new AccessToken { Id = 2, UserId = 9, Token = "second", ExpiredAt = DateTimeOffset.UtcNow.AddHours(1) },
            new AccessToken { Id = 3, UserId = 10, Token = "other-user", ExpiredAt = DateTimeOffset.UtcNow.AddHours(1) });
        await context.SaveChangesAsync();
        var service = new AccessService(context);

        Assert.False(await service.RevokeSessionAsync(9, 3, CancellationToken.None));
        Assert.True(await service.RevokeSessionAsync(9, 1, CancellationToken.None));
        await service.RevokeAllSessionsAsync(9, CancellationToken.None);

        Assert.Empty(await service.GetActiveSessionsAsync(9, CancellationToken.None));
        Assert.Equal("other-user", Assert.Single(await service.GetActiveSessionsAsync(10, CancellationToken.None)).Token);
    }
}
