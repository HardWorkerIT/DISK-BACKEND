using App.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace DiskApi.Tests;

public sealed class LocalFileStorageTests
{
    [Fact]
    public async Task StoresReadsListsAndDeletesContentWithinRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"diskapi-storage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var storage = new LocalFileStorage(new TestEnvironment(root));
            await using var source = new MemoryStream("payload"u8.ToArray());
            await storage.StoreAsync("sample.bin", source, CancellationToken.None);

            await using var read = await storage.OpenReadAsync("sample.bin", CancellationToken.None);
            Assert.NotNull(read);
            using var result = new MemoryStream();
            await read!.CopyToAsync(result);
            await read.DisposeAsync();
            Assert.Equal("payload", System.Text.Encoding.UTF8.GetString(result.ToArray()));
            Assert.Contains(await storage.ListAsync(CancellationToken.None), item => item.Key == "sample.bin");

            await storage.DeleteAsync("sample.bin", CancellationToken.None);
            Assert.Null(await storage.OpenReadAsync("sample.bin", CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RejectsKeysThatEscapeStorageRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"diskapi-storage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var storage = new LocalFileStorage(new TestEnvironment(root));
            await Assert.ThrowsAsync<InvalidOperationException>(() => storage.OpenReadAsync("..\\outside.bin", CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "DiskApi.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
