using Common.Infrastructure.Abstracts;
using Microsoft.EntityFrameworkCore;
using Common.Abstracts;

namespace Common.Infrastructure.Repositories;

public class GenericRepository<TEntity>(IDbContext context) : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public Task<IEnumerable<TEntity>> GetAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        return Task.Run(() => _dbSet.Where(x => x.DeletedAt == default).Skip(page - 1).Take(perInPage).AsEnumerable(), cancellationToken);
    }

    public Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var query = _dbSet.Where(x => x.DeletedAt == default);
            var totalCount = query.Count();
            var items = query.Skip(page - 1).Take(perInPage).AsEnumerable();
            return (items, totalCount);
        }, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken)
    {
        return Task.Run(() => _dbSet.Where(x => x.DeletedAt == default).AsEnumerable().Where(predicate).AsEnumerable(), cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetAsync(Func<TEntity, bool> predicate, int perInPage, int page, CancellationToken cancellationToken)
    {
        return Task.Run(() => _dbSet.Where(x => x.DeletedAt == default).AsEnumerable().Where(predicate).Skip(page - 1).Take(perInPage).AsEnumerable(), cancellationToken);
    }

    public Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(Func<TEntity, bool> predicate, int perInPage, int page, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var allItems = _dbSet.Where(x => x.DeletedAt == default).AsEnumerable().Where(predicate).ToList();
            var totalCount = allItems.Count;
            var items = allItems.Skip(page - 1).Take(perInPage).AsEnumerable();
            return (items, totalCount);
        }, cancellationToken);
    }

    public async ValueTask<TEntity?> FindByIdAsync(long id, CancellationToken cancellationToken)
    {
        TEntity? entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null || entity.DeletedAt != default)
        {
            return null;
        }

        return entity;
    }

    public async Task<TEntity?> CreateAsync(TEntity item, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(item, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return item;
    }

    public async Task UpdateAsync(TEntity item, CancellationToken cancellationToken)
    {
        TEntity? local = _dbSet.Local.FirstOrDefault(entity => entity.Id == item.Id);

        if (local is not null)
        {
            context.Entry(local).State = EntityState.Detached;
        }

        _dbSet.Attach(item);
        context.Entry(item).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(TEntity item, CancellationToken cancellationToken)
    {
        item.DeletedAt = DateTimeOffset.UtcNow;
        context.Entry(item).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken)
    {
        return Task.Run(() => _dbSet.Where(x => x.DeletedAt == default).AsEnumerable().FirstOrDefault(predicate), cancellationToken);
    }
}