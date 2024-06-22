using Maintainify.Core;
using Maintainify.Core.Entity.ApplicationData;
//using Maintainify.Core.Entity.EvaluationData;
//using Maintainify.Core.Entity.EvaulationData;
using Maintainify.Core.Entity.OrderData;
using Maintainify.Core.Entity.ProfessionData;
using Maintainify.RepositoryLayer.Interfaces;

namespace Maintainify.RepositoryLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IBaseRepository<ApplicationUser> Users { get; private set; }
        public IBaseRepository<ApplicationRole> Roles { get; private set; }
        //-------------------------------------------------------------------------------------------------
        public IBaseRepository<PathFiles> pathFiles { get; private set; }
        public IBaseRepository<Images> images { get; private set; }
        public IBaseRepository<Profession> Profession { get; private set; }
        //-------------------------------------------------------------------------------------------------
        public IBaseRepository<Order> Orders { get; private set; }
        //public IBaseRepository<EvaluationProvider> evaluationProvider { get; private set; }
        //public IBaseRepository<EvaluationOrder> evaluationOrder { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new BaseRepository<ApplicationUser>(_context);
            Roles = new BaseRepository<ApplicationRole>(_context);
            pathFiles = new BaseRepository<PathFiles>(_context);
            images = new BaseRepository<Images>(_context);
            Profession = new BaseRepository<Profession>(_context);
            Orders = new BaseRepository<Order>(_context);
            //evaluationOrder = new BaseRepository<EvaluationOrder>(_context);
            //evaluationProvider = new BaseRepository<EvaluationProvider>(_context);
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
