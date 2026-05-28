using BeerZdec.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.ViewModels
{
    public class CrudeViewModel : ObservableObject
    {
        // Пока заглушки для будущих ViewModel
        private readonly SuppliersViewModel _suppliersViewModel;
        private readonly SuppliableCrudeViewModel _suppliableCrudeViewModel;
        private readonly CrudeSuppliesViewModel _crudeSuppliesViewModel;
        //private readonly CrudeViewModel _crudeOnStockViewModel;
        //private readonly WareCellsViewModel _wareCellsViewModel;

        // Внедряем дочерние ViewModel через DI
        public CrudeViewModel(
            SuppliersViewModel suppliersViewModel,
            SuppliableCrudeViewModel suppliableCrudeViewModel,
            CrudeSuppliesViewModel crudeSuppliesViewModel
            //CrudeViewModel crudeOnStockViewModel,
            //WareCellsViewModel wareCellsViewModel
            )
        {
            _suppliersViewModel = suppliersViewModel ?? throw new ArgumentNullException(nameof(suppliersViewModel));
            _suppliableCrudeViewModel = suppliableCrudeViewModel ?? throw new ArgumentNullException(nameof(suppliableCrudeViewModel));
            _crudeSuppliesViewModel = crudeSuppliesViewModel ?? throw new ArgumentNullException(nameof(crudeSuppliesViewModel));
            // _crudeOnStockViewModel = crudeOnStockViewModel ?? throw new ArgumentNullException(nameof(crudeOnStockViewModel));
            // _wareCellsViewModel = wareCellsViewModel ?? throw new ArgumentNullException(nameof(wareCellsViewModel));
        }

        // Свойства, которые будут биндиться во View
        public SuppliersViewModel SuppliersContext => _suppliersViewModel;
        public SuppliableCrudeViewModel SuppliableCrudeContext => _suppliableCrudeViewModel;
        public CrudeSuppliesViewModel CrudeSuppliesContext => _crudeSuppliesViewModel;
        // public CrudeViewModel CrudeOnStockContext => _crudeOnStockViewModel;
        // public WareCellsViewModel WareCellsContext => _wareCellsViewModel;

        public void Initialize()
        {
            _suppliersViewModel.LoadCommand.Execute(null);
            _suppliableCrudeViewModel.LoadCommand.Execute(null);
            _crudeSuppliesViewModel.LoadCommand.Execute(null);
            // _crudeOnStockViewModel.LoadCommand.Execute(null);
            // _wareCellsViewModel.LoadCommand.Execute(null);
        }
    }
}