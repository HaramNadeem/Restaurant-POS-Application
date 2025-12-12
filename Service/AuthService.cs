using System;

namespace RestaurantPOS.Service
{
    // Simple in-memory auth service.
    // For production: replace with hashed passwords and secure store.
    public class AuthService
    {
        private string _username = "admin";
        private string _password = "admin"; // default

        public bool Validate(string username, string password)
        {
            return string.Equals(username, _username, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(password, _password);
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!Validate(username, oldPassword)) return false;
            _password = newPassword;
            return true;
        }

        public string Username => _username;
    }
}
