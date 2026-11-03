using Microsoft.AspNetCore.Http;
using System.Text;

namespace App.Infrastructure.Storage;

public sealed record UploadValidationFailure(string Code, string Message);

public static class UploadFileValidator
{
    public const long MaxFileSize = 50L * 1024 * 1024;
    public const long MaxRequestSize = 100L * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".docx", ".xlsx", ".pptx", ".zip", ".txt", ".csv", ".md"
    };

    public static string NormalizeFileName(string fileName)
        => Path.GetFileName(fileName.Replace('\\', '/')).Normalize(NormalizationForm.FormC);

    public static async Task<UploadValidationFailure?> ValidateAsync(IFormFileCollection files, CancellationToken cancellationToken)
    {
        if (files.Count == 0)
            return new("NO_FILES", "Файлы не были загружены");
        if (files.Any(file => file.Length > MaxFileSize) || files.Sum(file => file.Length) > MaxRequestSize)
            return new("FILE_TOO_LARGE", "Размер одного файла не должен превышать 50 МБ, а общий размер запроса — 100 МБ");

        foreach (var file in files)
        {
            var name = NormalizeFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(name) || name.Length > 255 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return new("INVALID_FILE_NAME", "Имя файла должно содержать от 1 до 255 допустимых символов");

            var extension = Path.GetExtension(name);
            if (!AllowedExtensions.Contains(extension))
                return new("UNSUPPORTED_FILE_TYPE", $"Формат {extension} не поддерживается");

            if (!await HasExpectedContentAsync(file, extension, cancellationToken))
                return new("INVALID_FILE_CONTENT", $"Содержимое файла {name} не соответствует формату");
        }

        return null;
    }

    private static async Task<bool> HasExpectedContentAsync(IFormFile file, string extension, CancellationToken cancellationToken)
    {
        var header = new byte[16];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        byte[]? signature = extension.ToLowerInvariant() switch
        {
            ".pdf" => "%PDF-"u8.ToArray(),
            ".png" => [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a],
            ".jpg" or ".jpeg" => [0xff, 0xd8, 0xff],
            ".gif" => "GIF8"u8.ToArray(),
            ".docx" or ".xlsx" or ".pptx" or ".zip" => [0x50, 0x4b, 0x03, 0x04],
            _ => null
        };

        if (signature is not null)
            return read >= signature.Length && header.AsSpan(0, signature.Length).SequenceEqual(signature);

        for (var index = 0; index < read; index++)
            if (header[index] == 0 || (header[index] < 0x20 && header[index] is not (0x09 or 0x0a or 0x0d)))
                return false;
        return true;
    }
}
