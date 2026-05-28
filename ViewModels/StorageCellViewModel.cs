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
    public class StorageCellViewModel : ObservableObject
    {
        private readonly IRepository<StorageCell> _repo;
        private readonly IDialogService _dialogService;

        public StorageCellViewModel(
            IRepository<StorageCell> repo,
            IDialogService dialogService)
        {
            _repo = repo;
            _dialogService = dialogService;

            StorageCells = new ObservableCollection<StorageCell>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<StorageCell> StorageCells { get; }

        private StorageCell? _selectedCell;
        public StorageCell? SelectedCell
        {
            get => _selectedCell;
            set
            {
                Set(ref _selectedCell, value);
                if (value != null)
                {
                    EditMaxCapacity = value.Storage_MaxCapacity ?? 0;
                    EditCurOccup = value.Storage_CurOccup ?? 0;
                    EditCondition = value.Storage_Condition ?? 0;
                }
                UpdateButtons();
            }
        }

        private double _editMaxCapacity;
        public double EditMaxCapacity
        {
            get => _editMaxCapacity;
            set { Set(ref _editMaxCapacity, value); UpdateButtons(); }
        }

        private double _editCurOccup;
        public double EditCurOccup
        {
            get => _editCurOccup;
            set { Set(ref _editCurOccup, value); UpdateButtons(); }
        }

        private double _editCondition;
        public double EditCondition
        {
            get => _editCondition;
            set { Set(ref _editCondition, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedCell != null &&
            SelectedCell.Storage_ID > 0 &&
            EditMaxCapacity > 0 &&
            EditCurOccup <= EditMaxCapacity;

        private bool CanAdd() =>
            EditMaxCapacity > 0 &&
            EditCurOccup >= 0 &&
            EditCurOccup <= EditMaxCapacity;

        private bool CanDelete() =>
            SelectedCell != null &&
            SelectedCell.Storage_ID > 0;

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
                var cells = await _repo.Query().AsNoTracking().ToListAsync();
                StorageCells.Clear();
                foreach (var c in cells) StorageCells.Add(c);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [StorageCellVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            var newCell = new StorageCell
            {
                Storage_MaxCapacity = EditMaxCapacity,
                Storage_CurOccup = EditCurOccup,
                Storage_Condition = EditCondition
            };

            await _repo.AddAsync(newCell);
            await LoadData();

            EditMaxCapacity = 0;
            EditCurOccup = 0;
            EditCondition = 0;
            SelectedCell = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedCell == null || !CanSave()) return;

            SelectedCell.Storage_MaxCapacity = EditMaxCapacity;
            SelectedCell.Storage_CurOccup = EditCurOccup;
            SelectedCell.Storage_Condition = EditCondition;

            await _repo.UpdateAsync(SelectedCell);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedCell == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedCell);

            if (!success)
            {
                _dialogService.ShowError(
                    "Эта ячейка используется в перемещениях или отгрузках.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedCell = null;
            EditMaxCapacity = 0;
            EditCurOccup = 0;
            EditCondition = 0;
            UpdateButtons();
        }
    }
}