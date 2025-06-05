using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<FunctionModule> FunctionModules { get; set; } = null!;
    public DbSet<FunctionAction> FunctionActions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } // Thêm DbSet
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!; // Thêm DbSet


    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        Console.WriteLine($"ConnectString in  AuthDbContext: {Database.GetConnectionString()}");

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Configure primary keys
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<Role>().HasKey(r => r.Id);
        modelBuilder.Entity<UserRole>().HasKey(ur => ur.Id);
        modelBuilder.Entity<FunctionModule>().HasKey(fm => fm.Id); // Đổi từ FunctionModuleID sang Id
        modelBuilder.Entity<FunctionAction>().HasKey(fa => fa.Id); // Đổi từ FunctionID sang Id
        modelBuilder.Entity<RolePermission>().HasKey(rp => rp.Id);
        modelBuilder.Entity<RefreshToken>().HasKey(rt => rt.Id);
        modelBuilder.Entity<PasswordResetToken>().HasKey(prt => prt.Id);

        // Configure properties
        // User
        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(100)
            .IsRequired();
        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();

        // RefreshToken
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

        //PasswordResetToken
        modelBuilder.Entity<PasswordResetToken>()
            .Property(prt => prt.Token)
            .HasMaxLength(256)
            .IsRequired();
        modelBuilder.Entity<PasswordResetToken>()
            .Property(prt => prt.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<PasswordResetToken>()
            .Property(prt => prt.ExpiresAt)
            .IsRequired();
        modelBuilder.Entity<PasswordResetToken>()
            .Property(prt => prt.CreatedOn)
            .IsRequired();
        modelBuilder.Entity<PasswordResetToken>()
            .Property(prt => prt.IsUsed)
            .IsRequired();

        // Role
        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<Role>()
            .Property(r => r.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();

        // FunctionModule
        modelBuilder.Entity<FunctionModule>()
            .Property(fm => fm.CodeName)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<FunctionModule>()
            .Property(fm => fm.Name)
            .HasMaxLength(100)
            .IsRequired();
        modelBuilder.Entity<FunctionModule>()
            .Property(fm => fm.ModuleType)
            .HasMaxLength(1)
            .IsRequired();
        modelBuilder.Entity<FunctionModule>()
            .Property(fm => fm.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<FunctionModule>()
            .Property(fm => fm.ModifiedBy)
            .HasMaxLength(50);

        // FunctionAction
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.CodeName)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.Name)
            .HasMaxLength(100)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.ActionType)
            .HasMaxLength(20)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.Module)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.UrlPattern)
            .HasMaxLength(200)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.HttpMethod)
            .HasMaxLength(10)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.FunctionScope)
            .HasMaxLength(20)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<FunctionAction>()
            .Property(fa => fa.ModifiedBy)
            .HasMaxLength(50);

        // RolePermission
        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();
        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.ModifiedBy)
            .HasMaxLength(50);

        // Configure relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        modelBuilder.Entity<FunctionModule>()
            .HasOne(fm => fm.Parent)
            .WithMany(fm => fm.Children)
            .HasForeignKey(fm => fm.ParentId) // Đổi từ ParentID sang ParentId
            .IsRequired(false);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .IsRequired();

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.FunctionAction)
            .WithMany(fa => fa.RolePermissions)
            .HasForeignKey(rp => rp.FunctionID) // FunctionID là Guid
            .IsRequired();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .IsRequired();

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(prt => prt.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(prt => prt.UserId)
            .IsRequired();

        // Configure unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<FunctionModule>()
            .HasIndex(fm => fm.CodeName)
            .IsUnique();

        modelBuilder.Entity<FunctionAction>()
            .HasIndex(fa => fa.CodeName)
            .IsUnique();

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(prt => prt.Token)
            .IsUnique();

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Customer",
                Administrator = false,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new Role
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Driver",
                Administrator = false,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new Role
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                Name = "Restaurant",
                Administrator = false,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new Role
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                Name = "Admin",
                Administrator = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "system"
            }
        );

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Username = "john_doe",
                PasswordHash = "$2a$11$hashedpassword",
                Email = "john_doe@example.com",
                IsLocking = false,
                IsOnline = false,
                FailedPWAttempt = 0,
                CreatedAt = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Username = "driver_1",
                PasswordHash = "$2a$11$hashedpassword",
                Email = "driver_1@example.com",
                IsLocking = false,
                IsOnline = false,
                FailedPWAttempt = 0,
                CreatedAt = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new User
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Username = "admin",
                PasswordHash = "$2a$11$hashedpassword",
                Email = "admin@example.com",
                IsLocking = false,
                IsOnline = false,
                FailedPWAttempt = 0,
                CreatedAt = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "system"
            }
        );

        // Seed UserRoles
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                StartDate = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new UserRole
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000002"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                StartDate = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin"
            },
            new UserRole
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000003"),
                UserId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                StartDate = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "system"
            }
        );

        // Seed FunctionModules
        modelBuilder.Entity<FunctionModule>().HasData(
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000001"),
                CodeName = "ORDER_MANAGEMENT",
                Name = "Order Management",
                ParentId = null,
                Level = 0,
                ModuleType = "M",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000002"),
                CodeName = "ORDER_OPERATIONS",
                Name = "Order Operations",
                ParentId = Guid.Parse("80000000-0000-0000-0000-000000000001"),
                Level = 1,
                ModuleType = "G",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000003"),
                CodeName = "MENU_MANAGEMENT",
                Name = "Menu Management",
                ParentId = null,
                Level = 0,
                ModuleType = "M",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000004"),
                CodeName = "MENU_OPERATIONS",
                Name = "Menu Operations",
                ParentId = Guid.Parse("80000000-0000-0000-0000-000000000003"),
                Level = 1,
                ModuleType = "G",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000005"),
                CodeName = "DELIVERY_MANAGEMENT",
                Name = "Delivery Management",
                ParentId = null,
                Level = 0,
                ModuleType = "M",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000006"),
                CodeName = "DELIVERY_OPERATIONS",
                Name = "Delivery Operations",
                ParentId = Guid.Parse("80000000-0000-0000-0000-000000000005"),
                Level = 1,
                ModuleType = "G",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionModule
            {
                Id = Guid.Parse("80000000-0000-0000-0000-000000000007"),
                CodeName = "SYSTEM_SETTINGS",
                Name = "System Settings",
                ParentId = null,
                Level = 0,
                ModuleType = "S",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            }
        );

        // Seed FunctionActions
        modelBuilder.Entity<FunctionAction>().HasData(
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
                CodeName = "CREATE_ORDER",
                Name = "Create Order",
                ActionType = "Create",
                Module = "OrderService",
                UrlPattern = "/api/orders",
                HttpMethod = "POST",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000002"),
                CodeName = "VIEW_ORDER_DETAILS",
                Name = "View Order Details",
                ActionType = "View",
                Module = "OrderService",
                UrlPattern = "/api/orders/{id}",
                HttpMethod = "GET",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000003"),
                CodeName = "UPDATE_ORDER_STATUS",
                Name = "Update Order Status",
                ActionType = "Update",
                Module = "OrderService",
                UrlPattern = "/api/orders/{id}/status",
                HttpMethod = "PUT",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000004"),
                CodeName = "CANCEL_ORDER",
                Name = "Cancel Order",
                ActionType = "Delete",
                Module = "OrderService",
                UrlPattern = "/api/orders/{id}",
                HttpMethod = "DELETE",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000005"),
                CodeName = "RATE_ORDER",
                Name = "Rate Order",
                ActionType = "Update",
                Module = "OrderService",
                UrlPattern = "/api/orders/{id}/rate",
                HttpMethod = "POST",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000006"),
                CodeName = "CREATE_MENU",
                Name = "Create Menu",
                ActionType = "Create",
                Module = "MenuService",
                UrlPattern = "/api/menus",
                HttpMethod = "POST",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000007"),
                CodeName = "VIEW_MENU",
                Name = "View Menu",
                ActionType = "View",
                Module = "MenuService",
                UrlPattern = "/api/menus",
                HttpMethod = "GET",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000008"),
                CodeName = "UPDATE_MENU",
                Name = "Update Menu",
                ActionType = "Update",
                Module = "MenuService",
                UrlPattern = "/api/menus/{id}",
                HttpMethod = "PUT",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000009"),
                CodeName = "DELETE_MENU",
                Name = "Delete Menu",
                ActionType = "Delete",
                Module = "MenuService",
                UrlPattern = "/api/menus/{id}",
                HttpMethod = "DELETE",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000010"),
                CodeName = "ACCEPT_DELIVERY",
                Name = "Accept Delivery",
                ActionType = "Update",
                Module = "DeliveryPartnerService",
                UrlPattern = "/api/deliveries/{id}/accept",
                HttpMethod = "POST",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000011"),
                CodeName = "UPDATE_DELIVERY_STATUS",
                Name = "Update Delivery Status",
                ActionType = "Update",
                Module = "DeliveryPartnerService",
                UrlPattern = "/api/deliveries/{id}/status",
                HttpMethod = "PUT",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000012"),
                CodeName = "REASSIGN_DELIVERY",
                Name = "Reassign Delivery",
                ActionType = "Update",
                Module = "DeliveryPartnerService",
                UrlPattern = "/api/deliveries/{id}/reassign",
                HttpMethod = "PUT",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000013"),
                CodeName = "MANAGE_ROLES",
                Name = "Manage Roles",
                ActionType = "Manage",
                Module = "AuthService",
                UrlPattern = "/api/roles",
                HttpMethod = "POST",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new FunctionAction
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000014"),
                CodeName = "MANAGE_USERS",
                Name = "Manage Users",
                ActionType = "Manage",
                Module = "AuthService",
                UrlPattern = "/api/users",
                HttpMethod = "POST",
                FunctionScope = "API",
                IsActive = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            }
        );

        // Seed RolePermissions
        modelBuilder.Entity<RolePermission>().HasData(
            // Customer (RoleId: 10000000-...)
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000001"), // CREATE_ORDER
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000002"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000002"), // VIEW_ORDER_DETAILS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000003"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000003"), // UPDATE_ORDER_STATUS
                Allowed = false,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000004"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000004"), // CANCEL_ORDER
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000005"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000005"), // RATE_ORDER
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000006"),
                RoleId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000007"), // VIEW_MENU
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            // Driver (RoleId: 20000000-...)
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000007"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000002"), // VIEW_ORDER_DETAILS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000008"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000003"), // UPDATE_ORDER_STATUS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000009"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000004"), // CANCEL_ORDER
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000010"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000010"), // ACCEPT_DELIVERY
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000011"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000011"), // UPDATE_DELIVERY_STATUS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000012"),
                RoleId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000012"), // REASSIGN_DELIVERY
                Allowed = false,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            // Restaurant (RoleId: 30000000-...)
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000013"),
                RoleId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000002"), // VIEW_ORDER_DETAILS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000014"),
                RoleId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000003"), // UPDATE_ORDER_STATUS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000015"),
                RoleId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000006"), // CREATE_MENU
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000016"),
                RoleId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000007"), // VIEW_MENU
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000017"),
                RoleId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000008"), // UPDATE_MENU
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000018"),
                RoleId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000009"), // DELETE_MENU
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            // Admin (RoleId: 50000000-...)
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000019"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000001"), // CREATE_ORDER
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000020"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000002"), // VIEW_ORDER_DETAILS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000021"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000003"), // UPDATE_ORDER_STATUS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000022"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000004"), // CANCEL_ORDER
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000023"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000013"), // MANAGE_ROLES
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            },
            new RolePermission
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000024"),
                RoleId = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                FunctionID = Guid.Parse("90000000-0000-0000-0000-000000000014"), // MANAGE_USERS
                Allowed = true,
                CreatedOn = new DateTime(2025, 5, 28, 12, 24, 0),
                CreatedBy = "admin",
                ModifiedOn = null,
                ModifiedBy = string.Empty
            }
        );
    }
}