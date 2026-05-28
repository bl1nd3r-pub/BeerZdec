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
    public class StorageMoveViewModel : ObservableObject
    {
        private readonly IRepository<StorageMove> _repo;
        private readonly IRepository<GrainBatch> _batchRepo;
        private readonly IRepository<StorageCell> _cellRepo;
        private readonly IRepository<Employee> _empRepo;
        private readonly IDialogService _dialogService;

        public StorageMoveViewModel(
            IRepository<StorageMove> repo,
            IRepository<GrainBatch> batchRepo,
            IRepository<StorageCell> cellRepo,
            IRepository<Employee> empRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _batchRepo = batchRepo;
            _cellRepo = cellRepo;
            _empRepo = empRepo;
            _dialogService = dialogService;

            StorageMoves = new ObservableCollection<StorageMove>();
            GrainBatches = new ObservableCollection<GrainBatch>();
            StorageCells = new ObservableCollection<StorageCell>();
            Employees = new ObservableCollection<Employee>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<StorageMove> StorageMoves { get; }
        public ObservableCollection<GrainBatch> GrainBatches { get; }
        public ObservableCollection<StorageCell> StorageCells { get; }
        public ObservableCollection<Employee> Employees { get; }

        private StorageMove? _selectedMove;
        public StorageMove? SelectedMove
        {
            get => _selectedMove;
            set
            {
                Set(ref _selectedMove, value);
                if (value != null)
                {
                    EditWeight = value.StorageMoves_Weight ?? 0;

                    // Партия
                    SelectedBatch = GrainBatches.FirstOrDefault(b => b.GB_ID == value.StorageMoves_GrainBatch);

                    // Ячейка ОТКУДА (если null, ищем наш виртуальный ID=0)
                    var fromId = value.StorageMoves_FromStorage ?? 0;
                    SelectedFromCell = StorageCells.FirstOrDefault(c => c.Storage_ID == fromId);

                    // Ячейка КУДА
                    SelectedToCell = StorageCells.FirstOrDefault(c => c.Storage_ID == value.StorageMoves_ToStorage);

                    // Сотрудник
                    SelectedEmployee = Employees.FirstOrDefault(e => e.Emp_ID == value.StorageMoves_MovedBy);
                }
                UpdateButtons();
            }
        }

        private double _editWeight;
        public double EditWeight
        {
            get => _editWeight;
            set { Set(ref _editWeight, value); UpdateButtons(); }
        }

        private GrainBatch? _selectedBatch;
        public GrainBatch? SelectedBatch
        {
            get => _selectedBatch;
            set { Set(ref _selectedBatch, value); UpdateButtons(); }
        }

        private StorageCell? _selectedFromCell;
        public StorageCell? SelectedFromCell
        {
            get => _selectedFromCell;
            set { Set(ref _selectedFromCell, value); UpdateButtons(); }
        }

        private StorageCell? _selectedToCell;
        public StorageCell? SelectedToCell
        {
            get => _selectedToCell;
            set { Set(ref _selectedToCell, value); UpdateButtons(); }
        }

        private Employee? _selectedEmployee;
        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set { Set(ref _selectedEmployee, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedMove != null &&
            SelectedMove.StorageMoves_Zapis_ID > 0 &&
            SelectedBatch != null &&
            SelectedToCell != null && // Куда перемещаем - обязательно
            SelectedEmployee != null &&
            EditWeight > 0;

        private bool CanAdd() =>
            SelectedBatch != null &&
            SelectedToCell != null &&
            SelectedEmployee != null &&
            EditWeight > 0;

        private bool CanDelete() =>
            SelectedMove != null &&
            SelectedMove.StorageMoves_Zapis_ID > 0;

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
                // 1. Загружаем справочники
                var batches = await _batchRepo.Query()
                    .Include(b => b.GB_HarvestNavigation)
                    .Include(b => b.GB_StatusNavigation)
                    .AsNoTracking().ToListAsync();
                GrainBatches.Clear();
                foreach (var b in batches) GrainBatches.Add(b);

                var cells = await _cellRepo.Query().AsNoTracking().ToListAsync();
                StorageCells.Clear();

                // 2. МАГИЯ: Добавляем виртуальную ячейку "Внешний источник" (ID = 0)
                // Это позволит выбрать null для FromStorage
                StorageCells.Add(new StorageCell
                {
                    Storage_ID = 0,
                    Storage_MaxCapacity = 0,
                    Storage_CurOccup = 0,
                    Storage_Condition = 0
                });

                foreach (var c in cells) StorageCells.Add(c);

                var emps = await _empRepo.Query().AsNoTracking().ToListAsync();
                Employees.Clear();
                foreach (var e in emps) Employees.Add(e);

                // 3. Загружаем перемещения
                var moves = await _repo.Query()
                    .Include(m => m.StorageMoves_GrainBatchNavigation)
                    .Include(m => m.StorageMoves_FromStorageNavigation)
                    .Include(m => m.StorageMoves_ToStorageNavigation)
                    .Include(m => m.StorageMoves_MovedByNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                StorageMoves.Clear();
                foreach (var m in moves) StorageMoves.Add(m);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [StorageMoveVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            // Если выбран ID 0 (наш виртуальный), пишем null
            int? fromId = SelectedFromCell?.Storage_ID == 0 ? null : SelectedFromCell?.Storage_ID;

            var newMove = new StorageMove
            {
                StorageMoves_GrainBatch = SelectedBatch!.GB_ID,
                StorageMoves_FromStorage = fromId,
                StorageMoves_ToStorage = SelectedToCell!.Storage_ID,
                StorageMoves_MovedBy = SelectedEmployee!.Emp_ID,
                StorageMoves_Weight = EditWeight
            };

            await _repo.AddAsync(newMove);
            await LoadData();

            EditWeight = 0;
            SelectedBatch = null;
            SelectedFromCell = null;
            SelectedToCell = null;
            SelectedEmployee = null;
            SelectedMove = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedMove == null || !CanSave()) return;

            int? fromId = SelectedFromCell?.Storage_ID == 0 ? null : SelectedFromCell?.Storage_ID;

            SelectedMove.StorageMoves_GrainBatch = SelectedBatch!.GB_ID;
            SelectedMove.StorageMoves_FromStorage = fromId;
            SelectedMove.StorageMoves_ToStorage = SelectedToCell!.Storage_ID;
            SelectedMove.StorageMoves_MovedBy = SelectedEmployee!.Emp_ID;
            SelectedMove.StorageMoves_Weight = EditWeight;

            await _repo.UpdateAsync(SelectedMove);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedMove == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedMove);

            if (!success)
            {
                _dialogService.ShowError("Не удалось удалить запись перемещения.", "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedMove = null;
            EditWeight = 0;
            SelectedBatch = null;
            SelectedFromCell = null;
            SelectedToCell = null;
            SelectedEmployee = null;
            UpdateButtons();
        }
    }
}