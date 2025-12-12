using RestaurantPOS.Models;
using RestaurantPOS.Service;
using System;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantPOS.UI
{
    public partial class ProductsForm : Form
    {
        private readonly IProductService _service;
        private bool isEditing = false;

        public ProductsForm(IProductService service)
        {
            _service = service;
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var list = _service.GetAll().ToList();
            dgvProducts.DataSource = list
                .Select(p => new { p.Code, ItemName = p.Name, SalePrice = p.Price })
                .ToList();

            lblTotal.Text = list.Count.ToString();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            txtCode.Text = "";
            txtName.Text = "";
            txtPrice.Text = "";
            isEditing = false;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;

            int code = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["Code"].Value);
            var p = _service.Get(code);
            if (p == null) return;

            txtCode.Text = p.Code.ToString();
            txtName.Text = p.Name;
            txtPrice.Text = p.Price.ToString("F2");
            isEditing = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Invalid price.");
                return;
            }

            if (isEditing)
            {
                if (!int.TryParse(txtCode.Text, out int code))
                {
                    MessageBox.Show("Invalid product code.");
                    return;
                }
                var p = _service.Get(code);
                if (p == null) return;
                p.Name = txtName.Text;
                p.Price = price;
                _service.Update(p);
            }
            else
            {
                var p = new Product
                {
                    Name = txtName.Text,
                    Price = price
                };
                _service.Add(p);
            }

            LoadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;

            int code = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["Code"].Value);
            _service.Delete(code);

            LoadData();
        }
    }
}
