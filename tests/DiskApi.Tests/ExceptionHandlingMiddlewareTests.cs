using Common.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DiskApi.Tests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task UnhandledErrorsReturnSafeJsonAndEchoCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "trace_123-abc";
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("secret database details"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("trace_123-abc", context.Response.Headers["X-Correlation-ID"]);
        Assert.Equal(context.TraceIdentifier, context.Response.Headers["X-Trace-ID"]);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("INTERNAL_SERVER_ERROR", body);
        Assert.Contains("trace_123-abc", body);
        Assert.DoesNotContain("secret database details", body);
    }

    [Fact]
    public async Task InvalidCorrelationHeaderIsReplaced()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "bad\r\nheader";
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(_ => Task.CompletedTask,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(context.TraceIdentifier, context.Response.Headers["X-Correlation-ID"]);
        Assert.Equal(context.TraceIdentifier, context.Response.Headers["X-Trace-ID"]);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
}
