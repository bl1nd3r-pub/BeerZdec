using System.Windows.Controls;
using BeerZdec.ViewModels;

namespace BeerZdec.Views
{
    public partial class MaltingView : UserControl
    {
        private bool _isInitialized = false;

        public MaltingView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_isInitialized) return;

            if (DataContext is MaltingViewModel vm)
            {
                vm.MaltingLinesContext.LoadCommand.Execute(null);
                vm.MaltEquipTypesContext.LoadCommand.Execute(null);
                vm.MaltEquipmentContext.LoadCommand.Execute(null);
                vm.MaltingOrdersContext.LoadCommand.Execute(null);
                vm.StorageToMaltingContext.LoadCommand.Execute(null);
                vm.MaltProcessesContext.LoadCommand.Execute(null);

                _isInitialized = true;
            }
        }
    }
}