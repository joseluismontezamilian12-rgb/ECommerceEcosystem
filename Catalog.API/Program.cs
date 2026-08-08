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
    options.DocumentTitle = "Catalog.API";

    // La página de Swagger es un esqueleto que rellena JavaScript: sin estas
    // etiquetas un rastreador social solo ve un documento vacío y se niega a
    // generar la vista previa del enlace.
    options.HeadContent = """
        <meta name="description" content="Microservicio de catalogo en .NET 10: Minimal APIs, Entity Framework Core y SQL Server en Azure. Documentacion publica.">
        <meta property="og:type" content="website">
        <meta property="og:site_name" content="Jose Luis Monteza">
        <meta property="og:title" content="Catalog.API - catalogo de productos en .NET 10">
        <meta property="og:description" content="Minimal APIs sobre EF Core y Azure SQL. Es la fuente de verdad de los precios: Basket.API los relee de aqui antes de guardar cualquier carrito.">
        <meta property="og:url" content="https://ecommerce-catalog-lnxj7c.azurewebsites.net">
        <meta property="og:image" content="https://avatars.githubusercontent.com/u/272381527?v=4">
        <meta name="twitter:card" content="summary">
        """;
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