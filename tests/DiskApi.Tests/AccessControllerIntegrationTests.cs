using Access.Application.Services;
using Access.Domain.Entities;
using Access.Presentation;
using App.Infrastructure.RateLimiting;
using Common.Abstracts;
using Common.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Users.Domain.Entities;
using Xunit;

namespace DiskApi.Tests;

public sealed class AccessControllerIntegrationTests
{
    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options), IDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<AccessToken>().Property(token => token.ExpiredAt).HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.Ticks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        }
    }

    [Fact]
    public async Task InvalidCredentialsReturnGenericUnauthorizedResponse()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var app = CreateApplication(connection);
        await EnsureDatabaseAsync(connection);
        await app.StartAsync();
        using var client = app.GetTestClient();

        using var response = await client.PostAsJsonAsync("/api/v1/access/authentificate", new { login = "missing", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Неверный логин или пароль", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ValidCredentialsReturnSuccessAndAuthenticationCookie()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await EnsureDatabaseAsync(connection);
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using (var seedContext = new TestContext(options))
        {
            seedContext.Set<User>().Add(new User
            {
                Id = 1,
                Login = "admin",
                Email = "admin@example.test",
                Username = "Admin",
                Password = PasswordHash.Create("StrongPass123!"),
                RoleId = 3
            });
            await seedContext.SaveChangesAsync();
        }

        await using var app = CreateApplication(connection);
        await app.StartAsync();
        using var client = app.GetTestClient();

        using var response = await client.PostAsJsonAsync("/api/v1/access/authentificate", new { login = "admin", password = "StrongPass123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie => cookie.StartsWith("AuthCookie=", StringComparison.Ordinal));
    }

    private static WebApplication CreateApplication(SqliteConnection connection)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<TestContext>(options => options.UseSqlite(connection));
        builder.Services.AddScoped<IDbContext>(provider => provider.GetRequiredService<TestContext>());
        builder.Services.AddScoped<Access.Application.Interfaces.IAccessService, AccessService>();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => options.Cookie.Name = "AuthCookie");
        builder.Services.AddAuthorization();
        builder.Services.AddAuthenticationRateLimiting();
        builder.Services.AddControllers().AddApplicationPart(typeof(AccessController).Assembly);

        var app = builder.Build();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    private static async Task EnsureDatabaseAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();
    }
}
