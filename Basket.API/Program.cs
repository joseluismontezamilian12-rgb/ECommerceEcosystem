using Basket.API.Models;
using Basket.API.Repositories;
using Basket.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔴 1. CONFIGURACIÓN DEL SERVICIO OFICIAL DE REDIS
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Intenta leer la cadena desde appsettings.json; si no existe, usa localhost:6379 por defecto
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString") ?? "localhost:6379";
});

// 🛠️ 2. REGISTRAR NUESTRO REPOSITORIO (Cambiado a Scoped para conexiones a infraestructura externa)
builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// 🌐 3. CONFIGURAR LA COMUNICACIÓN CON EL MICROSERVICIO DE CATÁLOGO
builder.Services.AddHttpClient<CatalogService>(client =>
{
    // La dirección del catálogo cambia con el entorno (local, Azure, otro cluster),
    // así que se lee de la configuración. El puerto local queda solo como respaldo.
    client.BaseAddress = new Uri(
        builder.Configuration.GetValue<string>("Services:CatalogUrl") ?? "https://localhost:44366");
});

// Configuración básica de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 📖 DOCUMENTACIÓN PÚBLICA
// Igual que en Catalog.API: la documentación vive en la raíz y también en
// producción, porque el punto de esta API es que se pueda probar sin instalar nada.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Basket.API";

    // Igual que en Catalog.API: sin metadatos, ningun rastreador social puede
    // construir la vista previa de este enlace.
    options.HeadContent = """
        <meta name="description" content="Microservicio de carrito en .NET 10 sobre Redis. Nunca confia en el precio que envia el cliente: lo relee del catalogo antes de guardar.">
        <meta property="og:type" content="website">
        <meta property="og:site_name" content="Jose Luis Monteza">
        <meta property="og:title" content="Basket.API - carrito en .NET 10 sobre Redis">
        <meta property="og:description" content="Mandale una laptop de 1200 con el precio en 1.00 y te responde 1200: la validacion vive en el servidor, donde el cliente no llega.">
        <meta property="og:url" content="https://ecommerce-basket-lnxj7c.azurewebsites.net">
        <meta property="og:image" content="https://avatars.githubusercontent.com/u/272381527?v=4">
        <meta name="twitter:card" content="summary">
        """;
});

// Azure App Service ya termina el TLS en su proxy.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// 🌐 RUTAS DE LA API (Endpoints para el Carrito de Compras)

// 1. GET: api/basket/{userName} -> Obtener el carrito de un usuario específico desde Redis
app.MapGet("/api/basket/{userName}", async (string userName, IBasketRepository repository) =>
{
    var basket = await repository.GetBasketAsync(userName);

    // Si el usuario aún no tiene un carrito creado, le devolvemos uno nuevo y vacío
    return Results.Ok(basket ?? new ShoppingCart(userName));
})
.WithName("GetBasket");

// 2. POST: api/basket -> Crear o actualizar los productos de un carrito en Redis (Validando con el Catálogo)
app.MapPost("/api/basket", async (ShoppingCart basket, IBasketRepository repository, CatalogService catalogService) =>
{
    // Validamos de forma asíncrona cada artículo que el cliente quiere meter al carrito
    foreach (var item in basket.Items)
    {
        var validProduct = await catalogService.GetProductAsync(item.ProductId);

        if (validProduct is null)
        {
            return Results.BadRequest(new { mensaje = $"El producto con ID '{item.ProductId}' no existe en el catálogo real." });
        }

        // Seguridad y Consistencia: Forzamos el nombre y precio real desde SQL Server
        item.ProductName = validProduct.ProductName;
        item.Price = validProduct.Price;
    }

    var updatedBasket = await repository.UpdateBasketAsync(basket);
    return Results.Ok(updatedBasket);
})
.WithName("UpdateBasket");

// 3. DELETE: api/basket/{userName} -> Vaciar o eliminar el carrito de Redis
app.MapDelete("/api/basket/{userName}", async (string userName, IBasketRepository repository) =>
{
    var success = await repository.DeleteBasketAsync(userName);
    return success ? Results.Ok() : Results.NotFound(new { mensaje = "No se encontró un carrito activo para este usuario." });
})
.WithName("DeleteBasket");

app.Run();