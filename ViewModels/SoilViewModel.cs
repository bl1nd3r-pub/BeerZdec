using BeerZdec.Interfaces;
using BeerZdec.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace BeerZdec.ViewModels
{
    public class SoilViewModel : ObservableObject
    {
        private readonly IRepository<SoilType> _soilRepo;
        private readonly IRepository<SoilTextureClass> _textureRepo;

        public SoilViewModel(IRepository<SoilType> soilRepo, IRepository<SoilTextureClass> textureRepo)
        {
            _soilRepo = soilRepo;
            _textureRepo = textureRepo;

            SoilTypes = new ObservableCollection<SoilType>();
            TextureClasses = new ObservableCollection<SoilTextureClass>();

            LoadCommand = new RelayCommandAsync(LoadData);

            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<SoilType> SoilTypes { get; }
        public ObservableCollection<SoilTextureClass> TextureClasses { get; }

        private SoilType? _selectedSoilType;
        public SoilType? SelectedSoilType
        {
            get => _selectedSoilType;
            set
            {
                Set(ref _selectedSoilType, value);
                if (value != null)
                {
                    EditName = value.SoilType_Name;
                    SelectedTextureClass = TextureClasses.FirstOrDefault(t => t.SoilTextureClass_ID == value.SoilType_TextureClass);
                }
                UpdateButtons();
            }
        }

        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set
            {
                Set(ref _editName, value);
                UpdateButtons();
            }
        }

        private SoilTextureClass? _selectedTextureClass;
        public SoilTextureClass? SelectedTextureClass
        {
            get => _selectedTextureClass;
            set
            {
                Set(ref _selectedTextureClass, value);
                UpdateButtons();
            }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        // "Сохранить" активна ТОЛЬКО если выбрана СУЩЕСТВУЮЩАЯ запись (ID > 0)
        private bool CanSave() =>
            SelectedSoilType != null &&
            SelectedSoilType.SoilType_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditName) &&
            SelectedTextureClass != null;

        // "Добавить" активна ТОЛЬКО если заполнено имя И выбрана текстура
        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditName) &&
            SelectedTextureClass != null &&
            (SelectedSoilType == null || SelectedSoilType.SoilType_ID == 0);

        // "Удалить" активна ТОЛЬКО если выбрана СУЩЕСТВУЮЩАЯ запись
        private bool CanDelete() =>
            SelectedSoilType != null &&
            SelectedSoilType.SoilType_ID > 0;

        // Вспомогательный метод, чтобы не писать вызовы 10 раз
        private void UpdateButtons()
        {
            AddCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        private async Task LoadData()
        {

            try
            {
                // 1. Загружаем текстуры (для выпадающего списка)
                var textures = await _textureRepo.Query().AsNoTracking().ToListAsync();
                TextureClasses.Clear();
                foreach (var t in textures) TextureClasses.Add(t);

                // 2. Загружаем типы почв
                var soils = await _soilRepo.Query()
                    .Include(s => s.SoilType_TextureClassNavigation)
                    //.AsNoTracking()
                    .ToListAsync();
                SoilTypes.Clear();
                foreach (var s in soils) SoilTypes.Add(s);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [SoilVM] Пропущена гонка потоков при загрузке данных.");
                await Task.Delay(100); // Ждём чуть-чуть
                await LoadData();      // Рекурсивно пробуем снова
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            // Создаем запись с данными из формы
            var newSoil = new SoilType
            {
                SoilType_Name = EditName,
                SoilType_TextureClass = SelectedTextureClass!.SoilTextureClass_ID
            };

            await _soilRepo.AddAsync(newSoil);
            // После добавления перезагружаем список, чтобы получить ID и связи
            await LoadData();

            // Сбрасываем форму
            EditName = string.Empty;
            SelectedTextureClass = null;
            SelectedSoilType = null; // Снимаем выделение

            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedSoilType == null || !CanSave()) return;

            SelectedSoilType.SoilType_Name = EditName;
            SelectedSoilType.SoilType_TextureClass = SelectedTextureClass!.SoilTextureClass_ID;

            //_soilRepo.Update(SelectedSoilType);
            await _soilRepo.SaveChangesAsync();

            await LoadData(); // Перезагружаем, чтобы обновить связи
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedSoilType == null || !CanDelete()) return;

            _soilRepo.Remove(SelectedSoilType);
            await _soilRepo.SaveChangesAsync();

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedSoilType = null;
            EditName = string.Empty;
            SelectedTextureClass = null;
            UpdateButtons();
        }
    }
}