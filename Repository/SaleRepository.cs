using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repository
{
    public class SaleRepository : ISaleRepository
    {
        private readonly Database _db;
        public SaleRepository(Database db) { _db = db; }

        public int CreateSale(Sale sale, IEnumerable<SaleItem> items)
        {
            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var tr = c.BeginTransaction())
                {
                    int saleId = 0;
                    using (var cmd = new SqlCommand(
                        "INSERT INTO Sales (Date, Total) OUTPUT INSERTED.Id VALUES (@d, @t);",
                        c, tr))
                    {
                        cmd.Parameters.AddWithValue("@d", sale.Date);
                        cmd.Parameters.AddWithValue("@t", sale.Total);

                        var scalar = cmd.ExecuteScalar();
                        if (scalar != null && scalar != DBNull.Value)
                            saleId = Convert.ToInt32(scalar);
                    }

                    foreach (var it in items)
                    {
                        using (var itcmd = new SqlCommand(
                            "INSERT INTO SaleItems (SaleId, ProductCode, Quantity, Price) VALUES (@s, @p, @q, @pr);",
                            c, tr))
                        {
                            itcmd.Parameters.AddWithValue("@s", saleId);
                            itcmd.Parameters.AddWithValue("@p", it.ProductCode);
                            itcmd.Parameters.AddWithValue("@q", it.Quantity);
                            itcmd.Parameters.AddWithValue("@pr", it.Price);
                            itcmd.ExecuteNonQuery();
                        }
                    }

                    tr.Commit();
                    return saleId;
                }
            }
        }

        public IEnumerable<Tuple<Sale, IEnumerable<SaleItem>>> GetSalesByDate(DateTime date)
        {
            var result = new List<Tuple<Sale, IEnumerable<SaleItem>>>();

            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Date, Total FROM Sales WHERE CAST(Date AS DATE) = @d";
                    cmd.Parameters.AddWithValue("@d", date.Date);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int saleId = r.GetInt32(0);
                            var sale = new Sale
                            {
                                Id = saleId,
                                Date = r.GetDateTime(1),
                                Total = r.GetDecimal(2)
                            };

                            // load items for this sale
                            var items = new List<SaleItem>();
                            using (var icmd = new SqlCommand("SELECT Id, SaleId, ProductCode, Quantity, Price FROM SaleItems WHERE SaleId=@s", c))
                            {
                                icmd.Parameters.AddWithValue("@s", saleId);
                                using (var ir = icmd.ExecuteReader())
                                {
                                    while (ir.Read())
                                    {
                                        items.Add(new SaleItem
                                        {
                                            Id = ir.GetInt32(0),
                                            SaleId = ir.GetInt32(1),
                                            ProductCode = ir.GetInt32(2),
                                            Quantity = ir.GetInt32(3),
                                            Price = ir.GetDecimal(4)
                                        });
                                    }
                                }
                            }

                            result.Add(Tuple.Create(sale, (IEnumerable<SaleItem>)items));
                        }
                    }
                }
            }

            return result;
        }
    }
}
