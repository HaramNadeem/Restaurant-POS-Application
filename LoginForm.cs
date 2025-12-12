using RestaurantPOS.Service;
using System;
using System.Windows.Forms;

namespace RestaurantPOS.UI
{
    public partial class LoginForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly AuthService _auth;

        public LoginForm(IProductService productService, ISaleService saleService, AuthService auth)
        {
            _productService = productService;
            _saleService = saleService;
            _auth = auth;
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (_auth.Validate(txtUser.Text.Trim(), txtPass.Text))
            {
                var main = new MainForm(_productService, _saleService, _auth);

                this.Hide();
                main.FormClosed += (s, ev) => this.Close();
                main.Show();
            }
            else
            {
                MessageBox.Show("Invalid credentials.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            using (var cp = new ChangePasswordForm(_auth))
            {
                cp.ShowDialog();
            }
        }
    }
}
