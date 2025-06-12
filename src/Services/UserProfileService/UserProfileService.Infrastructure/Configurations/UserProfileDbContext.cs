using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Infrastructure.Configurations;

public class UserProfileDbContext : DbContext, IDbContext
{
    public UserProfileDbContext(DbContextOptions<UserProfileDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserDevice> UserDevices { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Áp dụng soft delete cho UserProfile
        modelBuilder.Entity<UserProfile>().HasQueryFilter(up => !up.IsDeleted);
        modelBuilder.Entity<UserDevice>().HasQueryFilter(ud => !ud.IsDeleted);
        modelBuilder.Entity<Address>().HasQueryFilter(a => !a.IsDeleted);

        // Cấu hình khóa chính
        modelBuilder.Entity<UserProfile>().HasKey(up => up.UserId);
        modelBuilder.Entity<UserDevice>().HasKey(ud => ud.Id);
        modelBuilder.Entity<Address>().HasKey(a => a.Id);

        // Cấu hình quan hệ
        modelBuilder.Entity<UserProfile>()
            .HasMany(up => up.Addresses)
            .WithOne(a => a.UserProfile)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<UserProfile>()
            .HasMany(up => up.Devices)
            .WithOne(ud => ud.UserProfile)
            .HasForeignKey(ud => ud.UserId);

        // Chỉ mục duy nhất
        modelBuilder.Entity<UserProfile>().HasIndex(up => up.Email).IsUnique();
        modelBuilder.Entity<UserDevice>().HasIndex(ud => new { ud.UserId, ud.DeviceId }).IsUnique();
        modelBuilder.Entity<Address>().HasIndex(a => new { a.UserId, a.Street, a.City, a.PostalCode });
    }

    public new DbSet<T> Set<T>() where T : class => base.Set<T>();
}