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

// 🚚 2. MIGRACIONES AUTOMÁTICAS (solo si se activa por configuración)
// En la nube no hay nadie para correr "dotnet ef database update" a mano, así que
// el propio arranque crea el esquema y siembra el catálogo. Va detrás de una
// bandera para que un despliegue local nunca toque una base sin querer.
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
}

// 📖 3. DOCUMENTACIÓN PÚBLICA
// Swagger queda disponible también en producción y en la raíz: es una API de
// portafolio, y su valor es que cualquiera pueda probarla desde el navegador.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1");
    options.RoutePrefix = string.Empty;
});

// Azure App Service ya termina el TLS en su proxy: redirigir dentro de la app
// solo produce saltos de más. Fuera de producción sí se conserva.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

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