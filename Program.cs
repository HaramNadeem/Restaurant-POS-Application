using System;
using System.Windows.Forms;
using RestaurantPOS.Data;
using RestaurantPOS.Repository;
using RestaurantPOS.Service;

namespace RestaurantPOS
{
    internal static class Program
    {
        
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            
            string connectionString = @"Server=DESKTOP-DPP1L2J;Database=RestaurantPOS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            var db = new Database(connectionString);
            db.EnsureCreated();

            // Repositories
            var productRepo = new ProductRepository(db);
            var saleRepo = new SaleRepository(db);

            // Services
            var productService = new ProductService(productRepo);
            var saleService = new SaleService(saleRepo);
            var authService = new AuthService();

            // Pass them into login form
            Application.Run(new RestaurantPOS.UI.LoginForm(productService, saleService, authService));
        }
    }
}
