using BeerZdec.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BeerZdec.ViewModels
{
    public class AgronomyViewModel : ObservableObject
    {
        private readonly SoilViewModel _soilViewModel;
        private readonly SoilTextureViewModel _textureViewModel;
        private readonly VarietyViewModel _varietyViewModel;
        private readonly GrainViewModel _grainViewModel;
        private readonly SowingPlotViewModel _sowingPlotViewModel;
        private readonly SowingProcessViewModel _sowingProcessViewModel;
        private readonly StorageCellViewModel _storageCellViewModel;
        private readonly HarvestEventViewModel _harvestEventViewModel;
        private readonly GrainBatchViewModel _grainBatchViewModel;
        private readonly StorageMoveViewModel _storageMoveViewModel;

        // Внедряем дочерний ViewModel через DI
        public AgronomyViewModel(
            SoilViewModel soilViewModel,
            SoilTextureViewModel textureViewModel,
            VarietyViewModel varietyViewModel,
            GrainViewModel grainViewModel,
            SowingPlotViewModel sowingPlotViewModel,
            SowingProcessViewModel sowingProcessViewModel,
            StorageCellViewModel storageCellViewModel,
            HarvestEventViewModel harvestEventViewModel,
            GrainBatchViewModel grainBatchViewModel,
            StorageMoveViewModel storageMoveViewModel
            )
        {
            _soilViewModel = soilViewModel ?? throw new ArgumentNullException(nameof(soilViewModel));
            _textureViewModel = textureViewModel ?? throw new ArgumentNullException(nameof(textureViewModel));
            _varietyViewModel = varietyViewModel ?? throw new ArgumentNullException(nameof(varietyViewModel));
            _grainViewModel = grainViewModel ?? throw new ArgumentNullException(nameof(grainViewModel));
            _sowingPlotViewModel = sowingPlotViewModel ?? throw new ArgumentNullException(nameof(sowingPlotViewModel));
            _sowingProcessViewModel = sowingProcessViewModel ?? throw new ArgumentNullException(nameof(sowingProcessViewModel));
            _storageCellViewModel = storageCellViewModel ?? throw new ArgumentNullException(nameof(storageCellViewModel));
            _harvestEventViewModel = harvestEventViewModel ?? throw new ArgumentNullException(nameof(harvestEventViewModel));
            _grainBatchViewModel = grainBatchViewModel ?? throw new ArgumentNullException(nameof(grainBatchViewModel));
            _storageMoveViewModel = storageMoveViewModel ?? throw new ArgumentNullException(nameof(storageMoveViewModel));
        }

        // Свойства, которые мы будем передавать во View
        public SoilViewModel SoilContext => _soilViewModel;
        public SoilTextureViewModel TextureContext => _textureViewModel;
        public VarietyViewModel VarietyContext => _varietyViewModel;
        public GrainViewModel GrainContext => _grainViewModel;
        public SowingPlotViewModel SowingPlotContext => _sowingPlotViewModel;
        public SowingProcessViewModel SowingProcessContext => _sowingProcessViewModel;
        public StorageCellViewModel StorageCellContext => _storageCellViewModel;
        public HarvestEventViewModel HarvestEventContext => _harvestEventViewModel;
        public GrainBatchViewModel GrainBatchContext => _grainBatchViewModel;
        public StorageMoveViewModel StorageMoveContext => _storageMoveViewModel;


        public void Initialize()
        {
            _soilViewModel.LoadCommand.Execute(null);
            _textureViewModel.LoadCommand.Execute(null);
            _varietyViewModel.LoadCommand.Execute(null);
            _grainViewModel.LoadCommand.Execute(null);
            _sowingPlotViewModel.LoadCommand.Execute(null);
            _sowingProcessViewModel.LoadCommand.Execute(null);
            _storageCellViewModel.LoadCommand.Execute(null);
            _harvestEventViewModel.LoadCommand.Execute(null);
            _grainBatchViewModel.LoadCommand.Execute(null);
            _storageMoveViewModel.LoadCommand.Execute(null);

        }
    }
}
