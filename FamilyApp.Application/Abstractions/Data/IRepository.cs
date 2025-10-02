namespace FamilyApp.Application.Abstractions.Data;

public interface IRepository
{
    Task<T> CreateAsync<T>(T entity) where T : class;
    Task<List<T>> SelectAll<T>() where T : class;
    Task DeleteAsync<T>(T entity) where T : class;
    Task<T?> SelectById<T>(int id) where T : class;
    Task UpdateAsync<T>(T entity) where T : class;
}
