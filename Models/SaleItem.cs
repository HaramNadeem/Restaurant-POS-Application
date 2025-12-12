namespace RestaurantPOS.Models
{
    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }

        // changed to int to match database ProductCode INT
        public int ProductCode { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        // unit price
        public decimal Price { get; set; }

        // Discount cash per line
        public decimal DiscountCash { get; set; }

        // Discount percentage per line (0-100)
        public decimal DiscountPercent { get; set; }

        // Calculated values
        public decimal TotalBeforeDiscount => Quantity * Price;
        public decimal DiscountFromPercent => (DiscountPercent / 100m) * TotalBeforeDiscount;
        public decimal TotalDiscount => DiscountCash + DiscountFromPercent;
        public decimal FinalAmount => TotalBeforeDiscount - TotalDiscount;

        // convenience for backwards compatibility
        public decimal Amount => FinalAmount;
    }
}
