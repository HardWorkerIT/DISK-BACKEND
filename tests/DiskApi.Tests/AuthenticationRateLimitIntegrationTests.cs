using App.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DiskApi.Tests;

public sealed class AuthenticationRateLimitIntegrationTests
{
    [Fact]
    public async Task AuthenticationPolicyAllowsTenRequestsThenReturnsTooManyRequests()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthenticationRateLimiting();

        await using var app = builder.Build();
        app.UseRouting();
        app.UseRateLimiter();
        app.MapPost("/api/v1/access/authentificate", () => Results.Ok())
            .RequireRateLimiting("authentication");
        await app.StartAsync();
        using var client = app.GetTestClient();

        for (var attempt = 0; attempt < 10; attempt++)
        {
            using var response = await client.PostAsync("/api/v1/access/authentificate", content: null);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        using var limitedResponse = await client.PostAsync("/api/v1/access/authentificate", content: null);
        Assert.Equal(System.Net.HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
    }
}
