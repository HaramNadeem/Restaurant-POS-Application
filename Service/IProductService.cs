
using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Service
{
    public interface IProductService
    {
        List<Product> GetAll();
        Product Get(int code);
        void Add(Product product);
        void Update(Product product);
        void Delete(int code);
    }
}
