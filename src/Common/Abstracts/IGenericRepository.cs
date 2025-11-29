using Common.Abstracts;

namespace Common.Infrastructure.Abstracts;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> CreateAsync(TEntity item, CancellationToken cancellationToken);
    ValueTask<TEntity?> FindByIdAsync(long id, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAsync(int perInPage, int page, CancellationToken cancellationToken);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(int perInPage, int page, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAsync(Func<TEntity, bool> predicate, int perInPage, int page, CancellationToken cancellationToken);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetWithCountAsync(Func<TEntity, bool> predicate, int perInPage, int page, CancellationToken cancellationToken);
    Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken);
    Task RemoveAsync(TEntity item, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity item, CancellationToken cancellationToken);
}
