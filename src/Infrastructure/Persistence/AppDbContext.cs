using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationUser : IdentityUser<Guid> { }

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Price).HasColumnType("numeric(18,2)");
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();
            e.HasIndex(x => x.UpdatedAt);
        });

        b.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.Status).HasMaxLength(40).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();

            e.HasMany(x => x.Items)
             .WithOne()
             .HasForeignKey(i => i.OrderId)   // artýk property var
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderId).IsRequired();
            e.Property(x => x.ProductId).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.UnitPrice).HasColumnType("numeric(18,2)");
            e.Property(x => x.Quantity).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var e in ChangeTracker.Entries<Product>())
        {
            if (e.State == EntityState.Added)
                e.Entity.CreatedAt = now;

            if (e.State is EntityState.Added or EntityState.Modified)
                e.Entity.UpdatedAt = now;
        }

        return base.SaveChangesAsync(ct);
    }
}
