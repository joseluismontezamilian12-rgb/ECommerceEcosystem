using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { "prod-001", "Laptop Gamer de alta eficiencia para desarrollo de software.", "Laptop Lenovo LOQ Gen 10", 1200.00m, 10 },
                    { "prod-002", "Audífonos inalámbricos con cancelación de ruido premium.", "Audífonos Sony WH-1000XM4", 350.00m, 15 },
                    { "prod-003", "Mouse gamer ergonómico con sensor de alta precisión.", "Mouse Logitech G502 Hero", 60.00m, 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "prod-001");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "prod-002");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "prod-003");
        }
    }
}
