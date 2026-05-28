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

        // Внедряем дочерний ViewModel через DI
        public AgronomyViewModel(SoilViewModel soilViewModel, SoilTextureViewModel textureViewModel, VarietyViewModel varietyViewModel, GrainViewModel grainViewModel)
        {
            _soilViewModel = soilViewModel ?? throw new ArgumentNullException(nameof(soilViewModel));
            _textureViewModel = textureViewModel ?? throw new ArgumentNullException(nameof(textureViewModel));
            _varietyViewModel = varietyViewModel ?? throw new ArgumentNullException(nameof(varietyViewModel));
            _grainViewModel = grainViewModel ?? throw new ArgumentNullException(nameof(grainViewModel));
        }

        // Свойство, которое мы будем передавать во View
        public SoilViewModel SoilContext => _soilViewModel;
        public SoilTextureViewModel TextureContext => _textureViewModel;
        public VarietyViewModel VarietyContext => _varietyViewModel;
        public GrainViewModel GrainContext => _grainViewModel;

        public void Initialize()
        {
            _soilViewModel.LoadCommand.Execute(null);
            _textureViewModel.LoadCommand.Execute(null);
            _varietyViewModel.LoadCommand.Execute(null);
            _grainViewModel.LoadCommand.Execute(null);
        }
    }
}
