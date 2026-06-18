using System.Net.Http.Json;
using Basket.API.Models;

namespace Basket.API.Services;

public class CatalogService
{
    private readonly HttpClient _httpClient;

    public CatalogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 📦 Pequeño DTO interno para leer el JSON tal cual lo envía el Catálogo (usa "Name")
    private record CatalogProductDto(string Id, string Name, decimal Price);

    // Método para viajar al otro microservicio y traer el producto real
    public async Task<BasketItem?> GetProductAsync(string productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/catalog/{productId}");

            if (!response.IsSuccessStatusCode) return null;

            // 1. Deserializamos usando el molde exacto del Catálogo
            var catalogProduct = await response.Content.ReadFromJsonAsync<CatalogProductDto>();

            if (catalogProduct is null) return null;

            // 2. Transfundimos los datos quirúrgicamente al molde que el Carrito entiende
            return new BasketItem
            {
                ProductId = catalogProduct.Id,
                ProductName = catalogProduct.Name, // 👈 ¡Aquí rescatamos el nombre real!
                Price = catalogProduct.Price,
                Quantity = 1 // Valor inicial por defecto
            };
        }
        catch
        {
            return null; // Si el microservicio catálogo está apagado o falla
        }
    }
}