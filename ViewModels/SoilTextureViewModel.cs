using BeerZdec.Interfaces;
using BeerZdec.Models;
using BeerZdec.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace BeerZdec.ViewModels
{
    public class SoilTextureViewModel : ObservableObject
    {
        private readonly IRepository<SoilTextureClass> _repo;
        private readonly IRepository<SoilType> _soilRepo;
        private readonly IDialogService _dialogService;

        public SoilTextureViewModel(
            IRepository<SoilTextureClass> repo,
            IRepository<SoilType> soilRepo,
            IDialogService dialogService
            )
        {
            _repo = repo;
            _soilRepo = soilRepo;
            _dialogService = dialogService;

            TextureClasses = new ObservableCollection<SoilTextureClass>();

            LoadCommand = new RelayCommandAsync(LoadData);

            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        private ObservableCollection<SoilTextureClass> _textureClasses;
        public ObservableCollection<SoilTextureClass> TextureClasses
        {
            get => _textureClasses;
            set => Set(ref _textureClasses, value);
        }

        private SoilTextureClass? _selectedTexture;
        public SoilTextureClass? SelectedTexture
        {
            get => _selectedTexture;
            set
            {
                Set(ref _selectedTexture, value);
                if (value != null)
                {
                    EditName = value.SoilTextureClass_Name;
                }

                SaveCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                AddCommand.RaiseCanExecuteChanged();
            }
        }

        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set
            {
                Set(ref _editName, value);
                AddCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedTexture != null &&
            SelectedTexture.SoilTextureClass_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditName) &&
            (SelectedTexture == null || SelectedTexture.SoilTextureClass_ID == 0);

        private bool CanDelete() =>
            SelectedTexture != null &&
            SelectedTexture.SoilTextureClass_ID > 0;

        private async Task LoadData()
        {

            try
            {
                var textures = await _repo.Query().AsNoTracking().ToListAsync();
                TextureClasses.Clear();
                foreach (var t in textures) TextureClasses.Add(t);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [SoilTextureVM] Пропущена гонка потоков при загрузке данных.");
                await Task.Delay(100); // Ждём
                await LoadData();      // Рекурсивно пробуем снова
            }
        }

        private async Task AddNew()
        {
            if (string.IsNullOrWhiteSpace(EditName)) return;

            var newTexture = new SoilTextureClass
            {
                SoilTextureClass_Name = EditName
            };

            await _repo.AddAsync(newTexture);

            var newList = new ObservableCollection<SoilTextureClass>(TextureClasses) { newTexture };
            TextureClasses = newList;

            EditName = string.Empty;
            SelectedTexture = null;

            AddCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        private async Task SaveData()
        {
            if (SelectedTexture == null || !CanSave()) return;

            SelectedTexture.SoilTextureClass_Name = EditName;

            await _repo.UpdateAsync(SelectedTexture);

            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedTexture == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedTexture);

            if (!success)
            {
                _dialogService.ShowError(
                    "Этот класс текстуры используется в типах почв.\n" +
                    "Удалить нельзя. Сначала удалите или измените связанные типы почв.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedTexture = null;
            EditName = string.Empty;

            SaveCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            AddCommand.RaiseCanExecuteChanged();
        }
    }
}