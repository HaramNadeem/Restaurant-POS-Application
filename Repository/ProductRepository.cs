using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly Database _db;
        public ProductRepository(Database db) { _db = db; }

        public void Add(Product item)
        {
            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Products (Name, Price) OUTPUT INSERTED.Code VALUES (@n, @p);";
                    cmd.Parameters.AddWithValue("@n", item.Name);
                    cmd.Parameters.AddWithValue("@p", item.Price);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        item.Code = Convert.ToInt32(result);
                }
            }
        }

        public void Delete(int id)
        {
            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Products WHERE Code=@c";
                    cmd.Parameters.AddWithValue("@c", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Product Get(int id)
        {
            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT Code, Name, Price FROM Products WHERE Code=@c";
                    cmd.Parameters.AddWithValue("@c", id);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new Product
                            {
                                Code = r.GetInt32(0),
                                Name = r.GetString(1),
                                Price = r.GetDecimal(2)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Product> GetAll()
        {
            var list = new List<Product>();

            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT Code, Name, Price FROM Products";
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new Product
                            {
                                Code = r.GetInt32(0),
                                Name = r.GetString(1),
                                Price = r.GetDecimal(2)
                            });
                        }
                    }
                }
            }

            return list;
        }

        public void Update(Product item)
        {
            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Products SET Name=@n, Price=@p WHERE Code=@c";
                    cmd.Parameters.AddWithValue("@n", item.Name);
                    cmd.Parameters.AddWithValue("@p", item.Price);
                    cmd.Parameters.AddWithValue("@c", item.Code);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Product GetByName(string name)
        {
            using (var c = new SqlConnection(_db.ConnectionString))
            {
                c.Open();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT Code, Name, Price FROM Products WHERE Name=@n";
                    cmd.Parameters.AddWithValue("@n", name);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return new Product
                            {
                                Code = r.GetInt32(0),
                                Name = r.GetString(1),
                                Price = r.GetDecimal(2)
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
