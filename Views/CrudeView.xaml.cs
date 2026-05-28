using System.Windows.Controls;
using BeerZdec.ViewModels;

namespace BeerZdec.Views
{
    public partial class CrudeView : UserControl
    {
        private bool _isInitialized = false;

        public CrudeView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_isInitialized) return;

            if (DataContext is CrudeViewModel vm)
            {
                vm.SuppliersContext.LoadCommand.Execute(null);
                vm.SuppliableCrudeContext.LoadCommand.Execute(null);
                vm.CrudeSuppliesContext.LoadCommand.Execute(null);
                vm.CrudeStockContext.LoadCommand.Execute(null);
                vm.WareCellsContext.LoadCommand.Execute(null);

                _isInitialized = true;
            }
        }
    }
}