namespace App.Infrastructure.Storage;

public interface IFileStorage
{
    Task StoreAsync(string key, Stream content, CancellationToken cancellationToken);
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);
    Task DeleteAsync(string key, CancellationToken cancellationToken);
    Task<IReadOnlyList<StoredObject>> ListAsync(CancellationToken cancellationToken);
}

public sealed record StoredObject(string Key, DateTimeOffset LastModifiedAt);
