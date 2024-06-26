﻿using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.OrderData;
using Maintainify.Core.Entity.ProfessionData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Maintainify.Core
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        //-----------------------------------------------------------------------------------
        public virtual DbSet<Profession> Professions { get; set; }
        public virtual DbSet<PathFiles> PathFiles { get; set; }
        public virtual DbSet<Images> Images { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        //public virtual DbSet<EvaluationOrder> EvaluationOrders { get; set; }
        //public virtual DbSet<EvaluationProvider> EvaluationProviders { get; set; }
        //-----------------------------------------------------------------------------------

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public ApplicationDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=p3nwplsk12sql-v04.shr.prod.phx3.secureserver.net;Database=Maintainify;User Id=Maintainify;Password=Ahmed@123;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>().ToTable("Users", "dbo");
            modelBuilder.Entity<ApplicationRole>().ToTable("Role", "dbo");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRole", "dbo");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim", "dbo");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin", "dbo");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "dbo");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "dbo");

            modelBuilder.Entity<Order>()
                        .HasOne(o => o.Provider)
                        .WithMany(u => u.OrdersAsProvider)
                        .HasForeignKey(o => o.ProviderId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Seeker)
                .WithMany(u => u.OrdersAsSeeker)
                .HasForeignKey(o => o.SeekerId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<EvaluationProvider>()
            //.HasOne(m => m.Seeker)
            //.WithMany(u => u.EvaluationSeekers)
            //.HasForeignKey(m => m.SeekerId)
            //.OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<EvaluationProvider>()
            //    .HasOne(m => m.Provider)
            //    .WithMany(m => m.EvaluationProviders)
            //    .HasForeignKey(m => m.ProviderId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
