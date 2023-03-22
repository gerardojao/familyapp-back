using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyApp.Controllers;
using FamilyApp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FamilyApp.Data
{
    public class Repository<TDbContext> : IRepository where TDbContext : DbContext
    {
        private readonly IConfiguration _appsettings;
        private readonly dbContext _context;
        private TDbContext _dbContext;

        public Repository(TDbContext context, IConfiguration appsettings, dbContext appcontext)
        {
            this._dbContext = context;
            this._appsettings = appsettings;
            this._context = appcontext;
        }
        
        public async Task<int> CreateAsync<T>(T entity) where T : class
        {
            this._dbContext.Set<T>().Add(entity);
            await this._dbContext.SaveChangesAsync();
            var IdProperty = entity.GetType().GetProperty("Id").GetValue(entity, null);
            return (int)IdProperty;
        }
        
        public async Task<List<T>> SelectAll<T>() where T : class
        {
            return await this._dbContext.Set<T>().ToListAsync();
        }

        //public async Task DeleteAsync<T>(T entity) where T : class
        //{
        //    this._dbContext.Set<T>().Update(entity);
        //    _ = await this._dbContext.SaveChangesAsync();
        //}

        //public async Task<T> SelectById<T>(int Id) where T : class
        //{
        //    return await this._dbContext.Set<T>().FindAsync(Id);
        //}

        //public async Task<List<T>> SelectByRange<T>(int pageNumber, int pagSize) where T : class
        //{
        //    return await this._dbContext
        //            .Set<T>()
        //            .Skip((pageNumber - 1) * pagSize)
        //            .Take(pagSize)
        //            .ToListAsync();
        //}

        //public async Task UpdateAsync<T>(T entity) where T : class
        //{
        //    this._dbContext.Set<T>().Update(entity);
        //    _ = await this._dbContext.SaveChangesAsync();
        //}

       
   }
}
