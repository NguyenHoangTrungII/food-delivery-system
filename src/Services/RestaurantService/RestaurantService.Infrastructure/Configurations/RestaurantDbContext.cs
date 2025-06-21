

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
            entity.Property(r => r.Distance).IsRequired(false); // Distance có thể không cần thiết trong DB

            // Khai báo shadow property cho cột geom
            entity.Property<Point>("geom")
                .HasColumnType("geography (Point, 4326)"); // Sử dụng geography với SRID 4326

            // Tự động tính toán geom từ Latitude và Longitude
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
            // Cập nhật geom từ Latitude và Longitude
            entry.Property("geom").CurrentValue = new Point(restaurant.Longitude, restaurant.Latitude)
            {
                SRID = 4326 // SRID 4326 cho hệ tọa độ WGS84
            };
        }
    }

    public new DbSet<T> Set<T>() where T : class => base.Set<T>();
}