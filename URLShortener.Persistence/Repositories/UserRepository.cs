using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using URLShortener.Application.Repositories;
using URLShortener.Domain.Entities.User;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(UserModel entity)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(UserModel entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<UserModel> entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<UserModel> entities)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<UserModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<UserModel, bool>>? predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserModel>> FindAsync(Expression<Func<UserModel, bool>> predicate, Func<IQueryable<UserModel>, IQueryable<UserModel>>? include = null, Func<IQueryable<UserModel>, IQueryable<UserModel>>? orderBy = null, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserModel>> GetAllAsync(bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel?> GetAsync(Expression<Func<UserModel, bool>> predicate, Func<IQueryable<UserModel>, IQueryable<UserModel>>? include = null, Func<IQueryable<UserModel>, IQueryable<UserModel>>? orderBy = null, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel?> GetByIdAsync(object id, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public IQueryable<UserModel> Query(bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public void Remove(UserModel entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<UserModel> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(UserModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
