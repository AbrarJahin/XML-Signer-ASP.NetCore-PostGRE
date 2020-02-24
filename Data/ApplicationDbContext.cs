﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XmlSigner.Data.Models;

namespace XmlSigner.Data
{
    //public class ApplicationDbContext : IdentityDbContext<IdentityUser<long>, IdentityRole<long>, long>
    public class ApplicationDbContext : IdentityDbContext<User, Role, long, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        //Table List - Auth tables are added by default
            public DbSet<XmlFile> XmlFiles { get; set; }
            public DbSet<LeaveApplication> LeaveApplications { get; set; }
            public DbSet<DownloadUploadToken> DownloadUploadTokens { get; set; }
            //public virtual DbSet<User> Users { get; set; }
        //Table List End
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(x => x.Id);

                // Each User can have many UserClaims
                user.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                user.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                user.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                user.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Role>(role =>
            {
                // Each Role can have many entries in the UserRole join table
                role.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                role.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });
        }

        /*
                public override int SaveChanges()
                {
                    AddOrUpdateTimestamps();
                    return base.SaveChanges();
                }

                public override int SaveChanges(bool acceptAllChangesOnSuccess)
                {
                    AddOrUpdateTimestamps();
                    return base.SaveChanges(acceptAllChangesOnSuccess);
                }

                public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
                {
                    AddOrUpdateTimestamps();
                    return base.SaveChangesAsync(cancellationToken);
                }

                public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
                {
                    AddOrUpdateTimestamps();
                    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }

                private void AddOrUpdateTimestamps()
                {
                    var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

                    //long currentUserId = -1;    //-1 for Anonimous

                    foreach (var entity in entities)
                    {
                        //Should store location also from here- http://www.jerriepelser.com/blog/aspnetcore-geo-location-from-ip-address/

                        if (entity.State == EntityState.Added)
                        {
                            ((BaseModel)entity.Entity).CreateTime = DateTime.UtcNow;
                            //((BaseModel)entity.Entity).CreateTime = currentUserId;
                            //((BaseEntity)entity.Entity).CreatorIPAddress = _httpContext.Connection.RemoteIpAddress.ToString();
                        }
                        else
                        {
                            this.Entry(((BaseModel)entity.Entity)).Property(e => e.CreateTime).IsModified = false;
                            //this.Entry(((BaseModel)entity.Entity)).Property(e => e.CreatorUserId).IsModified = false;
                            //this.Entry(((BaseEntity)entity.Entity)).Property(e => e.CreatorUserId).CreatorIPAddress = false;

                            //Set updated At time
                            ((BaseModel)entity.Entity).LastUpdateTime = DateTime.UtcNow;
                            //((BaseModel)entity.Entity).LastModifireUserId = currentUserId;
                            //((BaseEntity)entity.Entity).LastModifireIPAddress = _httpContext.Connection.RemoteIpAddress.ToString();
                        }
                    }
                }
        */
    }
}
