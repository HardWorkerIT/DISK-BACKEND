using App.Infrastructure.Data;
using App.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Services;

public sealed class TrashCleanupService(IServiceScopeFactory scopeFactory, ILogger<TrashCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan OrphanGracePeriod = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await PurgeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Scheduled trash cleanup failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var cutoff = DateTimeOffset.UtcNow.Subtract(Retention);
        var deletedFiles = await db.Files
            .Where(file => file.DeletedAt != null && file.DeletedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (deletedFiles.Count > 0)
        {
            var uuids = deletedFiles.Select(file => file.Uuid).ToArray();
            var storedFiles = await db.AppFiles.Where(file => uuids.Contains(file.Uuid)).ToListAsync(cancellationToken);
            foreach (var storedFile in storedFiles)
            {
                if (!string.IsNullOrWhiteSpace(storedFile.Path))
                {
                    try
                    {
                        await storage.DeleteAsync(storedFile.Path, cancellationToken);
                    }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
                    {
                        logger.LogWarning(exception, "Could not remove expired upload {FileId}", storedFile.Id);
                        continue;
                    }
                }

                db.AppFiles.Remove(storedFile);
            }

            var removableUuids = deletedFiles.Select(file => file.Uuid).ToHashSet();
            var retainedUuids = db.ChangeTracker.Entries<AppFile>()
                .Where(entry => entry.State != EntityState.Deleted)
                .Select(entry => entry.Entity.Uuid)
                .ToHashSet();
            removableUuids.ExceptWith(retainedUuids);
            var purgeableRecords = deletedFiles.Where(file => removableUuids.Contains(file.Uuid)).ToList();
            db.Files.RemoveRange(purgeableRecords);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Purged {PurgedCount} expired file records", purgeableRecords.Count);
        }

        var purgedFolders = 0;
        while (true)
        {
            var leafFolders = await db.Folders
                .Where(folder => folder.DeletedAt != null && folder.DeletedAt < cutoff)
                .Where(folder => !db.Folders.Any(child => child.ParentId == folder.Id))
                .Where(folder => !db.Files.Any(file => file.FolderId == folder.Id))
                .ToListAsync(cancellationToken);
            if (leafFolders.Count == 0)
                break;

            db.Folders.RemoveRange(leafFolders);
            purgedFolders += await db.SaveChangesAsync(cancellationToken);
        }

        if (purgedFolders > 0)
            logger.LogInformation("Purged {PurgedCount} expired folder records", purgedFolders);

        await ReconcileOrphanedObjectsAsync(db, storage, cancellationToken);
    }

    private async Task ReconcileOrphanedObjectsAsync(AppDbContext db, IFileStorage storage, CancellationToken cancellationToken)
    {
        var knownFiles = await db.AppFiles
            .Select(file => new { file.Path, file.Uuid, file.Filename })
            .ToListAsync(cancellationToken);
        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in knownFiles)
        {
            if (!string.IsNullOrWhiteSpace(file.Path))
                knownNames.Add(Path.GetFileName(file.Path));
            knownNames.Add($"{file.Uuid}{Path.GetExtension(file.Filename)}");
        }
        var cutoff = DateTimeOffset.UtcNow.Subtract(OrphanGracePeriod);
        var orphanCount = 0;

        foreach (var storedObject in await storage.ListAsync(cancellationToken))
        {
            if (storedObject.LastModifiedAt > cutoff || knownNames.Contains(storedObject.Key))
                continue;

            try
            {
                await storage.DeleteAsync(storedObject.Key, cancellationToken);
                orphanCount++;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                logger.LogWarning(exception, "Could not remove orphaned upload object {StorageKey}", storedObject.Key);
            }
        }

        if (orphanCount > 0)
            logger.LogInformation("Removed {OrphanCount} orphaned upload objects", orphanCount);
    }

}
