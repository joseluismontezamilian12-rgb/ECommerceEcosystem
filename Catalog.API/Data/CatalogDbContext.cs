using Catalog.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    // 🛒 Esta línea le dice a EF: "Quiero una tabla física llamada 'Products' basada en el molde Product"
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración quirúrgica de la tabla
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id); // ID como Llave Primaria
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)"); // Precisión exacta para dinero
        });

        // 🌱 Inyección de datos usando el constructor posicional de tu Record (Id, Name, Description, Price, Stock)
        modelBuilder.Entity<Product>().HasData(
            new Product(
                "prod-001", // 👈 Cambiado de "string" a "prod-001" para evitar el choque de llaves primarias
                "Laptop Lenovo LOQ Gen 10",
                "Laptop Gamer de alta eficiencia para desarrollo de software.",
                1200.00M,
                10
            ),
            new Product(
                "prod-002",
                "Audífonos Sony WH-1000XM4",
                "Audífonos inalámbricos con cancelación de ruido premium.",
                350.00M,
                15
            ),
            new Product(
                "prod-003",
                "Mouse Logitech G502 Hero",
                "Mouse gamer ergonómico con sensor de alta precisión.",
                60.00M,
                30
            )
        );
    }
}