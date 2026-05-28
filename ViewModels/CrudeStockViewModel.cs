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
    public class CrudeStockViewModel : ObservableObject
    {
        private readonly IRepository<Crude> _repo;
        private readonly IRepository<MaltBatch> _maltRepo;
        private readonly IRepository<CrudeSupply> _supplyRepo;
        private readonly IDialogService _dialogService;

        public CrudeStockViewModel(
            IRepository<Crude> repo,
            IRepository<MaltBatch> maltRepo,
            IRepository<CrudeSupply> supplyRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _maltRepo = maltRepo;
            _supplyRepo = supplyRepo;
            _dialogService = dialogService;

            Crudes = new ObservableCollection<Crude>();
            MaltBatches = new ObservableCollection<MaltBatch>();
            Supplies = new ObservableCollection<CrudeSupply>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<Crude> Crudes { get; }
        public ObservableCollection<MaltBatch> MaltBatches { get; }
        public ObservableCollection<CrudeSupply> Supplies { get; }

        private Crude? _selectedCrude;
        public Crude? SelectedCrude
        {
            get => _selectedCrude;
            set
            {
                Set(ref _selectedCrude, value);
                if (value != null)
                {
                    SelectedMaltBatch = MaltBatches.FirstOrDefault(m => m.MaltBatch_ID == value.Crude_MaltBatch);
                    SelectedSupply = Supplies.FirstOrDefault(s => s.CrudeSupply_ID == value.Crude_OtherBatch);
                }
                UpdateButtons();
            }
        }

        private MaltBatch? _selectedMaltBatch;
        public MaltBatch? SelectedMaltBatch
        {
            get => _selectedMaltBatch;
            set
            {
                Set(ref _selectedMaltBatch, value);
                if (value != null) SelectedSupply = null; // Логика XOR: только один источник
                UpdateButtons();
            }
        }

        private CrudeSupply? _selectedSupply;
        public CrudeSupply? SelectedSupply
        {
            get => _selectedSupply;
            set
            {
                Set(ref _selectedSupply, value);
                if (value != null) SelectedMaltBatch = null; // Логика XOR
                UpdateButtons();
            }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Разрешаем сохранение/добавление, если выбран ровно один источник
        private bool CanSave() =>
            SelectedCrude != null &&
            SelectedCrude.Crude_ID > 0 &&
            (SelectedMaltBatch != null ^ SelectedSupply != null);

        private bool CanAdd() =>
            (SelectedMaltBatch != null ^ SelectedSupply != null);

        private bool CanDelete() =>
            SelectedCrude != null &&
            SelectedCrude.Crude_ID > 0;

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
                var malts = await _maltRepo.Query().AsNoTracking().ToListAsync();
                MaltBatches.Clear();
                foreach (var m in malts) MaltBatches.Add(m);

                var supplies = await _supplyRepo.Query().AsNoTracking().ToListAsync();
                Supplies.Clear();
                foreach (var s in supplies) Supplies.Add(s);

                var crudes = await _repo.Query()
                    .Include(c => c.Crude_MaltBatchNavigation)
                    .Include(c => c.Crude_OtherBatchNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                Crudes.Clear();
                foreach (var c in crudes) Crudes.Add(c);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [CrudeStockVM] Пропущена гонка потоков.");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newCrude = new Crude
            {
                Crude_MaltBatch = SelectedMaltBatch?.MaltBatch_ID,
                Crude_OtherBatch = SelectedSupply?.CrudeSupply_ID
            };

            await _repo.AddAsync(newCrude);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedCrude == null || !CanSave()) return;

            SelectedCrude.Crude_MaltBatch = SelectedMaltBatch?.MaltBatch_ID;
            SelectedCrude.Crude_OtherBatch = SelectedSupply?.CrudeSupply_ID;

            await _repo.UpdateAsync(SelectedCrude);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedCrude == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedCrude);

            if (!success)
            {
                _dialogService.ShowError(
                    "Это сырьё используется в рецептах варки или привязано к ячейкам склада.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedCrude = null;
            SelectedMaltBatch = null;
            SelectedSupply = null;
            UpdateButtons();
        }
    }
}