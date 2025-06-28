using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Infrastructure.Configurations;

public class RestaurantDbContext : DbContext, IDbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

    public DbSet<Restaurant> Restaurants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.ToTable("restaurants");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(255);
            entity.Property(r => r.Address).HasMaxLength(255);
            entity.Property(r => r.Latitude).IsRequired();
            entity.Property(r => r.Longitude).IsRequired();
            entity.Property(r => r.Distance).HasDefaultValue(0.0);

            // Định nghĩa shadow property geom
            entity.Property<Point>("geom")
                .HasColumnType("geometry  (Point, 4326)");

            // Định nghĩa computed columns từ geom
            entity.Property(r => r.Latitude)
                .HasComputedColumnSql("ST_Y(geom)", stored: true);
            entity.Property(r => r.Longitude)
                .HasComputedColumnSql("ST_X(geom)", stored: true);

            // Tạo chỉ mục GIST cho geom
            entity.HasIndex("geom")
                .HasMethod("GIST");
        });

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateGeomColumn();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateGeomColumn();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateGeomColumn()
    {
        var entries = ChangeTracker
            .Entries<Restaurant>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var restaurant = entry.Entity;
            // Kiểm tra giá trị hợp lệ thay vì HasValue
            if (!double.IsNaN(restaurant.Latitude) && !double.IsNaN(restaurant.Longitude) &&
                restaurant.Latitude >= -90 && restaurant.Latitude <= 90 &&
                restaurant.Longitude >= -180 && restaurant.Longitude <= 180)
            {
                var point = new Point(restaurant.Longitude, restaurant.Latitude)
                {
                    SRID = 4326
                };
                entry.Property("geom").CurrentValue = point;
            }
        }
    }

    public new DbSet<T> Set<T>() where T : class => base.Set<T>();
}

////using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
////using Microsoft.EntityFrameworkCore;
////using NetTopologySuite.Geometries; // Cần thêm using này
////using RestaurantService.Domain.Entities;
////using NetTopologySuite.IO.;

////namespace RestaurantService.Infrastructure.Configurations;

////public class RestaurantDbContext : DbContext, IDbContext
////{
////    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

////    public DbSet<Restaurant> Restaurants { get; set; }

////    protected override void OnModelCreating(ModelBuilder modelBuilder)
////    {
////        modelBuilder.Entity<Restaurant>(entity =>
////        {
////            entity.ToTable("restaurants");
////            entity.HasKey(r => r.Id);
////            entity.Property(r => r.Name).IsRequired().HasMaxLength(255);
////            entity.Property(r => r.Address).HasMaxLength(255);
////            entity.Property(r => r.Latitude).IsRequired();
////            entity.Property(r => r.Longitude).IsRequired();
////            entity.Property(r => r.Distance).HasDefaultValue(0.0);
////            // Thêm cột geometry cho PostGIS
////            entity.Property(r => r.Geom) // Sửa tên property thành Geom (theo lỗi bạn cung cấp)
////                .HasColumnName("geom")
////                .HasColumnType("geography (Point, 4326)")
////                .HasConversion(
////                    point => point == null ? (PgGeometry)null : new PgGeometry(point),
////                    pgGeometry => pgGeometry == null ? null : (Point)pgGeometry.Geometry
////                );
////        });
////    }

////    public new DbSet<T> Set<T>() where T : class => base.Set<T>();
////}

////using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
////using Microsoft.EntityFrameworkCore;
////using RestaurantService.Domain.Entities;

////namespace RestaurantService.Infrastructure.Configurations;

////public class RestaurantDbContext : DbContext, IDbContext
////{
////    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

////    public DbSet<Restaurant> Restaurants { get; set; }

////    protected override void OnModelCreating(ModelBuilder modelBuilder)
////    {
////        modelBuilder.Entity<Restaurant>(entity =>
////        {
////            entity.ToTable("restaurants");
////            entity.HasKey(r => r.Id);
////            entity.Property(r => r.Name).IsRequired().HasMaxLength(255);
////            entity.Property(r => r.Address).HasMaxLength(255);
////            entity.Property(r => r.Latitude).IsRequired();
////            entity.Property(r => r.Longitude).IsRequired();
////            entity.Property(r => r.Distance).HasDefaultValue(0.0);
////        });
////    }

////    public new DbSet<T> Set<T>() where T : class => base.Set<T>();
////}

//using FoodDeliverySystem.DataAccess.Contexts.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using NetTopologySuite.Geometries;
//using RestaurantService.Domain.Entities;

//namespace RestaurantService.Infrastructure.Configurations;

//public class RestaurantDbContext : DbContext, IDbContext
//{
//    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

//    public DbSet<Restaurant> Restaurants { get; set; }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<Restaurant>(entity =>
//        {
//            entity.ToTable("restaurants");
//            entity.HasKey(r => r.Id);
//            entity.Property(r => r.Name).IsRequired().HasMaxLength(255);
//            entity.Property(r => r.Address).HasMaxLength(255);
//            entity.Property(r => r.Latitude).IsRequired();
//            entity.Property(r => r.Longitude).IsRequired();
//            entity.Property(r => r.Distance).IsRequired(false); // Distance có thể không cần thiết trong DB

//            //Khai báo shadow property cho cột geom
//            entity.Property<Point>("geom")
//                .HasColumnType("geography (Point, 4326)"); // Sử dụng geography với SRID 4326

//            //Tự động tính toán geom từ Latitude và Longitude
//            entity.Property(r => r.Latitude)
//                .HasComputedColumnSql("ST_Y(geom)", stored: true);
//            entity.Property(r => r.Longitude)
//                .HasComputedColumnSql("ST_X(geom)", stored: true);

//            //Tạo chỉ mục GIST cho geom
//            entity.HasIndex("geom")
//                .HasMethod("GIST");
//        });

//        base.OnModelCreating(modelBuilder);
//    }

//    public override int SaveChanges()
//    {
//        UpdateGeomColumn();
//        return base.SaveChanges();
//    }

//    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//    {
//        UpdateGeomColumn();
//        return await base.SaveChangesAsync(cancellationToken);
//    }

//    private void UpdateGeomColumn()
//    {
//        var entries = ChangeTracker
//            .Entries<Restaurant>()
//            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

//        foreach (var entry in entries)
//        {
//            var restaurant = entry.Entity;
//            Cập nhật geom từ Latitude và Longitude
//            entry.Property("geom").CurrentValue = new Point(restaurant.Longitude, restaurant.Latitude)
//            {
//                SRID = 4326 // SRID 4326 cho hệ tọa độ WGS84
//            };
//        }
//    }

//    public new DbSet<T> Set<T>() where T : class => base.Set<T>();
//}