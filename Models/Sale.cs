using System;
using System.Collections.Generic;

namespace RestaurantPOS.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public decimal Total { get; set; }

        public List<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
