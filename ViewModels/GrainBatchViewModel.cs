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
    public class GrainBatchViewModel : ObservableObject
    {
        private readonly IRepository<GrainBatch> _repo;
        private readonly IRepository<HarvestEvent> _harvestRepo;
        private readonly IRepository<GBStatus> _statusRepo;
        private readonly IRepository<GBQualGrade> _gradeRepo;
        private readonly IDialogService _dialogService;

        public GrainBatchViewModel(
            IRepository<GrainBatch> repo,
            IRepository<HarvestEvent> harvestRepo,
            IRepository<GBStatus> statusRepo,
            IRepository<GBQualGrade> gradeRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _harvestRepo = harvestRepo;
            _statusRepo = statusRepo;
            _gradeRepo = gradeRepo;
            _dialogService = dialogService;

            GrainBatches = new ObservableCollection<GrainBatch>();
            HarvestEvents = new ObservableCollection<HarvestEvent>();
            GBStatuses = new ObservableCollection<GBStatus>();
            GBQualGrades = new ObservableCollection<GBQualGrade>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<GrainBatch> GrainBatches { get; }
        public ObservableCollection<HarvestEvent> HarvestEvents { get; }
        public ObservableCollection<GBStatus> GBStatuses { get; }
        public ObservableCollection<GBQualGrade> GBQualGrades { get; }

        private GrainBatch? _selectedBatch;
        public GrainBatch? SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                Set(ref _selectedBatch, value);
                if (value != null)
                {
                    EditMoisture = value.GB_Moisture ?? 0;
                    EditForeignMatter = value.GB_ForeignMatter ?? 0;
                    EditWeightReceived = value.GB_WeightReceived ?? 0;

                    SelectedHarvest = HarvestEvents.FirstOrDefault(h => h.HarvestEvent_ID == value.GB_Harvest);
                    SelectedStatus = GBStatuses.FirstOrDefault(s => s.GBStatus_ID == value.GB_Status);
                    SelectedGrade = GBQualGrades.FirstOrDefault(g => g.GBQualGrade_ID == value.GB_QualGrade);
                }
                UpdateButtons();
            }
        }

        private double _editMoisture;
        public double EditMoisture
        {
            get => _editMoisture;
            set { Set(ref _editMoisture, value); UpdateButtons(); }
        }

        private double _editForeignMatter;
        public double EditForeignMatter
        {
            get => _editForeignMatter;
            set { Set(ref _editForeignMatter, value); UpdateButtons(); }
        }

        private double _editWeightReceived;
        public double EditWeightReceived
        {
            get => _editWeightReceived;
            set { Set(ref _editWeightReceived, value); UpdateButtons(); }
        }

        private HarvestEvent? _selectedHarvest;
        public HarvestEvent? SelectedHarvest
        {
            get => _selectedHarvest;
            set { Set(ref _selectedHarvest, value); UpdateButtons(); }
        }

        private GBStatus? _selectedStatus;
        public GBStatus? SelectedStatus
        {
            get => _selectedStatus;
            set { Set(ref _selectedStatus, value); UpdateButtons(); }
        }

        private GBQualGrade? _selectedGrade;
        public GBQualGrade? SelectedGrade
        {
            get => _selectedGrade;
            set { Set(ref _selectedGrade, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedBatch != null &&
            SelectedBatch.GB_ID > 0 &&
            SelectedHarvest != null &&
            SelectedStatus != null &&
            SelectedGrade != null &&
            EditWeightReceived > 0;

        private bool CanAdd() =>
            SelectedHarvest != null &&
            SelectedStatus != null &&
            SelectedGrade != null &&
            EditWeightReceived > 0;

        private bool CanDelete() =>
            SelectedBatch != null &&
            SelectedBatch.GB_ID > 0;

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
                // Загружаем справочники
                var harvests = await _harvestRepo.Query()
                    .Include(h => h.HarvestEvent_SowPlotNavigation)
                    .AsNoTracking().ToListAsync();
                HarvestEvents.Clear();
                foreach (var h in harvests) HarvestEvents.Add(h);

                var statuses = await _statusRepo.Query().AsNoTracking().ToListAsync();
                GBStatuses.Clear();
                foreach (var s in statuses) GBStatuses.Add(s);

                var grades = await _gradeRepo.Query().AsNoTracking().ToListAsync();
                GBQualGrades.Clear();
                foreach (var g in grades) GBQualGrades.Add(g);

                // Загружаем партии
                var batches = await _repo.Query()
                    .Include(b => b.GB_HarvestNavigation)
                    .Include(b => b.GB_StatusNavigation)
                    .Include(b => b.GB_QualGradeNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                GrainBatches.Clear();
                foreach (var b in batches) GrainBatches.Add(b);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [GrainBatchVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newBatch = new GrainBatch
            {
                GB_Harvest = SelectedHarvest!.HarvestEvent_ID,
                GB_Status = SelectedStatus!.GBStatus_ID,
                GB_QualGrade = SelectedGrade!.GBQualGrade_ID,
                GB_Moisture = EditMoisture,
                GB_ForeignMatter = EditForeignMatter,
                GB_WeightReceived = EditWeightReceived
            };

            await _repo.AddAsync(newBatch);
            await LoadData();

            EditMoisture = 0;
            EditForeignMatter = 0;
            EditWeightReceived = 0;
            SelectedHarvest = null;
            SelectedStatus = null;
            SelectedGrade = null;
            SelectedBatch = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedBatch == null || !CanSave()) return;

            SelectedBatch.GB_Harvest = SelectedHarvest!.HarvestEvent_ID;
            SelectedBatch.GB_Status = SelectedStatus!.GBStatus_ID;
            SelectedBatch.GB_QualGrade = SelectedGrade!.GBQualGrade_ID;
            SelectedBatch.GB_Moisture = EditMoisture;
            SelectedBatch.GB_ForeignMatter = EditForeignMatter;
            SelectedBatch.GB_WeightReceived = EditWeightReceived;

            await _repo.UpdateAsync(SelectedBatch);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedBatch == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedBatch);

            if (!success)
            {
                _dialogService.ShowError(
                    "Эта партия используется в перемещениях.\n" +
                    "Удалить нельзя. Сначала удалите связанные перемещения.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedBatch = null;
            EditMoisture = 0;
            EditForeignMatter = 0;
            EditWeightReceived = 0;
            SelectedHarvest = null;
            SelectedStatus = null;
            SelectedGrade = null;
            UpdateButtons();
        }
    }
}