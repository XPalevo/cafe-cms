namespace Common.Data.Repository;

using System.Linq.Expressions;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

public class BaseRepository<TEntity, TContext> : IBaseRepository<TEntity>
    where TEntity : BaseEntity
    where TContext : DbContext
{
    private readonly TContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    protected BaseRepository(TContext context)
    {
        this._context = context;
        this._dbSet = this._context.Set<TEntity>();
    }

    public async Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this._dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await this._dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await this._dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await this._dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await this._dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        
        if (entity == null)
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} with id {id} not found.");

        updateAction(entity);
        this._context.Entry(entity).State = EntityState.Modified;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        
        if (entity == null)
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} with id {id} not found.");

        this._dbSet.Remove(entity);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await this._dbSet.CountAsync(cancellationToken);
        else
            return await this._dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await this._dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await this._context.SaveChangesAsync(cancellationToken);
    }
}
