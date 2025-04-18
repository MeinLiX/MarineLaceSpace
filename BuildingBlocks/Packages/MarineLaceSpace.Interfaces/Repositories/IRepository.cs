namespace MarineLaceSpace.Interfaces.Repositories;

public interface IRepository<T> : IRepository<T, string> where T : class
{
};

public interface IRepository<T, IDTYPE> where T : class
{
    Task<T> GetByIdAsync(IDTYPE entityId);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(IDTYPE entityId);
}
