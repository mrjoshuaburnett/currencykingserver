using CurrencyKing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CurrencyKing.Data.DatabaseModels
{
    public class DatabaseContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SecurityService _securityService;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(options)
        {
            _securityService = securityService;
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserExchange> UserExchanges { get; set; }

        public DbSet<PasswordReset> PasswordResets { get; set; }




        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetMetaData();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void SetMetaData()
        {
            var proceed = _httpContextAccessor.HttpContext != null;

            if (proceed)
            {
                var token = _httpContextAccessor.HttpContext.Request.Cookies["accessToken"];
                var claims = _securityService.GetJwtClaims(token);
                proceed = Guid.TryParse(claims?.FindFirst("UserId")?.Value, out Guid userId);
                var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseDbModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

                if (entities.Any() && proceed)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.State == EntityState.Added)
                        {
                            ((BaseDbModel)entity.Entity).CreatedDate = DateTime.UtcNow;
                        }

                        ((BaseDbModel)entity.Entity).ModifiedDate = DateTime.UtcNow;
                    }
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserExchange>()
                .HasOne(e => e.User)
                .WithMany(c => c.UserConversions);

            modelBuilder.Entity<PasswordReset>()
                .HasOne(e => e.User)
                .WithMany(c => c.PasswordResets);

            modelBuilder.Entity<UserExchange>().Property(x => x.ExchangeRate).HasPrecision(12, 10);
        }

    }
}
