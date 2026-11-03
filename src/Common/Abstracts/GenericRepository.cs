using Common.Infrastructure.Abstracts;
using Microsoft.EntityFrameworkCore;
using Common.Abstracts;
using System.Linq.Expressions;

namespace Common.Infrastructure.Repositories;

public class GenericRepository<TEntity>(IDbContext context) : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> GetAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.DeletedAt == default)
            .OrderBy(x => x.Id)
            .Skip((page - 1) * perInPage)
            .Take(perInPage)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking().Where(x => x.DeletedAt == default);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Id).Skip((page - 1) * perInPage).Take(perInPage).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetDeletedWithCountAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking().Where(x => x.DeletedAt != default);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Id).Skip((page - 1) * perInPage).Take(perInPage).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking().Where(x => x.DeletedAt == default).Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int perInPage, int page, CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.DeletedAt == default)
            .Where(predicate)
            .OrderBy(x => x.Id)
            .Skip((page - 1) * perInPage)
            .Take(perInPage)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(Expression<Func<TEntity, bool>> predicate, int perInPage, int page, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking().Where(x => x.DeletedAt == default).Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Id).Skip((page - 1) * perInPage).Take(perInPage).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync<TOrder>(Expression<Func<TEntity, bool>> predicate, int perInPage, int page, Expression<Func<TEntity, TOrder>> orderBy, bool descending, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking().Where(x => x.DeletedAt == default).Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        IOrderedQueryable<TEntity> orderedQuery = descending
            ? query.OrderByDescending(orderBy)
            : query.OrderBy(orderBy);
        var items = await orderedQuery
            .ThenBy(x => x.Id)
            .Skip((page - 1) * perInPage)
            .Take(perInPage)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
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
        var tracked = _dbSet.Local.FirstOrDefault(entity => entity.Id == item.Id);
        if (tracked is null)
        {
            _dbSet.Attach(item);
            tracked = item;
        }

        tracked.DeletedAt = DateTimeOffset.UtcNow;
        context.Entry(tracked).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> RemoveWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var deletedAt = DateTimeOffset.UtcNow;
        return _dbSet
            .Where(x => x.DeletedAt == default)
            .Where(predicate)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DeletedAt, deletedAt), cancellationToken);
    }

    public async Task<TEntity?> RestoreAsync(long id, CancellationToken cancellationToken)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt != default, cancellationToken);
        if (entity is null)
            return null;

        entity.DeletedAt = default;
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return _dbSet.Where(x => x.DeletedAt == default).Where(predicate).FirstOrDefaultAsync(cancellationToken);
    }
}
