using Catalog.API.Data;
using Catalog.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🛠️ 1. CONEXIÓN A LA BASE DE DATOS REAL (Inyección de Dependencias)
// Leemos la cadena de conexión del appsettings.json y registramos nuestro DbContext
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🌐 RUTAS DE LA API CONECTADAS A SQL SERVER (Usando Entity Framework Core)

// 1. GET: api/catalog -> Traer todos los productos de la base de datos física
app.MapGet("/api/catalog", async (CatalogDbContext context) =>
{
    var products = await context.Products.ToListAsync();
    return Results.Ok(products);
})
.WithName("GetProducts");


// 2. GET: api/catalog/{id} -> Buscar un producto por ID en las tablas reales
app.MapGet("/api/catalog/{id}", async (string id, CatalogDbContext context) =>
{
    var product = await context.Products.FindAsync(id);

    if (product is null)
    {
        return Results.NotFound(new { mensaje = $"El producto con ID '{id}' no existe en la base de datos." });
    }

    return Results.Ok(product);
})
.WithName("GetProductById");


// 3. POST: api/catalog -> Insertar un nuevo producto físicamente en SQL Server
app.MapPost("/api/catalog", async (Product newProduct, CatalogDbContext context) =>
{
    if (newProduct is null || string.IsNullOrEmpty(newProduct.Id))
    {
        return Results.BadRequest(new { mensaje = "Los datos del producto son inválidos." });
    }

    // Agrega el registro al bloque de cambios de EF
    context.Products.Add(newProduct);

    // Impacta y guarda los cambios permanentemente en el disco duro
    await context.SaveChangesAsync();

    return Results.Created($"/api/catalog/{newProduct.Id}", newProduct);
})
.WithName("CreateProduct");

app.Run();