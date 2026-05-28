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
    public class WareCellsViewModel : ObservableObject
    {
        private readonly IRepository<WareCell> _repo;
        private readonly IRepository<Crude> _crudeRepo;
        private readonly IDialogService _dialogService;

        public WareCellsViewModel(
            IRepository<WareCell> repo,
            IRepository<Crude> crudeRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _crudeRepo = crudeRepo;
            _dialogService = dialogService;

            WareCells = new ObservableCollection<WareCell>();
            Crudes = new ObservableCollection<Crude>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<WareCell> WareCells { get; }
        public ObservableCollection<Crude> Crudes { get; }

        private WareCell? _selectedCell;
        public WareCell? SelectedCell
        {
            get => _selectedCell;
            set
            {
                Set(ref _selectedCell, value);
                if (value != null)
                {
                    EditMaxCapacity = value.WareCell_MaxCapacity ?? 0;
                    EditCurOccup = value.WareCell_CurOccup ?? 0;
                    EditCondition = value.WareCell_Condition ?? string.Empty;

                    // Находим сырьё для ComboBox
                    SelectedCrude = Crudes.FirstOrDefault(c => c.Crude_ID == value.WareCell_Crude);
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

        private string _editCondition = string.Empty;
        public string EditCondition
        {
            get => _editCondition;
            set { Set(ref _editCondition, value); UpdateButtons(); }
        }

        private Crude? _selectedCrude;
        public Crude? SelectedCrude
        {
            get => _selectedCrude;
            set { Set(ref _selectedCrude, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedCell != null &&
            SelectedCell.WareCell_ID > 0 &&
            EditMaxCapacity > 0 &&
            EditCurOccup <= EditMaxCapacity;

        private bool CanAdd() =>
            EditMaxCapacity > 0 &&
            EditCurOccup >= 0 &&
            EditCurOccup <= EditMaxCapacity;

        private bool CanDelete() =>
            SelectedCell != null &&
            SelectedCell.WareCell_ID > 0;

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
                // Загружаем сырьё (для привязки ячейки)
                var crudes = await _crudeRepo.Query().AsNoTracking().ToListAsync();
                Crudes.Clear();
                foreach (var c in crudes) Crudes.Add(c);

                // Загружаем ячейки
                var cells = await _repo.Query()
                    .Include(c => c.WareCell_CrudeNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                WareCells.Clear();
                foreach (var c in cells) WareCells.Add(c);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [WareCellsVM] Пропущена гонка потоков.");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newCell = new WareCell
            {
                WareCell_Crude = SelectedCrude?.Crude_ID, // Может быть null
                WareCell_MaxCapacity = EditMaxCapacity,
                WareCell_CurOccup = EditCurOccup,
                WareCell_Condition = EditCondition
            };

            await _repo.AddAsync(newCell);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedCell == null || !CanSave()) return;

            SelectedCell.WareCell_Crude = SelectedCrude?.Crude_ID;
            SelectedCell.WareCell_MaxCapacity = EditMaxCapacity;
            SelectedCell.WareCell_CurOccup = EditCurOccup;
            SelectedCell.WareCell_Condition = EditCondition;

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
                    "Эта ячейка используется в производственных процессах.\n" +
                    "Удалить нельзя.",
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
            EditCondition = string.Empty;
            SelectedCrude = null;
            UpdateButtons();
        }
    }
}