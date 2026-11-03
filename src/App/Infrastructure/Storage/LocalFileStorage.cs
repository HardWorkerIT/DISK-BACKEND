namespace App.Infrastructure.Storage;

public sealed class LocalFileStorage(IWebHostEnvironment environment) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "uploads", "files"));

    public async Task StoreAsync(string key, Stream content, CancellationToken cancellationToken)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(_root);
        await using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
        await content.CopyToAsync(destination, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(key);
        Stream? stream = System.IO.File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan)
            : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(key);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoredObject>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Directory.Exists(_root))
            return Task.FromResult<IReadOnlyList<StoredObject>>([]);

        IReadOnlyList<StoredObject> objects = Directory.EnumerateFiles(_root, "*", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .Select(file => new StoredObject(file.Name, file.LastWriteTimeUtc))
            .ToArray();
        return Task.FromResult(objects);
    }

    private string ResolvePath(string key)
    {
        var path = Path.GetFullPath(Path.Combine(_root, key));
        var prefix = _root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage key resolves outside the configured root");
        return path;
    }
}
