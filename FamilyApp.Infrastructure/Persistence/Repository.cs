using FamilyApp.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Infrastructure.Persistence;

public class Repository : IRepository
{
    private readonly FamilyAppDbContext _context;

    public Repository(FamilyAppDbContext context)
    {
        _context = context;
    }

    public async Task<T> CreateAsync<T>(T entity) where T : class
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync<T>(T entity) where T : class
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<T>> SelectAll<T>() where T : class
        => await _context.Set<T>().AsNoTracking().ToListAsync();

    public async Task<T?> SelectById<T>(int id) where T : class
        => await _context.Set<T>().FindAsync(id);

    public async Task UpdateAsync<T>(T entity) where T : class
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
}
