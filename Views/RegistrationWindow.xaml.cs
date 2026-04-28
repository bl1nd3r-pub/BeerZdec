using BeerZdec.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BeerZdec.Views
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            using var db = new AppDbContext();

            if (db.Users.Any(u => u.UsLogin == LoginTextBox.Text))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!");
                return;
            }

            var user = new User
            {
                UsLogin = LoginTextBox.Text,
                UsPassword = PasswordBox.Password,
                Role = "User"
            };

            db.Users.Add(user);
            db.SaveChanges();

            MessageBox.Show("Регистрация успешна!");
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
