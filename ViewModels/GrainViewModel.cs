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
    public class GrainViewModel : ObservableObject
    {
        private readonly IRepository<Grain> _repo;
        private readonly IRepository<Variety> _varietyRepo;
        private readonly IDialogService _dialogService;

        public GrainViewModel(IRepository<Grain> repo, IRepository<Variety> varietyRepo, IDialogService dialogService)
        {
            _repo = repo;
            _varietyRepo = varietyRepo;
            _dialogService = dialogService;

            Grains = new ObservableCollection<Grain>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<Grain> Grains { get; }

        private Grain? _selectedGrain;
        public Grain? SelectedGrain
        {
            get => _selectedGrain;
            set
            {
                Set(ref _selectedGrain, value);
                if (value != null)
                {
                    EditNameRu = value.Grain_NameRu ?? string.Empty;
                    EditNameLatin = value.Grain_NameLatin ?? string.Empty;
                }
                UpdateButtons();
            }
        }

        private string _editNameRu = string.Empty;
        public string EditNameRu
        {
            get => _editNameRu;
            set { Set(ref _editNameRu, value); UpdateButtons(); }
        }

        private string _editNameLatin = string.Empty;
        public string EditNameLatin
        {
            get => _editNameLatin;
            set { Set(ref _editNameLatin, value); UpdateButtons(); }
        }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() =>
            SelectedGrain != null &&
            SelectedGrain.Grain_ID > 0 &&
            !string.IsNullOrWhiteSpace(EditNameRu);

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(EditNameRu) &&
            (SelectedGrain == null || SelectedGrain.Grain_ID == 0);

        private bool CanDelete() =>
            SelectedGrain != null &&
            SelectedGrain.Grain_ID > 0;

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
                var grains = await _repo.Query().AsNoTracking().ToListAsync();
                Grains.Clear();
                foreach (var g in grains) Grains.Add(g);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [GrainVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newGrain = new Grain
            {
                Grain_NameRu = EditNameRu,
                Grain_NameLatin = EditNameLatin
            };

            await _repo.AddAsync(newGrain);
            await LoadData();

            EditNameRu = string.Empty;
            EditNameLatin = string.Empty;
            SelectedGrain = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedGrain == null || !CanSave()) return;

            SelectedGrain.Grain_NameRu = EditNameRu;
            SelectedGrain.Grain_NameLatin = EditNameLatin;

            _repo.Update(SelectedGrain);
            await _repo.SaveChangesAsync();

            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedGrain == null || !CanDelete()) return;

            var hasChildren = await _varietyRepo.Query()
                .AnyAsync(v => v.Variety_Grain == SelectedGrain.Grain_ID);

            if (hasChildren)
            {
                _dialogService.ShowError(
                    "Это зерно используется в сортах.\n" +
                    "Удалить нельзя. Сначала удалите связанные сорта.",
                    "Ошибка удаления");
                return;
            }

            _repo.Remove(SelectedGrain);
            await _repo.SaveChangesAsync();

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedGrain = null;
            EditNameRu = string.Empty;
            EditNameLatin = string.Empty;
            UpdateButtons();
        }
    }
}