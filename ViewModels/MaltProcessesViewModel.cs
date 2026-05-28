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
    public class MaltProcessesViewModel : ObservableObject
    {
        private readonly IRepository<MaltProcess> _repo;
        private readonly IRepository<MaltingOrder> _orderRepo;
        private readonly IRepository<MaltingLine> _lineRepo;
        private readonly IRepository<Employee> _empRepo;
        private readonly IDialogService _dialogService;

        public MaltProcessesViewModel(
            IRepository<MaltProcess> repo,
            IRepository<MaltingOrder> orderRepo,
            IRepository<MaltingLine> lineRepo,
            IRepository<Employee> empRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _orderRepo = orderRepo;
            _lineRepo = lineRepo;
            _empRepo = empRepo;
            _dialogService = dialogService;

            Processes = new ObservableCollection<MaltProcess>();
            Orders = new ObservableCollection<MaltingOrder>();
            Lines = new ObservableCollection<MaltingLine>();
            Employees = new ObservableCollection<Employee>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<MaltProcess> Processes { get; }
        public ObservableCollection<MaltingOrder> Orders { get; }
        public ObservableCollection<MaltingLine> Lines { get; }
        public ObservableCollection<Employee> Employees { get; }

        private MaltProcess? _selectedProcess;
        public MaltProcess? SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                Set(ref _selectedProcess, value);
                if (value != null)
                {
                    EditStartTime = value.MaltProcess_StartTime ?? DateTime.Now;
                    EditEndTime = value.MaltProcess_EndTime ?? DateTime.Now;
                    SelectedOrder = Orders.FirstOrDefault(o => o.MaltingOrder_ID == value.MaltProcess_MaltOrder);
                    SelectedLine = Lines.FirstOrDefault(l => l.MaltingLine_ID == value.MaltProcess_MaltLine);
                    SelectedTechnologist = Employees.FirstOrDefault(e => e.Emp_ID == value.MaltProcess_Technologist);
                }
                UpdateButtons();
            }
        }

        private DateTime _editStartTime = DateTime.Now;
        public DateTime EditStartTime
        {
            get => _editStartTime;
            set { Set(ref _editStartTime, value); UpdateButtons(); }
        }

        private DateTime _editEndTime = DateTime.Now;
        public DateTime EditEndTime
        {
            get => _editEndTime;
            set { Set(ref _editEndTime, value); UpdateButtons(); }
        }

        private MaltingOrder? _selectedOrder;
        public MaltingOrder? SelectedOrder
        {
            get => _selectedOrder;
            set { Set(ref _selectedOrder, value); UpdateButtons(); }
        }

        private MaltingLine? _selectedLine;
        public MaltingLine? SelectedLine
        {
            get => _selectedLine;
            set { Set(ref _selectedLine, value); UpdateButtons(); }
        }

        private Employee? _selectedTechnologist;
        public Employee? SelectedTechnologist
        {
            get => _selectedTechnologist;
            set { Set(ref _selectedTechnologist, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedProcess != null &&
            SelectedProcess.MaltProcess_ID > 0 &&
            SelectedOrder != null &&
            SelectedLine != null &&
            SelectedTechnologist != null &&
            EditEndTime >= EditStartTime;

        private bool CanAdd() =>
            SelectedOrder != null &&
            SelectedLine != null &&
            SelectedTechnologist != null &&
            EditEndTime >= EditStartTime;

        private bool CanDelete() =>
            SelectedProcess != null &&
            SelectedProcess.MaltProcess_ID > 0;

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
                var orders = await _orderRepo.Query().AsNoTracking().ToListAsync();
                Orders.Clear(); foreach (var o in orders) Orders.Add(o);

                var lines = await _lineRepo.Query().AsNoTracking().ToListAsync();
                Lines.Clear(); foreach (var l in lines) Lines.Add(l);

                var emps = await _empRepo.Query().AsNoTracking().ToListAsync();
                Employees.Clear(); foreach (var e in emps) Employees.Add(e);

                var processes = await _repo.Query()
                    .Include(p => p.MaltProcess_MaltOrderNavigation)
                    .Include(p => p.MaltProcess_MaltLineNavigation)
                    .Include(p => p.MaltProcess_TechnologistNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                Processes.Clear();
                foreach (var p in processes) Processes.Add(p);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [MaltProcessesVM] Пропущена гонка потоков.");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newProcess = new MaltProcess
            {
                MaltProcess_MaltOrder = SelectedOrder!.MaltingOrder_ID,
                MaltProcess_MaltLine = SelectedLine!.MaltingLine_ID,
                MaltProcess_Technologist = SelectedTechnologist!.Emp_ID,
                MaltProcess_StartTime = EditStartTime,
                MaltProcess_EndTime = EditEndTime
            };

            await _repo.AddAsync(newProcess);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedProcess == null || !CanSave()) return;

            SelectedProcess.MaltProcess_MaltOrder = SelectedOrder!.MaltingOrder_ID;
            SelectedProcess.MaltProcess_MaltLine = SelectedLine!.MaltingLine_ID;
            SelectedProcess.MaltProcess_Technologist = SelectedTechnologist!.Emp_ID;
            SelectedProcess.MaltProcess_StartTime = EditStartTime;
            SelectedProcess.MaltProcess_EndTime = EditEndTime;

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
                    "Этот процесс используется в партиях солода.\n" +
                    "Удалить нельзя. Сначала удалите связанные партии.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedProcess = null;
            EditStartTime = DateTime.Now;
            EditEndTime = DateTime.Now;
            SelectedOrder = null;
            SelectedLine = null;
            SelectedTechnologist = null;
            UpdateButtons();
        }
    }
}