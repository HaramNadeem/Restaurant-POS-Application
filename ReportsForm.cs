using RestaurantPOS.Models;
using RestaurantPOS.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantPOS.UI
{
    public partial class ReportsForm : Form
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;

        // Local cache of sales for the selected date
        private List<Sale> _salesForDate = new List<Sale>();

        public ReportsForm(ISaleService saleService, IProductService productService)
        {
            _saleService = saleService;
            _productService = productService;
            InitializeComponent();

            // events
            btnLoad.Click += btnLoad_Click;
            dgvSales.SelectionChanged += dgvSales_SelectionChanged;

            // initial setup
            dtpDate.Value = DateTime.Now.Date;
            lblTotalBills.Text = "0";
            lblTotalAmount.Text = "0.00";

            SetupSalesGrid();
            SetupItemsGrid();
        }

        private void SetupSalesGrid()
        {
            dgvSales.AutoGenerateColumns = false;
            dgvSales.Columns.Clear();

            dgvSales.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Bill No",
                Name = "colBillNo",
                Width = 80,
                ReadOnly = true
            });

            dgvSales.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Total",
                HeaderText = "Bill Amount",
                Name = "colBillAmount",
                Width = 160,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvSales.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemsCount",
                HeaderText = "Items in Bill",
                Name = "colItemsCount",
                Width = 120,
                ReadOnly = true
            });

            dgvSales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSales.MultiSelect = false;
            dgvSales.RowHeadersVisible = false;
        }

        private void SetupItemsGrid()
        {
            dgvItems.AutoGenerateColumns = false;
            dgvItems.Columns.Clear();

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProductCode",
                HeaderText = "Code",
                Name = "colItemCode",
                Width = 80,
                ReadOnly = true
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProductName",
                HeaderText = "Item Name",
                Name = "colItemName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Pieces",
                Name = "colItemPieces",
                Width = 80,
                ReadOnly = true
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalDiscount",
                HeaderText = "T.Discount",
                Name = "colItemTDiscount",
                Width = 110,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FinalAmount",
                HeaderText = "Amount",
                Name = "colItemAmount",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvItems.RowHeadersVisible = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.MultiSelect = false;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadSalesForDate();
        }

        private void LoadSalesForDate()
        {
            DateTime date = dtpDate.Value.Date;

            _salesForDate.Clear();
            lblTotalBills.Text = "0";
            lblTotalAmount.Text = "0.00";
            dgvSales.DataSource = null;
            dgvItems.DataSource = null;
            ClearBillDetails();

            var raw = _saleService.GetSalesByDate(date);

            if (raw == null)
            {
                MessageBox.Show("No sales data.");
                return;
            }

            // raw is List<Tuple<Sale, IEnumerable<SaleItem>>>
            var list = new List<Sale>();
            foreach (var t in raw)
            {
                var s = t.Item1;
                var items = t.Item2?.ToList() ?? new List<SaleItem>();
                s.Items = items;
                list.Add(s);
            }

            _salesForDate = list;

            var salesForGrid = _salesForDate.Select(s => new
            {
                Id = s.Id,
                Total = s.Total,
                ItemsCount = s.Items?.Count ?? 0
            }).ToList();

            dgvSales.DataSource = salesForGrid;

            lblTotalBills.Text = salesForGrid.Count.ToString();
            lblTotalAmount.Text = salesForGrid.Sum(x => x.Total).ToString("N2");
        }

        private void dgvSales_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSales.CurrentRow == null)
            {
                dgvItems.DataSource = null;
                ClearBillDetails();
                return;
            }

            var row = dgvSales.CurrentRow.DataBoundItem;
            if (row == null) return;

            int selectedId = 0;
            try
            {
                var prop = row.GetType().GetProperty("Id");
                if (prop != null)
                    selectedId = Convert.ToInt32(prop.GetValue(row));
            }
            catch
            {
                selectedId = 0;
            }

            var sale = _salesForDate.FirstOrDefault(x => x.Id == selectedId);
            if (sale == null)
            {
                dgvItems.DataSource = null;
                ClearBillDetails();
                return;
            }

            txtBillNo.Text = sale.Id.ToString();
            txtBillItems.Text = (sale.Items?.Count ?? 0).ToString();
            txtBillAmount.Text = sale.Total.ToString("N2");
            txtBillDate.Text = sale.Date.ToString("yyyy-MM-dd");

            var itemsForGrid = sale.Items?.Select(i => new
            {
                ProductCode = i.ProductCode,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                TotalDiscount = i.TotalDiscount,
                FinalAmount = i.FinalAmount
            }).ToList();

            dgvItems.DataSource = itemsForGrid;
        }

        private void ClearBillDetails()
        {
            txtBillNo.Text = "0";
            txtBillItems.Text = "0";
            txtBillAmount.Text = "0.00";
            txtBillDate.Text = "";
        }
    }
}
