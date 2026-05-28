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
    public class SuppliersViewModel : ObservableObject
    {
        private readonly IRepository<Supplier> _repo;
        private readonly IDialogService _dialogService;

        public SuppliersViewModel(
            IRepository<Supplier> repo,
            IDialogService dialogService)
        {
            _repo = repo;
            _dialogService = dialogService;

            Suppliers = new ObservableCollection<Supplier>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<Supplier> Suppliers { get; }

        private Supplier? _selectedSupplier;
        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                Set(ref _selectedSupplier, value);
                if (value != null)
                {
                    EditName = value.Supplier_Name ?? string.Empty;
                    EditInn = value.Supplier_INN ?? 0;
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

        private int _editInn;
        public int EditInn
        {
            get => _editInn;
            set { Set(ref _editInn, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedSupplier != null &&
            SelectedSupplier.Supplier_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditName);

        private bool CanDelete() =>
            SelectedSupplier != null &&
            SelectedSupplier.Supplier_ID > 0;

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
                var suppliers = await _repo.Query().AsNoTracking().ToListAsync();
                Suppliers.Clear();
                foreach (var s in suppliers) Suppliers.Add(s);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [SuppliersVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newSupplier = new Supplier
            {
                Supplier_Name = EditName,
                Supplier_INN = EditInn > 0 ? EditInn : null
            };

            await _repo.AddAsync(newSupplier);
            await LoadData();
            CancelEdit();
        }

        private async Task SaveData()
        {
            if (SelectedSupplier == null || !CanSave()) return;

            SelectedSupplier.Supplier_Name = EditName;
            SelectedSupplier.Supplier_INN = EditInn > 0 ? EditInn : null;

            await _repo.UpdateAsync(SelectedSupplier);
            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedSupplier == null || !CanDelete()) return;

            var success = await _repo.RemoveAsync(SelectedSupplier);

            if (!success)
            {
                _dialogService.ShowError(
                    "Этот поставщик используется в журнале поставок.\n" +
                    "Удалить нельзя. Сначала удалите связанные записи.",
                    "Ошибка удаления");
                return;
            }

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedSupplier = null;
            EditName = string.Empty;
            EditInn = 0;
            UpdateButtons();
        }
    }
}