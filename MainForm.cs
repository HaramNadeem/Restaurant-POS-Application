using RestaurantPOS.Service;
using System;
using System.Windows.Forms;

namespace RestaurantPOS.UI
{
    public partial class MainForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly AuthService _auth;

        public MainForm(IProductService productService, ISaleService saleService, AuthService auth)
        {
            _productService = productService;
            _saleService = saleService;
            _auth = auth;
            InitializeComponent();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            new ProductsForm(_productService).ShowDialog();
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            new SalesForm(_productService, _saleService).ShowDialog();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            new ReportsForm(_saleService, _productService).ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
