
using RestaurantPOS.Models;

namespace RestaurantPOS.Repository
{
    public interface IProductRepository : IRepository<Product>
    {
        Product GetByName(string name);
    }
}
