﻿using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.OrderData;
using Maintainify.Core.Entity.ProfessionData;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Maintainify.RepositoryLayer.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        IBaseRepository<ApplicationUser> Users { get; }
        IBaseRepository<ApplicationRole> Roles { get; }
        IBaseRepository<PathFiles> pathFiles { get; }
        IBaseRepository<Images> images { get; }
        IBaseRepository<Profession> Profession {  get; }
        IBaseRepository<Order> Orders { get; }
        //IBaseRepository<EvaluationProvider> evaluationProvider { get; }
        //IBaseRepository<EvaluationOrder> evaluationOrder { get; }

        //-----------------------------------------------------------------------------------
        int SaveChanges();

        Task<int> SaveChangesAsync();

    }
}
