using RestaurantPOS.Service;
using System;
using System.Windows.Forms;

namespace RestaurantPOS.UI
{
    public partial class ChangePasswordForm : Form
    {
        private readonly AuthService _auth;

        public ChangePasswordForm(AuthService auth)
        {
            _auth = auth;
            InitializeComponent();
            txtUsername.Text = _auth.Username;
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            var user = txtUsername.Text.Trim();
            var oldP = txtOldPassword.Text;
            var newP = txtNewPassword.Text;
            var confirm = txtConfirm.Text;

            if (string.IsNullOrWhiteSpace(newP) || newP != confirm)
            {
                MessageBox.Show("New password and confirmation do not match.");
                return;
            }

            if (_auth.ChangePassword(user, oldP, newP))
            {
                MessageBox.Show("Password changed.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid old password.");
            }
        }
    }
}
