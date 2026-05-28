using BeerZdec.Interfaces;
using BeerZdec.Models;
using BeerZdec.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class MaltEquipTypesViewModel : ObservableObject
    {
        private readonly IRepository<MaltEquipType> _repo;
        private readonly IDialogService _dialogService;

        public MaltEquipTypesViewModel(
            IRepository<MaltEquipType> repo,
            IDialogService dialogService)
        {
            _repo = repo;
            _dialogService = dialogService;

            EquipTypes = new ObservableCollection<MaltEquipType>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<MaltEquipType> EquipTypes { get; }

        private MaltEquipType? _selectedType;
        public MaltEquipType? SelectedType
        {
            get => _selectedType;
            set
            {
                Set(ref _selectedType, value);
                if (value != null)
                {
                    EditName = value.MaltEquipType_Name ?? string.Empty;
                    EditDescr = value.MaltEquipType_Description ?? string.Empty;
                }
                UpdateButtons();
            }
        }

        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set { Set(ref _editName, value); UpdateButtons(); }
        }

        private string _editDescr = string.Empty;
        public string EditDescr
        {
            get => _editDescr;
            set { Set(ref _editDescr, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedType != null &&
            SelectedType.MaltEquipType_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanDelete() =>
            SelectedType != null &&
            SelectedType.MaltEquipType_ID > 0;

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
                var types = await _repo.Query().AsNoTracking().ToListAsync();
                EquipTypes.Clear();
                foreach (var t in types) EquipTypes.Add(t);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [MaltEquipTypesVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newType = new MaltEquipType
            {
                MaltEquipType_Name = EditName,
                MaltEquipType_Description = EditDescr
            };

            await _repo.AddAsync(newType);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedType == null || !CanSave()) return;

            SelectedType.MaltEquipType_Name = EditName;
            SelectedType.MaltEquipType_Description = EditDescr;

            await _repo.UpdateAsync(SelectedType);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedType == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedType);

            if (!success)
            {
                _dialogService.ShowError(
                    "Этот тип оборудования используется в цехе.\n" +
                    "Удалить нельзя. Сначала удалите связанное оборудование.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedType = null;
            EditName = string.Empty;
            EditDescr = string.Empty;
            UpdateButtons();
        }
    }
}