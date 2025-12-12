
using System;
using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repository
{
    public interface ISaleRepository
    {
        int CreateSale(Sale sale, IEnumerable<SaleItem> items);
        IEnumerable<Tuple<Sale, IEnumerable<SaleItem>>> GetSalesByDate(DateTime date);
    }
}
