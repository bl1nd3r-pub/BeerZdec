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
    public class MaltEquipmentViewModel : ObservableObject
    {
        private readonly IRepository<MaltEquipment> _repo;
        private readonly IRepository<MaltingLine> _lineRepo;
        private readonly IRepository<MaltEquipType> _typeRepo;
        private readonly IDialogService _dialogService;

        public MaltEquipmentViewModel(
            IRepository<MaltEquipment> repo,
            IRepository<MaltingLine> lineRepo,
            IRepository<MaltEquipType> typeRepo,
            IDialogService dialogService)
        {
            _repo = repo;
            _lineRepo = lineRepo;
            _typeRepo = typeRepo;
            _dialogService = dialogService;

            Equipment = new ObservableCollection<MaltEquipment>();
            Lines = new ObservableCollection<MaltingLine>();
            Types = new ObservableCollection<MaltEquipType>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<MaltEquipment> Equipment { get; }
        public ObservableCollection<MaltingLine> Lines { get; }
        public ObservableCollection<MaltEquipType> Types { get; }

        private MaltEquipment? _selectedEquip;
        public MaltEquipment? SelectedEquip
        {
            get => _selectedEquip;
            set
            {
                Set(ref _selectedEquip, value);
                if (value != null)
                {
                    EditManufacturer = value.MaltEquipment_Manufacturer ?? string.Empty;
                    EditInstallDate = value.MaltEquipment_InstallDate.HasValue
                        ? new DateTime(value.MaltEquipment_InstallDate.Value.Year,
                                      value.MaltEquipment_InstallDate.Value.Month,
                                      value.MaltEquipment_InstallDate.Value.Day)
                        : (DateTime?)DateTime.Today;
                    EditIsActive = value.MaltEquipment_IsActive ?? false;

                    SelectedLine = Lines.FirstOrDefault(l => l.MaltingLine_ID == value.MaltEquipment_MaltingLine);
                    SelectedType = Types.FirstOrDefault(t => t.MaltEquipType_ID == value.MaltEquipment_Type);
                }
                UpdateButtons();
            }
        }

        private string _editManufacturer = string.Empty;
        public string EditManufacturer
        {
            get => _editManufacturer;
            set { Set(ref _editManufacturer, value); UpdateButtons(); }
        }

        private DateTime? _editInstallDate;
        public DateTime? EditInstallDate
        {
            get => _editInstallDate;
            set { Set(ref _editInstallDate, value); UpdateButtons(); }
        }

        private bool _editIsActive;
        public bool EditIsActive
        {
            get => _editIsActive;
            set { Set(ref _editIsActive, value); UpdateButtons(); }
        }

        private MaltingLine? _selectedLine;
        public MaltingLine? SelectedLine
        {
            get => _selectedLine;
            set { Set(ref _selectedLine, value); UpdateButtons(); }
        }

        private MaltEquipType? _selectedType;
        public MaltEquipType? SelectedType
        {
            get => _selectedType;
            set { Set(ref _selectedType, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedEquip != null &&
            SelectedEquip.MaltEquipment_ID > 0 &&
            SelectedLine != null &&
            SelectedType != null;

        private bool CanAdd() =>
            SelectedLine != null &&
            SelectedType != null;

        private bool CanDelete() =>
            SelectedEquip != null &&
            SelectedEquip.MaltEquipment_ID > 0;

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
                var lines = await _lineRepo.Query().AsNoTracking().ToListAsync();
                Lines.Clear();
                foreach (var l in lines) Lines.Add(l);

                var types = await _typeRepo.Query().AsNoTracking().ToListAsync();
                Types.Clear();
                foreach (var t in types) Types.Add(t);

                // Загружаем оборудование
                var equip = await _repo.Query()
                    .Include(e => e.MaltEquipment_MaltingLineNavigation)
                    .Include(e => e.MaltEquipment_TypeNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                Equipment.Clear();
                foreach (var e in equip) Equipment.Add(e);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [MaltEquipmentVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newEquip = new MaltEquipment
            {
                MaltEquipment_MaltingLine = SelectedLine!.MaltingLine_ID,
                MaltEquipment_Type = SelectedType!.MaltEquipType_ID,
                MaltEquipment_Manufacturer = EditManufacturer,
                MaltEquipment_InstallDate = EditInstallDate.HasValue ? DateOnly.FromDateTime(EditInstallDate.Value) : null,
                MaltEquipment_IsActive = EditIsActive
            };

            await _repo.AddAsync(newEquip);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedEquip == null || !CanSave()) return;

            SelectedEquip.MaltEquipment_MaltingLine = SelectedLine!.MaltingLine_ID;
            SelectedEquip.MaltEquipment_Type = SelectedType!.MaltEquipType_ID;
            SelectedEquip.MaltEquipment_Manufacturer = EditManufacturer;
            SelectedEquip.MaltEquipment_InstallDate = EditInstallDate.HasValue ? DateOnly.FromDateTime(EditInstallDate.Value) : null;
            SelectedEquip.MaltEquipment_IsActive = EditIsActive;

            await _repo.UpdateAsync(SelectedEquip);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedEquip == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedEquip);

            if (!success)
            {
                _dialogService.ShowError(
                    "Это оборудование используется в процессах солодоращения.\n" +
                    "Удалить нельзя. Сначала удалите связанные процессы.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedEquip = null;
            EditManufacturer = string.Empty;
            EditInstallDate = DateTime.Today;
            EditIsActive = false;
            SelectedLine = null;
            SelectedType = null;
            UpdateButtons();
        }
    }
}