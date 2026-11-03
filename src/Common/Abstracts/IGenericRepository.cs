using Common.Abstracts;

namespace Common.Infrastructure.Abstracts;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> CreateAsync(TEntity item, CancellationToken cancellationToken);
    ValueTask<TEntity?> FindByIdAsync(long id, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAsync(int perInPage, int page, CancellationToken cancellationToken);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(int perInPage, int page, CancellationToken cancellationToken);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetDeletedWithCountAsync(int perInPage, int page, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, int perInPage, int page, CancellationToken cancellationToken);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, int perInPage, int page, CancellationToken cancellationToken);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync<TOrder>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, int perInPage, int page, System.Linq.Expressions.Expression<Func<TEntity, TOrder>> orderBy, bool descending, CancellationToken cancellationToken);
    Task<TEntity?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task RemoveAsync(TEntity item, CancellationToken cancellationToken);
    Task<int> RemoveWhereAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity?> RestoreAsync(long id, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity item, CancellationToken cancellationToken);
}
