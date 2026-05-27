using System.Windows.Controls;
using BeerZdec.ViewModels;

namespace BeerZdec.Views
{
    public partial class AgronomyView : UserControl
    {
        private bool _isInitialized = false;

        public AgronomyView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_isInitialized) return;

            if (DataContext is AgronomyViewModel vm)
            {
                // Загружаем данные для первой вкладки (по умолчанию активной)
                vm.TextureContext.LoadCommand.Execute(null);
                vm.SoilContext.LoadCommand.Execute(null);
                _isInitialized = true;
            }
        }
    }
}