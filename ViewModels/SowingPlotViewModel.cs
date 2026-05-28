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
    public class SowingPlotViewModel : ObservableObject
    {
        private readonly IRepository<SowingPlot> _repo;
        private readonly IRepository<SoilType> _soilRepo;
        private readonly IDialogService _dialogService;

        public SowingPlotViewModel(IRepository<SowingPlot> repo, IRepository<SoilType> soilRepo, IDialogService dialogService)
        {
            _repo = repo;
            _soilRepo = soilRepo;
            _dialogService = dialogService;

            SowingPlots = new ObservableCollection<SowingPlot>();
            SoilTypes = new ObservableCollection<SoilType>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<SowingPlot> SowingPlots { get; }
        public ObservableCollection<SoilType> SoilTypes { get; }

        private SowingPlot? _selectedPlot;
        public SowingPlot? SelectedPlot
        {
            get => _selectedPlot;
            set
            {
                Set(ref _selectedPlot, value);
                if (value != null)
                {
                    EditSquare = value.SowPlot_Square ?? 0;
                    SelectedSoilType = SoilTypes.FirstOrDefault(s => s.SoilType_ID == value.SowPlot_SoilType);
                }
                UpdateButtons();
            }
        }

        private double _editSquare;
        public double EditSquare
        {
            get => _editSquare;
            set { Set(ref _editSquare, value); UpdateButtons(); }
        }

        private SoilType? _selectedSoilType;
        public SoilType? SelectedSoilType
        {
            get => _selectedSoilType;
            set { Set(ref _selectedSoilType, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() => SelectedPlot != null && SelectedPlot.SowingPlot_ID > 0 && SelectedSoilType != null;
        private bool CanAdd() => SelectedSoilType != null && EditSquare > 0;
        private bool CanDelete() => SelectedPlot != null && SelectedPlot.SowingPlot_ID > 0;

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
                var soils = await _soilRepo.Query().AsNoTracking().ToListAsync();
                SoilTypes.Clear();
                foreach (var s in soils) SoilTypes.Add(s);

                var plots = await _repo.Query()
                    .Include(p => p.SowPlot_SoilTypeNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                SowingPlots.Clear();
                foreach (var p in plots) SowingPlots.Add(p);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [SowingPlotVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newPlot = new SowingPlot
            {
                SowPlot_SoilType = SelectedSoilType!.SoilType_ID,
                SowPlot_Square = EditSquare,
                SowPlot_IrrigationSystemType = 1
            };

            await _repo.AddAsync(newPlot);
            await LoadData();

            EditSquare = 0;
            SelectedSoilType = null;
            SelectedPlot = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedPlot == null || !CanSave()) return;

            SelectedPlot.SowPlot_SoilType = SelectedSoilType!.SoilType_ID;
            SelectedPlot.SowPlot_Square = EditSquare;

            await _repo.UpdateAsync(SelectedPlot);

            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedPlot == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedPlot);

            if (!success)
            {
                _dialogService.ShowError(
                    "Этот участок имеет записи об уборке урожая.\n" +
                    "Удалить нельзя. Сначала удалите связанные события уборки.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedPlot = null;
            EditSquare = 0;
            SelectedSoilType = null;
            UpdateButtons();
        }
    }
}