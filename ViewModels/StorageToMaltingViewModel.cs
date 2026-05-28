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
    public class StorageToMaltingViewModel : ObservableObject
    {
        private readonly IRepository<StorageToMalting> _repo;
        private readonly IRepository<MaltingOrder> _orderRepo;
        private readonly IRepository<StorageCell> _cellRepo;
        private readonly IDialogService _dialogService;

        public StorageToMaltingViewModel(
            IRepository<StorageToMalting> repo,
            IRepository<MaltingOrder> orderRepo,
            IRepository<StorageCell> cellRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _orderRepo = orderRepo;
            _cellRepo = cellRepo;
            _dialogService = dialogService;

            StorageToMaltings = new ObservableCollection<StorageToMalting>();
            MaltingOrders = new ObservableCollection<MaltingOrder>();
            StorageCells = new ObservableCollection<StorageCell>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<StorageToMalting> StorageToMaltings { get; }
        public ObservableCollection<MaltingOrder> MaltingOrders { get; }
        public ObservableCollection<StorageCell> StorageCells { get; }

        private StorageToMalting? _selectedRecord;
        public StorageToMalting? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                Set(ref _selectedRecord, value);
                if (value != null)
                {
                    EditQuantity = value.STM_Quantity ?? 0;
                    EditDatetime = value.STM_Datetime ?? DateTime.Today;

                    SelectedOrder = MaltingOrders.FirstOrDefault(o => o.MaltingOrder_ID == value.STM_MaltOrder);
                    SelectedCell = StorageCells.FirstOrDefault(c => c.Storage_ID == value.STM_Storage);
                }
                UpdateButtons();
            }
        }

        private double _editQuantity;
        public double EditQuantity
        {
            get => _editQuantity;
            set { Set(ref _editQuantity, value); UpdateButtons(); }
        }

        private DateTime _editDatetime = DateTime.Today;
        public DateTime EditDatetime
        {
            get => _editDatetime;
            set { Set(ref _editDatetime, value); UpdateButtons(); }
        }

        private MaltingOrder? _selectedOrder;
        public MaltingOrder? SelectedOrder
        {
            get => _selectedOrder;
            set { Set(ref _selectedOrder, value); UpdateButtons(); }
        }

        private StorageCell? _selectedCell;
        public StorageCell? SelectedCell
        {
            get => _selectedCell;
            set { Set(ref _selectedCell, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedRecord != null &&
            SelectedRecord.STM_Zapis_ID > 0 &&
            SelectedOrder != null &&
            SelectedCell != null &&
            EditQuantity > 0;

        private bool CanAdd() =>
            SelectedOrder != null &&
            SelectedCell != null &&
            EditQuantity > 0;

        private bool CanDelete() =>
            SelectedRecord != null &&
            SelectedRecord.STM_Zapis_ID > 0;

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
                // Загружаем заказы (например, можно фильтровать по активным статусам)
                var orders = await _orderRepo.Query().AsNoTracking().ToListAsync();
                MaltingOrders.Clear();
                foreach (var o in orders) MaltingOrders.Add(o);

                // Загружаем ячейки
                var cells = await _cellRepo.Query().AsNoTracking().ToListAsync();
                StorageCells.Clear();
                foreach (var c in cells) StorageCells.Add(c);

                // Загружаем отгрузки
                var records = await _repo.Query()
                    .Include(r => r.STM_MaltOrderNavigation)
                    .Include(r => r.STM_StorageNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                StorageToMaltings.Clear();
                foreach (var r in records) StorageToMaltings.Add(r);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [StorageToMaltingVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newRecord = new StorageToMalting
            {
                STM_MaltOrder = SelectedOrder!.MaltingOrder_ID,
                STM_Storage = SelectedCell!.Storage_ID,
                STM_Quantity = EditQuantity,
                STM_Datetime = EditDatetime
            };

            await _repo.AddAsync(newRecord);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedRecord == null || !CanSave()) return;

            SelectedRecord.STM_MaltOrder = SelectedOrder!.MaltingOrder_ID;
            SelectedRecord.STM_Storage = SelectedCell!.Storage_ID;
            SelectedRecord.STM_Quantity = EditQuantity;
            SelectedRecord.STM_Datetime = EditDatetime;

            await _repo.UpdateAsync(SelectedRecord);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedRecord == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedRecord);

            if (!success)
            {
                _dialogService.ShowError(
                    "Не удалось удалить запись отгрузки.\n" +
                    "Возможно, на неё есть ссылки.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedRecord = null;
            EditQuantity = 0;
            EditDatetime = DateTime.Today;
            SelectedOrder = null;
            SelectedCell = null;
            UpdateButtons();
        }
    }
}