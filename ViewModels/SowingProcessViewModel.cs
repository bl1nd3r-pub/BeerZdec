using BeerZdec.Interfaces;
using BeerZdec.Models;
using BeerZdec.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeerZdec.ViewModels
{
    public class SowingProcessViewModel : ObservableObject
    {
        private readonly IRepository<SowingProcess> _repo;
        private readonly IRepository<Variety> _varietyRepo;
        private readonly IRepository<SowingPlot> _plotRepo;
        private readonly IDialogService _dialogService;

        public SowingProcessViewModel(
            IRepository<SowingProcess> repo,
            IRepository<Variety> varietyRepo,
            IRepository<SowingPlot> plotRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _varietyRepo = varietyRepo;
            _plotRepo = plotRepo;
            _dialogService = dialogService;

            SowingProcesses = new ObservableCollection<SowingProcess>();
            Varieties = new ObservableCollection<Variety>();
            SowingPlots = new ObservableCollection<SowingPlot>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<SowingProcess> SowingProcesses { get; }
        public ObservableCollection<Variety> Varieties { get; }
        public ObservableCollection<SowingPlot> SowingPlots { get; }

        private SowingProcess? _selectedProcess;
        public SowingProcess? SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                Set(ref _selectedProcess, value);
                if (value != null)
                {
                    EditDate = value.SowProc_Datetime;
                    SelectedVariety = Varieties.FirstOrDefault(v => v.Variety_ID == value.SowProc_Variety);
                    SelectedPlot = SowingPlots.FirstOrDefault(p => p.SowingPlot_ID == value.SowProc_SowPlot);
                }
                UpdateButtons();
            }
        }

        private DateOnly _editDate;
        public DateOnly EditDate
        {
            get => _editDate;
            set { Set(ref _editDate, value); UpdateButtons(); }
        }

        private Variety? _selectedVariety;
        public Variety? SelectedVariety
        {
            get => _selectedVariety;
            set { Set(ref _selectedVariety, value); UpdateButtons(); }
        }

        private SowingPlot? _selectedPlot;
        public SowingPlot? SelectedPlot
        {
            get => _selectedPlot;
            set { Set(ref _selectedPlot, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedProcess != null &&
            SelectedProcess.SowingProcess_ID > 0 &&
            SelectedVariety != null &&
            SelectedPlot != null;

        private bool CanAdd() =>
            SelectedVariety != null &&
            SelectedPlot != null;

        private bool CanDelete() =>
            SelectedProcess != null &&
            SelectedProcess.SowingProcess_ID > 0;

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
                // Загружаем справочники для ComboBox
                var varieties = await _varietyRepo.Query().AsNoTracking().ToListAsync();
                Varieties.Clear();
                foreach (var v in varieties) Varieties.Add(v);

                var plots = await _plotRepo.Query()
                    .Include(p => p.SowPlot_SoilTypeNavigation)
                    .AsNoTracking()
                    .ToListAsync();
                SowingPlots.Clear();
                foreach (var p in plots) SowingPlots.Add(p);

                // Загружаем процессы посева
                var processes = await _repo.Query()
                    .Include(p => p.SowProc_VarietyNavigation)
                    .Include(p => p.SowProc_SowPlotNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                SowingProcesses.Clear();
                foreach (var p in processes) SowingProcesses.Add(p);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [SowingProcessVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newProcess = new SowingProcess
            {
                SowProc_Datetime = EditDate == default ? DateOnly.FromDateTime(DateTime.Today) : EditDate,
                SowProc_Variety = SelectedVariety!.Variety_ID,
                SowProc_SowPlot = SelectedPlot!.SowingPlot_ID
            };

            await _repo.AddAsync(newProcess);
            await LoadData();

            EditDate = default;
            SelectedVariety = null;
            SelectedPlot = null;
            SelectedProcess = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedProcess == null || !CanSave()) return;

            SelectedProcess.SowProc_Datetime = EditDate;
            SelectedProcess.SowProc_Variety = SelectedVariety!.Variety_ID;
            SelectedProcess.SowProc_SowPlot = SelectedPlot!.SowingPlot_ID;

            await _repo.UpdateAsync(SelectedProcess);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedProcess == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedProcess);

            if (!success)
            {
                _dialogService.ShowError(
                    "Эта запись посева используется в других таблицах.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedProcess = null;
            EditDate = default;
            SelectedVariety = null;
            SelectedPlot = null;
            UpdateButtons();
        }
    }
}