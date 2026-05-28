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
    public class CrudeSuppliesViewModel : ObservableObject
    {
        private readonly IRepository<CrudeSupply> _repo;
        private readonly IRepository<Supplier> _supplierRepo;
        private readonly IRepository<SuppliableCrude> _crudeTypeRepo;
        private readonly IDialogService _dialogService;

        public CrudeSuppliesViewModel(
            IRepository<CrudeSupply> repo,
            IRepository<Supplier> supplierRepo,
            IRepository<SuppliableCrude> crudeTypeRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _supplierRepo = supplierRepo;
            _crudeTypeRepo = crudeTypeRepo;
            _dialogService = dialogService;

            Supplies = new ObservableCollection<CrudeSupply>();
            Suppliers = new ObservableCollection<Supplier>();
            CrudeTypes = new ObservableCollection<SuppliableCrude>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<CrudeSupply> Supplies { get; }
        public ObservableCollection<Supplier> Suppliers { get; }
        public ObservableCollection<SuppliableCrude> CrudeTypes { get; }

        private CrudeSupply? _selectedSupply;
        public CrudeSupply? SelectedSupply
        {
            get => _selectedSupply;
            set
            {
                Set(ref _selectedSupply, value);
                if (value != null)
                {
                    EditAmount = value.CrudeSupply_Amount ?? 0;
                    EditDatetime = value.CrudeSupply_Datetime ?? DateTime.Now;

                    SelectedSupplier = Suppliers.FirstOrDefault(s => s.Supplier_ID == value.CrudeSupply_Supplier);
                    SelectedCrudeType = CrudeTypes.FirstOrDefault(c => c.SuppliableCrude_ID == value.CrudeSupply_Crude);
                }
                UpdateButtons();
            }
        }

        private double _editAmount;
        public double EditAmount
        {
            get => _editAmount;
            set { Set(ref _editAmount, value); UpdateButtons(); }
        }

        private DateTime _editDatetime = DateTime.Now;
        public DateTime EditDatetime
        {
            get => _editDatetime;
            set { Set(ref _editDatetime, value); UpdateButtons(); }
        }

        private Supplier? _selectedSupplier;
        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set { Set(ref _selectedSupplier, value); UpdateButtons(); }
        }

        private SuppliableCrude? _selectedCrudeType;
        public SuppliableCrude? SelectedCrudeType
        {
            get => _selectedCrudeType;
            set { Set(ref _selectedCrudeType, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedSupply != null &&
            SelectedSupply.CrudeSupply_ID > 0 &&
            SelectedSupplier != null &&
            SelectedCrudeType != null &&
            EditAmount > 0;

        private bool CanAdd() =>
            SelectedSupplier != null &&
            SelectedCrudeType != null &&
            EditAmount > 0;

        private bool CanDelete() =>
            SelectedSupply != null &&
            SelectedSupply.CrudeSupply_ID > 0;

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
                // Справочники
                var suppliers = await _supplierRepo.Query().AsNoTracking().ToListAsync();
                Suppliers.Clear();
                foreach (var s in suppliers) Suppliers.Add(s);

                var types = await _crudeTypeRepo.Query().AsNoTracking().ToListAsync();
                CrudeTypes.Clear();
                foreach (var t in types) CrudeTypes.Add(t);

                // Основная таблица
                var records = await _repo.Query()
                    .Include(r => r.CrudeSupply_SupplierNavigation)
                    .Include(r => r.CrudeSupply_CrudeNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                Supplies.Clear();
                foreach (var r in records) Supplies.Add(r);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [CrudeSuppliesVM] Пропущена гонка потоков.");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newRecord = new CrudeSupply
            {
                CrudeSupply_Supplier = SelectedSupplier!.Supplier_ID,
                CrudeSupply_Crude = SelectedCrudeType!.SuppliableCrude_ID,
                CrudeSupply_Amount = EditAmount,
                CrudeSupply_Datetime = EditDatetime
            };

            await _repo.AddAsync(newRecord);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedSupply == null || !CanSave()) return;

            SelectedSupply.CrudeSupply_Supplier = SelectedSupplier!.Supplier_ID;
            SelectedSupply.CrudeSupply_Crude = SelectedCrudeType!.SuppliableCrude_ID;
            SelectedSupply.CrudeSupply_Amount = EditAmount;
            SelectedSupply.CrudeSupply_Datetime = EditDatetime;

            await _repo.UpdateAsync(SelectedSupply);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedSupply == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedSupply);

            if (!success)
            {
                _dialogService.ShowError(
                    "Эта поставка используется в складе сырья.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи сырья.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedSupply = null;
            EditAmount = 0;
            EditDatetime = DateTime.Now;
            SelectedSupplier = null;
            SelectedCrudeType = null;
            UpdateButtons();
        }
    }
}