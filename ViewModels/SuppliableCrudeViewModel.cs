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
    public class SuppliableCrudeViewModel : ObservableObject
    {
        private readonly IRepository<SuppliableCrude> _repo;
        private readonly IDialogService _dialogService;

        public SuppliableCrudeViewModel(
            IRepository<SuppliableCrude> repo,
            IDialogService dialogService)
        {
            _repo = repo;
            _dialogService = dialogService;

            SuppliableCrudes = new ObservableCollection<SuppliableCrude>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<SuppliableCrude> SuppliableCrudes { get; }

        private SuppliableCrude? _selectedCrude;
        public SuppliableCrude? SelectedCrude
        {
            get => _selectedCrude;
            set
            {
                Set(ref _selectedCrude, value);
                if (value != null)
                {
                    EditName = value.SuppliableCrude_Name ?? string.Empty;
                    EditUnit = value.SuppliableCrude_MeasurementUnit ?? string.Empty;
                }
                UpdateButtons();
            }
        }

        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set { Set(ref _editName, value); UpdateButtons(); }
        }

        private string _editUnit = string.Empty;
        public string EditUnit
        {
            get => _editUnit;
            set { Set(ref _editUnit, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedCrude != null &&
            SelectedCrude.SuppliableCrude_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanDelete() =>
            SelectedCrude != null &&
            SelectedCrude.SuppliableCrude_ID > 0;

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
                var crudes = await _repo.Query().AsNoTracking().ToListAsync();
                SuppliableCrudes.Clear();
                foreach (var c in crudes) SuppliableCrudes.Add(c);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [SuppliableCrudeVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newCrude = new SuppliableCrude
            {
                SuppliableCrude_Name = EditName,
                SuppliableCrude_MeasurementUnit = EditUnit
            };

            await _repo.AddAsync(newCrude);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedCrude == null || !CanSave()) return;

            SelectedCrude.SuppliableCrude_Name = EditName;
            SelectedCrude.SuppliableCrude_MeasurementUnit = EditUnit;

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
                    "Этот тип сырья используется в журнале поставок.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи поставок.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedCrude = null;
            EditName = string.Empty;
            EditUnit = string.Empty;
            UpdateButtons();
        }
    }
}