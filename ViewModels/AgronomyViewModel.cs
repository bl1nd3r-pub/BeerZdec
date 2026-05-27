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

        // Внедряем дочерний ViewModel через DI
        public AgronomyViewModel(SoilViewModel soilViewModel, SoilTextureViewModel textureViewModel)
        {
            _soilViewModel = soilViewModel ?? throw new ArgumentNullException(nameof(soilViewModel));
            _textureViewModel = textureViewModel ?? throw new ArgumentNullException(nameof(textureViewModel));
        }

        // Свойство, которое мы будем передавать во View
        public SoilViewModel SoilContext => _soilViewModel;
        public SoilTextureViewModel TextureContext => _textureViewModel;

        public void Initialize()
        {
             _soilViewModel.LoadCommand.Execute(null);
             _textureViewModel.LoadCommand.Execute(null);
        }
    }
}
