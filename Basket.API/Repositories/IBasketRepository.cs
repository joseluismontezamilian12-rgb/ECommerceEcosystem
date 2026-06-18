using Basket.API.Models;

namespace Basket.API.Repositories;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string userName);
    Task<ShoppingCart?> UpdateBasketAsync(ShoppingCart basket);
    Task<bool> DeleteBasketAsync(string userName);
}