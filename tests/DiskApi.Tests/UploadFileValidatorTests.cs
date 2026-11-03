using App.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DiskApi.Tests;

public sealed class UploadFileValidatorTests
{
    [Fact]
    public async Task AcceptsMatchingSignatureAndNormalizesDisplayName()
    {
        var files = new FormFileCollection
        {
            CreateFile("folder\\cafe\u0301.pdf", "%PDF-1.7"u8.ToArray())
        };

        var failure = await UploadFileValidator.ValidateAsync(files, CancellationToken.None);

        Assert.Null(failure);
        Assert.Equal("café.pdf", UploadFileValidator.NormalizeFileName(files[0].FileName));
    }

    [Fact]
    public async Task RejectsUnknownFormatsAndMismatchedSignatures()
    {
        Assert.Equal("UNSUPPORTED_FILE_TYPE", (await UploadFileValidator.ValidateAsync(
            new FormFileCollection { CreateFile("payload.exe", [0x4d, 0x5a]) }, CancellationToken.None))?.Code);
        Assert.Equal("INVALID_FILE_CONTENT", (await UploadFileValidator.ValidateAsync(
            new FormFileCollection { CreateFile("payload.png", "%PDF-1.7"u8.ToArray()) }, CancellationToken.None))?.Code);
    }

    private static FormFile CreateFile(string name, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "files", name);
    }
}
