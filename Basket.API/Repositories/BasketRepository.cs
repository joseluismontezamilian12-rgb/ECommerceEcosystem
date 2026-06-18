using System.Text.Json;
using Basket.API.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDistributedCache _redisCache;

    // Inyectamos el servicio de caché distribuida de Redis
    public BasketRepository(IDistributedCache redisCache)
    {
        _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
    }

    // 1. Obtener el carrito desde Redis
    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        var basketString = await _redisCache.GetStringAsync(userName);

        if (string.IsNullOrEmpty(basketString))
            return null;

        // Deserializamos el texto JSON de vuelta a nuestro objeto ShoppingCart
        return JsonSerializer.Deserialize<ShoppingCart>(basketString);
    }

    // 2. Crear o actualizar el carrito en Redis
    public async Task<ShoppingCart?> UpdateBasketAsync(ShoppingCart basket)
    {
        // Convertimos el objeto del carrito a un string JSON plano
        var basketString = JsonSerializer.Serialize(basket);

        await _redisCache.SetStringAsync(basket.UserName, basketString);

        return await GetBasketAsync(basket.UserName);
    }

    // 3. Eliminar el carrito de Redis
    public async Task<bool> DeleteBasketAsync(string userName)
    {
        await _redisCache.RemoveAsync(userName);
        return true;
    }
}