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
    public class MaltingOrdersViewModel : ObservableObject
    {
        private readonly IRepository<MaltingOrder> _repo;
        private readonly IDialogService _dialogService;

        public MaltingOrdersViewModel(
            IRepository<MaltingOrder> repo,
            IDialogService dialogService)
        {
            _repo = repo;
            _dialogService = dialogService;

            MaltingOrders = new ObservableCollection<MaltingOrder>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<MaltingOrder> MaltingOrders { get; }

        private MaltingOrder? _selectedOrder;
        public MaltingOrder? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                Set(ref _selectedOrder, value);
                if (value != null)
                {
                    EditCreatedAt = value.MaltingOrder_СreatedAt.HasValue
                        ? value.MaltingOrder_СreatedAt.Value
                        : DateTime.Today;
                    EditStatus = value.MaltingOrder_Status ?? string.Empty;
                    EditTargetMaltType = value.MaltingOrder_TargetMaltType ?? string.Empty;
                }
                UpdateButtons();
            }
        }

        private DateTime _editCreatedAt = DateTime.Today;
        public DateTime EditCreatedAt
        {
            get => _editCreatedAt;
            set { Set(ref _editCreatedAt, value); UpdateButtons(); }
        }

        private string _editStatus = string.Empty;
        public string EditStatus
        {
            get => _editStatus;
            set { Set(ref _editStatus, value); UpdateButtons(); }
        }

        private string _editTargetMaltType = string.Empty;
        public string EditTargetMaltType
        {
            get => _editTargetMaltType;
            set { Set(ref _editTargetMaltType, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedOrder != null &&
            SelectedOrder.MaltingOrder_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditStatus);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditStatus);

        private bool CanDelete() =>
            SelectedOrder != null &&
            SelectedOrder.MaltingOrder_ID > 0;

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
                var orders = await _repo.Query().AsNoTracking().ToListAsync();
                MaltingOrders.Clear();
                foreach (var o in orders) MaltingOrders.Add(o);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [MaltingOrdersVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newOrder = new MaltingOrder
            {
                MaltingOrder_СreatedAt = EditCreatedAt,
                MaltingOrder_Status = EditStatus,
                MaltingOrder_TargetMaltType = EditTargetMaltType
            };

            await _repo.AddAsync(newOrder);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedOrder == null || !CanSave()) return;

            SelectedOrder.MaltingOrder_СreatedAt = EditCreatedAt;
            SelectedOrder.MaltingOrder_Status = EditStatus;
            SelectedOrder.MaltingOrder_TargetMaltType = EditTargetMaltType;

            await _repo.UpdateAsync(SelectedOrder);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedOrder == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedOrder);

            if (!success)
            {
                _dialogService.ShowError(
                    "Этот заказ используется в процессах или отгрузках.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedOrder = null;
            EditCreatedAt = DateTime.Today;
            EditStatus = string.Empty;
            EditTargetMaltType = string.Empty;
            UpdateButtons();
        }
    }
}