using Castle.Core.Resource;
using KafeQRMenu.Data.Core.Concrete;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KafeQRMenu.DataAccess.Configurations;

namespace KafeQRMenu.DataAccess.AppContext
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Cache to avoid repeated database queries - once SuperAdmins exist, they always will
        private bool? _hasSuperAdmins = null;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual DbSet<SuperAdmin> SuperAdmins { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Cafe> Cafes { get; set; }
        public virtual DbSet<MenuCategory> MenuCategories { get; set; }
        public virtual DbSet<MenuItem> MenuItems { get; set; }
        public virtual DbSet<ImageFile> ImageFiles { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(IEntityConfiguration).Assembly);
            // Global Query Filters - Soft Delete için
            builder.Entity<SuperAdmin>().HasQueryFilter(e => e.Status != Status.Deleted);
            builder.Entity<Admin>().HasQueryFilter(e => e.Status != Status.Deleted);
            builder.Entity<Cafe>().HasQueryFilter(e => e.Status != Status.Deleted);
            builder.Entity<MenuCategory>().HasQueryFilter(e => e.Status != Status.Deleted);
            builder.Entity<MenuItem>().HasQueryFilter(e => e.Status != Status.Deleted);
            builder.Entity<ImageFile>().HasQueryFilter(e => e.Status != Status.Deleted);
            builder.Entity<Menu>().HasQueryFilter(e => e.Status != Status.Deleted);

            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            SetBaseProperties();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetBaseProperties();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void SetBaseProperties()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var userId = GetUserId().ToString();
            foreach (var entry in entries)
            {
                SetIfAdded(userId, entry);
                SetIfModified(userId, entry);
                SetIfDeleted(userId, entry);
            }
        }

        private void SetIfDeleted(string userId, EntityEntry<BaseEntity> entry)
        {
            if (entry.State != EntityState.Deleted)
            {
                return;
            }
            if (entry.Entity is not AuditableEntity entity)
            {
                return;
            }
            entry.State = EntityState.Modified;
            entry.Entity.Status = Status.Deleted;
            entity.DeletedTime = DateTime.Now;
            entity.DeletedBy = userId;
        }

        private void SetIfModified(string userId, EntityEntry<BaseEntity> entry)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Status = Status.Modified;
                entry.Entity.UpdatedBy = userId;
                entry.Entity.UpdatedTime = DateTime.Now;
            }
        }

        private void SetIfAdded(string userId, EntityEntry<BaseEntity> entry)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Status = Status.Created;
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedTime = DateTime.Now;
            }
        }

        private string GetUserId()
        {
            // Use cached value if available, otherwise check database
            // Once SuperAdmins exist, they will always exist (one-way state change)
            if (!_hasSuperAdmins.HasValue)
            {
                _hasSuperAdmins = SuperAdmins.Any();
            }

            if (_hasSuperAdmins.Value)
            {
                return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "SystemCreatedThee";
            }

            return "SystemCreatedThee";
        }
    }
}
