using BeerZdec.Models;
using BeerZdec.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.ViewModels
{
    public class StorageViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigation;
        private User _user = null!;
        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is User c) _user = c;
        }
    }
}

