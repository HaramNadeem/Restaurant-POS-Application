using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Repository;

namespace RestaurantPOS.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public List<Product> GetAll() => _repo.GetAll().ToList();

        public Product Get(int code) => _repo.Get(code);

        public void Add(Product p) => _repo.Add(p);

        public void Update(Product p) => _repo.Update(p);

        public void Delete(int code) => _repo.Delete(code);
    }
}
