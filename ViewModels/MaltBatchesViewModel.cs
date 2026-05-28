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
    public class MaltBatchesViewModel : ObservableObject
    {
        private readonly IRepository<MaltBatch> _repo;
        private readonly IRepository<MaltProcess> _processRepo;
        private readonly IDialogService _dialogService;

        public MaltBatchesViewModel(
            IRepository<MaltBatch> repo,
            IRepository<MaltProcess> processRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _processRepo = processRepo;
            _dialogService = dialogService;

            MaltBatches = new ObservableCollection<MaltBatch>();
            Processes = new ObservableCollection<MaltProcess>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<MaltBatch> MaltBatches { get; }
        public ObservableCollection<MaltProcess> Processes { get; }

        private MaltBatch? _selectedBatch;
        public MaltBatch? SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                Set(ref _selectedBatch, value);
                if (value != null)
                {
                    EditQuantity = value.MaltBatch_Quantity ?? 0;
                    // Находим процесс в загруженном списке (ComboBox)
                    SelectedProcess = Processes.FirstOrDefault(p => p.MaltProcess_ID == value.MaltBatch_MaltProcess);
                }
                UpdateButtons();
            }
        }

        private double _editQuantity;
        public double EditQuantity
        {
            get => _editQuantity;
            set { Set(ref _editQuantity, value); UpdateButtons(); }
        }

        private MaltProcess? _selectedProcess;
        public MaltProcess? SelectedProcess
        {
            get => _selectedProcess;
            set { Set(ref _selectedProcess, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedBatch != null &&
            SelectedBatch.MaltBatch_ID > 0 &&
            SelectedProcess != null &&
            EditQuantity > 0;

        private bool CanAdd() =>
            SelectedProcess != null &&
            EditQuantity > 0;

        private bool CanDelete() =>
            SelectedBatch != null &&
            SelectedBatch.MaltBatch_ID > 0;

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
                // Загружаем процессы (для ComboBox)
                var processes = await _processRepo.Query()
                    .Include(p => p.MaltProcess_MaltOrderNavigation) // Чтобы видеть, к какому заказу относится
                    .AsNoTracking().ToListAsync();

                Processes.Clear();
                foreach (var p in processes) Processes.Add(p);

                // Загружаем партии солода
                var batches = await _repo.Query()
                    .Include(b => b.MaltBatch_MaltProcessNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                MaltBatches.Clear();
                foreach (var b in batches) MaltBatches.Add(b);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [MaltBatchesVM] Пропущена гонка потоков.");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newBatch = new MaltBatch
            {
                MaltBatch_MaltProcess = SelectedProcess!.MaltProcess_ID,
                MaltBatch_Quantity = EditQuantity
            };

            await _repo.AddAsync(newBatch);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedBatch == null || !CanSave()) return;

            SelectedBatch.MaltBatch_MaltProcess = SelectedProcess!.MaltProcess_ID;
            SelectedBatch.MaltBatch_Quantity = EditQuantity;

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
                    "Эта партия солода используется в учете сырья.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи сырья.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedBatch = null;
            EditQuantity = 0;
            SelectedProcess = null;
            UpdateButtons();
        }
    }
}