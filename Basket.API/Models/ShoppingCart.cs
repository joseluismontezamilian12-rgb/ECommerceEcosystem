namespace Basket.API.Models;

public class ShoppingCart
{
    public string UserName { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();

    public decimal TotalPrice
    {
        get
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Price * item.Quantity;
            }
            return total;
        }
    }

    public ShoppingCart()
    {
    }

    public ShoppingCart(string userName)
    {
        UserName = userName;
    }
}