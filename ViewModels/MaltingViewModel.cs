using BeerZdec.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.ViewModels
{
    public class MaltingViewModel : ObservableObject
    {
        private readonly MaltingLinesViewModel _maltingLinesViewModel;
        private readonly MaltEquipTypesViewModel _maltEquipTypesViewModel;
        private readonly MaltEquipmentViewModel _maltEquipmentViewModel;
        private readonly MaltingOrdersViewModel _maltingOrdersViewModel;
        private readonly StorageToMaltingViewModel _storageToMaltingViewModel;
        private readonly MaltProcessesViewModel _maltProcessesViewModel;
        private readonly MaltBatchesViewModel _maltBatchesViewModel;

        // Внедряем дочерние ViewModel через DI
        public MaltingViewModel(
            MaltingLinesViewModel maltingLinesViewModel,
            MaltEquipTypesViewModel maltEquipTypesViewModel,
            MaltEquipmentViewModel maltEquipmentViewModel,
            MaltingOrdersViewModel maltingOrdersViewModel,
            StorageToMaltingViewModel storageToMaltingViewModel,
            MaltProcessesViewModel maltProcessesViewModel,
            MaltBatchesViewModel maltBatchesViewModel
            )
        {
            _maltingLinesViewModel = maltingLinesViewModel ?? throw new ArgumentNullException(nameof(maltingLinesViewModel));
            _maltEquipTypesViewModel = maltEquipTypesViewModel ?? throw new ArgumentNullException(nameof(maltEquipTypesViewModel));
            _maltEquipmentViewModel = maltEquipmentViewModel ?? throw new ArgumentNullException(nameof(maltEquipmentViewModel));
            _maltingOrdersViewModel = maltingOrdersViewModel ?? throw new ArgumentNullException(nameof(maltingOrdersViewModel));
            _storageToMaltingViewModel = storageToMaltingViewModel ?? throw new ArgumentNullException(nameof(storageToMaltingViewModel));
            _maltProcessesViewModel = maltProcessesViewModel ?? throw new ArgumentNullException(nameof(maltProcessesViewModel));
            _maltBatchesViewModel = maltBatchesViewModel ?? throw new ArgumentNullException(nameof(maltBatchesViewModel));
        }

        // Свойства, которые будут биндиться во View
        public MaltingLinesViewModel MaltingLinesContext => _maltingLinesViewModel;
        public MaltEquipTypesViewModel MaltEquipTypesContext => _maltEquipTypesViewModel;
        public MaltEquipmentViewModel MaltEquipmentContext => _maltEquipmentViewModel;
        public MaltingOrdersViewModel MaltingOrdersContext => _maltingOrdersViewModel;
        public StorageToMaltingViewModel StorageToMaltingContext => _storageToMaltingViewModel;
        public MaltProcessesViewModel MaltProcessesContext => _maltProcessesViewModel;
        public MaltBatchesViewModel MaltBatchesContext => _maltBatchesViewModel;

        public void Initialize()
        {
            _maltingLinesViewModel.LoadCommand.Execute(null);
            _maltEquipTypesViewModel.LoadCommand.Execute(null);
            _maltEquipmentViewModel.LoadCommand.Execute(null);
            _maltingOrdersViewModel.LoadCommand.Execute(null);
            _storageToMaltingViewModel.LoadCommand.Execute(null);
            _maltProcessesViewModel.LoadCommand.Execute(null);
            _maltBatchesViewModel.LoadCommand.Execute(null);
        }
    }
}