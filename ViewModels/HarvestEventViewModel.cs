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
    public class HarvestEventViewModel : ObservableObject
    {
        private readonly IRepository<HarvestEvent> _repo;
        private readonly IRepository<SowingPlot> _plotRepo;
        private readonly IDialogService _dialogService;

        public HarvestEventViewModel(
            IRepository<HarvestEvent> repo,
            IRepository<SowingPlot> plotRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _plotRepo = plotRepo;
            _dialogService = dialogService;

            HarvestEvents = new ObservableCollection<HarvestEvent>();
            SowingPlots = new ObservableCollection<SowingPlot>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<HarvestEvent> HarvestEvents { get; }
        public ObservableCollection<SowingPlot> SowingPlots { get; }

        private HarvestEvent? _selectedEvent;
        public HarvestEvent? SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                Set(ref _selectedEvent, value);
                if (value != null)
                {
                    EditDate = value.HarvestEvent_Date.HasValue
                        ? new DateTime(value.HarvestEvent_Date.Value.Year, value.HarvestEvent_Date.Value.Month, value.HarvestEvent_Date.Value.Day)
                        : (DateTime?)DateTime.Today;
                    EditWeight = value.HarvestEvent_GrossWeight ?? 0;
                    SelectedPlot = SowingPlots.FirstOrDefault(p => p.SowingPlot_ID == value.HarvestEvent_SowPlot);
                }
                UpdateButtons();
            }
        }

        private DateTime? _editDate;
        public DateTime? EditDate
        {
            get => _editDate;
            set { Set(ref _editDate, value); UpdateButtons(); }
        }

        private double _editWeight;
        public double EditWeight
        {
            get => _editWeight;
            set { Set(ref _editWeight, value); UpdateButtons(); }
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
            SelectedEvent != null &&
            SelectedEvent.HarvestEvent_ID > 0 &&
            SelectedPlot != null &&
            EditWeight > 0;

        private bool CanAdd() =>
            SelectedPlot != null &&
            EditWeight > 0;

        private bool CanDelete() =>
            SelectedEvent != null &&
            SelectedEvent.HarvestEvent_ID > 0;

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
                var plots = await _plotRepo.Query().AsNoTracking().ToListAsync();
                SowingPlots.Clear();
                foreach (var p in plots) SowingPlots.Add(p);

                var events = await _repo.Query()
                    .Include(e => e.HarvestEvent_SowPlotNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                HarvestEvents.Clear();
                foreach (var e in events) HarvestEvents.Add(e);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [HarvestEventVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newEvent = new HarvestEvent
            {
                HarvestEvent_Date = EditDate.HasValue ? DateOnly.FromDateTime(EditDate.Value) : DateOnly.FromDateTime(DateTime.Today),
                HarvestEvent_GrossWeight = EditWeight,
                HarvestEvent_SowPlot = SelectedPlot!.SowingPlot_ID
            };

            await _repo.AddAsync(newEvent);
            await LoadData();

            EditDate = DateTime.Today;
            EditWeight = 0;
            SelectedPlot = null;
            SelectedEvent = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedEvent == null || !CanSave()) return;

            SelectedEvent.HarvestEvent_Date = EditDate.HasValue ? DateOnly.FromDateTime(EditDate.Value) : null;
            SelectedEvent.HarvestEvent_GrossWeight = EditWeight;
            SelectedEvent.HarvestEvent_SowPlot = SelectedPlot!.SowingPlot_ID;

            await _repo.UpdateAsync(SelectedEvent);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedEvent == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedEvent);

            if (!success)
            {
                _dialogService.ShowError(
                    "Это событие уборки используется в партиях зерна.\n" +
                    "Удалить нельзя. Сначала удалите связанные партии.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedEvent = null;
            EditDate = DateTime.Today;
            EditWeight = 0;
            SelectedPlot = null;
            UpdateButtons();
        }
    }
}