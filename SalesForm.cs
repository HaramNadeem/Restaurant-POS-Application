using Microsoft.VisualBasic;
using RestaurantPOS.Models;
using RestaurantPOS.Service;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;



namespace RestaurantPOS.UI
{
    public partial class SalesForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;

        private BindingList<SaleItem> currentItems = new BindingList<SaleItem>();

        public SalesForm(IProductService productService, ISaleService saleService)
        {
            _productService = productService;
            _saleService = saleService;

            InitializeComponent();

            dgvBill.AutoGenerateColumns = false;
            dgvBill.DataSource = currentItems;

            timerClock.Tick += timerClock_Tick;
            timerClock.Start();

            LoadProducts();
            SetupItemsListGrid();
            SetupBillGridColumns();

            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            dtpDate.Value = DateTime.Now;
            txtBillNo.Text = "1";

            dgvItemsList.SelectionChanged += dgvItemsList_SelectionChanged;
            dgvBill.CellDoubleClick += dgvBill_CellDoubleClick;
        }

        private void LoadProducts()
        {
            dgvItemsList.DataSource = _productService.GetAll().ToList();
        }

        private void SetupItemsListGrid()
        {
            dgvItemsList.Columns.Clear();

            dgvItemsList.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Code",
                HeaderText = "Code",
                Width = 80,
                ReadOnly = true
            });

            dgvItemsList.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Name",
                HeaderText = "Item Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            dgvItemsList.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Price",
                HeaderText = "Price",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvItemsList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItemsList.MultiSelect = false;
        }

        private void SetupBillGridColumns()
        {
            dgvBill.Columns.Clear();

            dgvBill.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductCode",
                HeaderText = "Code",
                Width = 80,
                ReadOnly = true
            });

            dgvBill.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ProductName",
                HeaderText = "Item Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            dgvBill.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Quantity",
                HeaderText = "Qty",
                Width = 60,
                ReadOnly = true
            });

            dgvBill.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DiscountCash",
                HeaderText = "Discount Cash",
                Width = 90,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvBill.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DiscountPercent",
                HeaderText = "Discount %",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvBill.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "FinalAmount",
                HeaderText = "Total",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2" }
            });

            dgvBill.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBill.MultiSelect = false;
        }

        private void dgvItemsList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvItemsList.CurrentRow == null) return;

            dynamic p = dgvItemsList.CurrentRow.DataBoundItem;
            if (p == null) return;

            txtCode.Text = p.Code.ToString();
            txtItem.Text = p.Name?.ToString() ?? "";
            txtPrice.Text = Convert.ToDecimal(p.Price).ToString("F2");

            numQty.Value = 1;
            txtDiscountCash.Text = "0.00";
            txtDiscountPercent.Text = "0.00";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtCode.Text, out int code) ||
                string.IsNullOrWhiteSpace(txtItem.Text) ||
                !decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please select a valid product.");
                return;
            }

            decimal.TryParse(txtDiscountCash.Text, out decimal discountCash);
            decimal.TryParse(txtDiscountPercent.Text, out decimal discountPercent);
            int quantity = (int)numQty.Value;

            var item = new SaleItem
            {
                ProductCode = code,
                ProductName = txtItem.Text,
                Quantity = quantity,
                Price = price,
                DiscountCash = discountCash,
                DiscountPercent = discountPercent
            };

            currentItems.Add(item);
            UpdateTotals();
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (currentItems.Count == 0)
            {
                MessageBox.Show("No items to save.");
                return;
            }

            // Save Sale
            int saleId = _saleService.CreateSale(dtpDate.Value, currentItems.ToList());
            MessageBox.Show($"Sale #{saleId} saved successfully!");

            // Ask user
            DialogResult choice = MessageBox.Show(
                "Do you want to save a Word invoice also?\n\nYes = Report Only\nNo = Report + Word File",
                "Save Bill",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // Folder
            string folder = Application.StartupPath + "\\Bills";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string wordFile = Path.Combine(folder, $"Bill_{saleId}.docx");

            // Always save report
            SaveReportOnly(saleId);

            // Save to Word if user selects No
            if (choice == DialogResult.No)
            {
                SaveToWord(wordFile, saleId);
                MessageBox.Show("Bill saved to Word successfully!");
            }

            // Reset UI
            currentItems.Clear();
            UpdateTotals();

            if (int.TryParse(txtBillNo.Text, out int b))
                txtBillNo.Text = (b + 1).ToString();
        }

        private void SaveReportOnly(int saleId)
        {
            File.WriteAllText(
                $"Report_{saleId}.txt",
                $"Sale Report #{saleId}\nDate: {DateTime.Now}\nItems: {currentItems.Count}"
            );
        }


        private void btnDiscount_Click(object sender, EventArgs e)
        {
            if (dgvBill.CurrentRow == null)
            {
                MessageBox.Show("Please select an item from the bill to apply discount.");
                return;
            }

            var item = dgvBill.CurrentRow.DataBoundItem as SaleItem;
            if (item == null) return;

            // Ask user for discount type and value
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter discount (use number for cash discount, or number% for percentage):",
                "Apply Discount",
                "0");

            if (string.IsNullOrWhiteSpace(input)) return;

            try
            {
                if (input.EndsWith("%"))
                {
                    string numStr = input.TrimEnd('%');
                    if (decimal.TryParse(numStr, out decimal percent))
                    {
                        item.DiscountPercent = percent;
                    }
                    else
                    {
                        MessageBox.Show("Invalid percentage.");
                    }
                }
                else
                {
                    if (decimal.TryParse(input, out decimal cash))
                    {
                        item.DiscountCash = cash;
                    }
                    else
                    {
                        MessageBox.Show("Invalid cash amount.");
                    }
                }

                // Refresh the grid
                dgvBill.Refresh();
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying discount: " + ex.Message);
            }
        }

        private void dgvBill_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var item = dgvBill.Rows[e.RowIndex].DataBoundItem as SaleItem;
            if (item == null) return;

            txtCode.Text = item.ProductCode.ToString();
            txtItem.Text = item.ProductName;
            txtPrice.Text = item.Price.ToString("F2");
            numQty.Value = item.Quantity;
            txtDiscountCash.Text = item.DiscountCash.ToString("F2");
            txtDiscountPercent.Text = item.DiscountPercent.ToString("F2");

            currentItems.Remove(item);
            UpdateTotals();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBill.CurrentRow == null) return;

            var item = dgvBill.CurrentRow.DataBoundItem as SaleItem;
            if (item != null)
            {
                currentItems.Remove(item);
                UpdateTotals();
            }
        }

        private void SaveToWord(string path, int saleId)
        {
            Word.Application word = new Word.Application();
            Word.Document doc = word.Documents.Add();

            // 1️⃣ Header - Restaurant Name
            Word.Paragraph restName = doc.Paragraphs.Add();
            restName.Range.Text = "EATS AND BITES";
            restName.Range.Font.Size = 36;           // Correct size
            restName.Range.Font.Bold = 1;
            restName.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            restName.Range.InsertParagraphAfter();

            // 2️⃣ Bill Info - Bill No and Date
            Word.Paragraph header = doc.Paragraphs.Add();
            header.Range.Text = $"Bill No: {saleId}";
            header.Range.Font.Size = 14;
            header.Range.Font.Bold = 0;
            header.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            header.Range.InsertParagraphAfter();

            Word.Paragraph datePara = doc.Paragraphs.Add();
            datePara.Range.Text = $"Date: {dtpDate.Value:dd-MM-yyyy}";
            datePara.Range.Font.Size = 14;
            datePara.Range.Font.Bold = 0;
            datePara.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            datePara.Range.InsertParagraphAfter();

            // 3️⃣ Table — items + header row
            Word.Table tbl = doc.Tables.Add(
                doc.Content.Paragraphs.Last.Range,
                currentItems.Count + 1,
                4
            );

            // Table Headers
            tbl.Cell(1, 1).Range.Text = "Item";
            tbl.Cell(1, 2).Range.Text = "Qty";
            tbl.Cell(1, 3).Range.Text = "Price";
            tbl.Cell(1, 4).Range.Text = "Total";

            for (int i = 1; i <= 4; i++)
            {
                tbl.Cell(1, i).Range.Font.Bold = 1;
                tbl.Cell(1, i).Range.Font.Size = 14;   // Correct font size for table header
            }

            // Fill rows
            decimal grandTotal = 0;
            int r = 2;

            foreach (var item in currentItems)
            {
                tbl.Cell(r, 1).Range.Text = item.ProductName;
                tbl.Cell(r, 2).Range.Text = item.Quantity.ToString();
                tbl.Cell(r, 3).Range.Text = item.Price.ToString("0.00");
                tbl.Cell(r, 4).Range.Text = item.FinalAmount.ToString("0.00");

                // Set font size for each cell
                for (int c = 1; c <= 4; c++)
                    tbl.Cell(r, c).Range.Font.Size = 14;

                grandTotal += item.FinalAmount;
                r++;
            }

            // 4️⃣ Total Paragraph
            Word.Paragraph totalPara = doc.Paragraphs.Add();
            totalPara.Range.Text = $"Grand Total: {grandTotal:0.00}";
            totalPara.Range.Font.Size = 14;       // Correct font
            totalPara.Range.Font.Bold = 1;
            totalPara.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            totalPara.Range.InsertParagraphAfter();

            // Save and close
            doc.SaveAs2(path);
            doc.Close();
            word.Quit();
        }




        private void UpdateTotals()
        {
            lblTotalItems.Text = currentItems.Sum(i => i.Quantity).ToString();
            lblTotalAmount.Text = currentItems.Sum(i => i.FinalAmount).ToString("F2");
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void btnAddNewProduct_Click(object sender, EventArgs e)
        {
            // Open ProductsForm as dialog
            using (var pf = new ProductsForm(_productService))
            {
                pf.ShowDialog();
            }

            // Refresh product list after closing ProductsForm
            LoadProducts();
        }
    }
}
