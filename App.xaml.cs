using BeerZdec.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BeerZdec
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
