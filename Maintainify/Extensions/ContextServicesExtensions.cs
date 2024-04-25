using Maintainify.Core;
using Maintainify.RepositoryLayer.Interfaces;
using Maintainify.RepositoryLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Maintainify.Extensions
{
    public static class ContextServicesExtensions
    {
        public static IServiceCollection AddContextServices(this IServiceCollection services, IConfiguration config)
        {

            //- context && json services
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("url")));//,b => b.MigrationsAssembly(typeof(ApplicationContext).Assembly.FullName)).UseLazyLoadingProxies());
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
            services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // IBaseRepository && IUnitOfWork Service
            //services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>)); // only Repository
            services.AddTransient<IUnitOfWork, UnitOfWork>(); // Repository and UnitOfWork

            return services;
        }

    }
}
