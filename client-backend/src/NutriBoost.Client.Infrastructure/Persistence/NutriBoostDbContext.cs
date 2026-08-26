using Microsoft.EntityFrameworkCore;
using NutriBoost.Client.Domain.Entities;

namespace NutriBoost.Client.Infrastructure.Persistence;

public class NutriBoostDbContext(DbContextOptions<NutriBoostDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.PriceUsd).HasColumnType("numeric(10,2)");

            // Optimistic concurrency via PostgreSQL's hidden `xmin` system
            // column (Npgsql's idiomatic equivalent of SQL Server's
            // rowversion) — prevents two simultaneous purchases from
            // overselling the last unit. No mapped property needed; EF
            // tracks it as a shadow value and rejects a stale update with
            // DbUpdateConcurrencyException. See docs/ARCHITECTURE.md
            // "Stock concurrency".
            entity.Property<uint>("xmin").IsRowVersion();
        });

        base.OnModelCreating(modelBuilder);
    }
}
