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
    public class MaltingLinesViewModel : ObservableObject
    {
        private readonly IRepository<MaltingLine> _repo;
        private readonly IDialogService _dialogService;

        public MaltingLinesViewModel(IRepository<MaltingLine> repo, IDialogService dialogService)
        {
            _repo = repo;
            _dialogService = dialogService;

            MaltingLines = new ObservableCollection<MaltingLine>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<MaltingLine> MaltingLines { get; }

        private MaltingLine? _selectedLine;
        public MaltingLine? SelectedLine
        {
            get => _selectedLine;
            set
            {
                Set(ref _selectedLine, value);
                if (value != null)
                {
                    EditStatus = value.MaltingLine_CurStatus ?? string.Empty;
                    EditZone = value.MaltingLine_LocationZone ?? string.Empty;
                    EditCapacity = value.MaltingLine_TotalCapacity ?? 0;
                }
                UpdateButtons();
            }
        }

        private string _editStatus = string.Empty;
        public string EditStatus
        {
            get => _editStatus;
            set { Set(ref _editStatus, value); UpdateButtons(); }
        }

        private string _editZone = string.Empty;
        public string EditZone
        {
            get => _editZone;
            set { Set(ref _editZone, value); UpdateButtons(); }
        }

        private double _editCapacity;
        public double EditCapacity
        {
            get => _editCapacity;
            set { Set(ref _editCapacity, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedLine != null &&
            SelectedLine.MaltingLine_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditStatus);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditStatus);

        private bool CanDelete() =>
            SelectedLine != null &&
            SelectedLine.MaltingLine_ID > 0;

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
                var lines = await _repo.Query().AsNoTracking().ToListAsync();
                MaltingLines.Clear();
                foreach (var l in lines) MaltingLines.Add(l);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [MaltingLinesVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newLine = new MaltingLine
            {
                MaltingLine_CurStatus = EditStatus,
                MaltingLine_LocationZone = EditZone,
                MaltingLine_TotalCapacity = EditCapacity
            };

            await _repo.AddAsync(newLine);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedLine == null || !CanSave()) return;

            SelectedLine.MaltingLine_CurStatus = EditStatus;
            SelectedLine.MaltingLine_LocationZone = EditZone;
            SelectedLine.MaltingLine_TotalCapacity = EditCapacity;

            await _repo.UpdateAsync(SelectedLine);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedLine == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedLine);

            if (!success)
            {
                _dialogService.ShowError(
                    "Нельзя удалить линию, так как к ней привязано оборудование.\n" +
                    "Сначала удалите связанное оборудование.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedLine = null;
            EditStatus = string.Empty;
            EditZone = string.Empty;
            EditCapacity = 0;
            UpdateButtons();
        }
    }
}