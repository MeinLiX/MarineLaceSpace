namespace MarineLaceSpace.Interfaces.Repositories;

public interface IRepository<T> : IRepository<T, string> where T : class
{
};

public interface IRepository<T, IDTYPE> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(IDTYPE id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(IDTYPE id);
}
