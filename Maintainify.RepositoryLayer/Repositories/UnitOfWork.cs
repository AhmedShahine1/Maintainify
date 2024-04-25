using Maintainify.Core;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.RepositoryLayer.Interfaces;

namespace Maintainify.RepositoryLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IBaseRepository<ApplicationUser> Users { get; private set; }
        public IBaseRepository<ApplicationRole> Roles { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new BaseRepository<ApplicationUser>(_context);
            Roles = new BaseRepository<ApplicationRole>(_context);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
