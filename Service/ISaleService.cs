using System;
using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Service
{
    public interface ISaleService
    {
        int CreateSale(DateTime date, List<SaleItem> items);
        List<Tuple<Sale, IEnumerable<SaleItem>>> GetSalesByDate(DateTime date);
    }
}
