using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Repository;

namespace RestaurantPOS.Service
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repo;

        public SaleService(ISaleRepository repo)
        {
            _repo = repo;
        }

        public int CreateSale(DateTime date, List<SaleItem> items)
        {
            var sale = new Sale
            {
                Date = date,
                Items = items
            };

            sale.Total = items.Sum(i => i.FinalAmount);
            return _repo.CreateSale(sale, items);
        }

        public List<Tuple<Sale, IEnumerable<SaleItem>>> GetSalesByDate(DateTime date)
        {
            var raw = _repo.GetSalesByDate(date);
            return raw?.ToList() ?? new List<Tuple<Sale, IEnumerable<SaleItem>>>();
        }
    }
}
