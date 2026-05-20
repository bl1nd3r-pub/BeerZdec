using BeerZdec.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.ViewModels
{
    public class AboutViewModel : ObservableObject
    {
        private readonly IAppInfoService _appInfoService;

        private string _aboutText;
        public string AboutText
        {
            get => _aboutText;
            set => Set(ref _aboutText, value);
        }
        public AboutViewModel(IAppInfoService appInfoService)
        {
            _appInfoService = appInfoService;

            // Загружаем текст при создании
            AboutText = _appInfoService.GetAboutText();
        }


    }
}
