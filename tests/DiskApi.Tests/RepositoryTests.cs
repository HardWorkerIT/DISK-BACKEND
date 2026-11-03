using Common.Abstracts;
using Common.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DiskApi.Tests;

public class RepositoryTests
{
    private sealed class Item : BaseEntity
    {
        public string Category { get; set; } = "";
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options), IDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Item>();
    }

    [Fact]
    public async Task FilteredPagesAreOrderedAndExcludeDeletedItems()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Set<Item>().AddRange(
            new Item { Id = 1, Category = "a" },
            new Item { Id = 2, Category = "b" },
            new Item { Id = 3, Category = "a" },
            new Item { Id = 4, Category = "a", DeletedAt = DateTimeOffset.UtcNow },
            new Item { Id = 5, Category = "a" });
        await context.SaveChangesAsync();

        var repository = new GenericRepository<Item>(context);
        var (items, count) = await repository.GetWithCountAsync(x => x.Category == "a", 2, 2, CancellationToken.None);

        Assert.Equal(3, count);
        Assert.Equal(new long[] { 5 }, items.Select(x => x.Id));
    }

    [Fact]
    public async Task DeletedItemsCanBeListedAndRestored()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Set<Item>().Add(new Item { Id = 7, Category = "trash", DeletedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();
        var repository = new GenericRepository<Item>(context);

        var (deletedItems, deletedCount) = await repository.GetDeletedWithCountAsync(20, 1, CancellationToken.None);
        Assert.Equal(1, deletedCount);
        Assert.Equal(7, Assert.Single(deletedItems).Id);

        var restored = await repository.RestoreAsync(7, CancellationToken.None);
        Assert.NotNull(restored);
        var (activeItems, activeCount) = await repository.GetWithCountAsync(20, 1, CancellationToken.None);
        Assert.Equal(1, activeCount);
        Assert.Equal(7, Assert.Single(activeItems).Id);
    }

    [Fact]
    public async Task FilteredPagesCanBeSortedWithStableIdTieBreaker()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Set<Item>().AddRange(
            new Item { Id = 3, Category = "b" },
            new Item { Id = 1, Category = "a" },
            new Item { Id = 2, Category = "a" });
        await context.SaveChangesAsync();
        var repository = new GenericRepository<Item>(context);

        var (items, totalCount) = await repository.GetWithCountAsync(
            x => true,
            10,
            1,
            x => x.Category,
            descending: true,
            CancellationToken.None);

        Assert.Equal(3, totalCount);
        Assert.Equal(new long[] { 3, 1, 2 }, items.Select(x => x.Id));
    }
}
