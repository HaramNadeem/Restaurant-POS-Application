using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RestaurantPOS.Data
{
    public class Database
    {
        public string ConnectionString { get; private set; }

        public Database(string connectionString)
        {
            // Use the provided connection string
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Creates tables if they do not exist
        public void EnsureCreated()
        {
            using (var c = new SqlConnection(ConnectionString))
            {
                c.Open();

                string createProducts = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products](
        [Code] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL
    );
END
";
                string createSales = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sales]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Sales](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Date] DATETIME NOT NULL,
        [Total] DECIMAL(18,2) NOT NULL
    );
END
";
                string createSaleItems = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaleItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SaleItems](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SaleId] INT NOT NULL,
        [ProductCode] INT NOT NULL,
        [Quantity] INT NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleId) REFERENCES Sales(Id),
        CONSTRAINT FK_SaleItems_Products FOREIGN KEY (ProductCode) REFERENCES Products(Code)
    );
END
";

                using (var cmd = new SqlCommand(createProducts, c)) { cmd.ExecuteNonQuery(); }
                using (var cmd = new SqlCommand(createSales, c)) { cmd.ExecuteNonQuery(); }
                using (var cmd = new SqlCommand(createSaleItems, c)) { cmd.ExecuteNonQuery(); }
            }
        }
    }
}
