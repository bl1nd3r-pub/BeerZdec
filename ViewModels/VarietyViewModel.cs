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
    public class VarietyViewModel : ObservableObject
    {
        private readonly IRepository<Variety> _repo;
        private readonly IRepository<Grain> _grainRepo;
        private readonly IDialogService _dialogService;

        public VarietyViewModel(IRepository<Variety> repo, IRepository<Grain> grainRepo, IDialogService dialogService)
        {
            _repo = repo;
            _grainRepo = grainRepo;
            _dialogService = dialogService;

            Varieties = new ObservableCollection<Variety>();
            Grains = new ObservableCollection<Grain>();

            LoadCommand = new RelayCommandAsync(LoadData);
            AddCommand = new RelayCommandAsync(AddNew, CanAdd);
            SaveCommand = new RelayCommandAsync(SaveData, CanSave);
            DeleteCommand = new RelayCommandAsync(DeleteData, CanDelete);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<Variety> Varieties { get; }
        public ObservableCollection<Grain> Grains { get; }

        private Variety? _selectedVariety;
        public Variety? SelectedVariety
        {
            get => _selectedVariety;
            set
            {
                Set(ref _selectedVariety, value);
                if (value != null)
                {
                    EditNameRu = value.Variety_NameRu ?? string.Empty;
                    EditNameLatin = value.Variety_NameLatin ?? string.Empty;
                    EditMaturityGroup = value.Variety_MaturityGroup ?? string.Empty;
                    EditMaltingPurpose = value.Variety_MaltingPurpose ?? string.Empty;
                    SelectedGrain = Grains.FirstOrDefault(g => g.Grain_ID == value.Variety_Grain);
                }
                UpdateButtons();
            }
        }

        private string _editNameRu = string.Empty;
        public string EditNameRu { get => _editNameRu; set { Set(ref _editNameRu, value); UpdateButtons(); } }

        private string _editNameLatin = string.Empty;
        public string EditNameLatin { get => _editNameLatin; set { Set(ref _editNameLatin, value); UpdateButtons(); } }

        private string _editMaturityGroup = string.Empty;
        public string EditMaturityGroup { get => _editMaturityGroup; set { Set(ref _editMaturityGroup, value); UpdateButtons(); } }

        private string _editMaltingPurpose = string.Empty;
        public string EditMaltingPurpose { get => _editMaltingPurpose; set { Set(ref _editMaltingPurpose, value); UpdateButtons(); } }

        private Grain? _selectedGrain;
        public Grain? SelectedGrain { get => _selectedGrain; set { Set(ref _selectedGrain, value); UpdateButtons(); } }

        public RelayCommandAsync LoadCommand { get; }
        public RelayCommandAsync AddCommand { get; }
        public RelayCommandAsync SaveCommand { get; }
        public RelayCommandAsync DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        private bool CanSave() => SelectedVariety != null && SelectedVariety.Variety_ID > 0 && !string.IsNullOrWhiteSpace(EditNameRu) && SelectedGrain != null;
        private bool CanAdd() => !string.IsNullOrWhiteSpace(EditNameRu) && SelectedGrain != null;
        private bool CanDelete() => SelectedVariety != null && SelectedVariety.Variety_ID > 0;

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
                // Справочник зерна (для ComboBox)
                var grains = await _grainRepo.Query().AsNoTracking().ToListAsync();
                Grains.Clear();
                foreach (var g in grains) Grains.Add(g);

                // Сорта
                var varieties = await _repo.Query()
                    .Include(v => v.Variety_GrainNavigation)
                    .AsNoTracking().ToListAsync();
                Varieties.Clear();
                foreach (var v in varieties) Varieties.Add(v);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("A second operation was started"))
            {
                Debug.WriteLine("!!! [VarietyVM] Пропущена гонка потоков. Повторная загрузка...");
                await Task.Delay(100);
                await LoadData();
            }
        }

        private async Task AddNew()
        {
            if (!CanAdd()) return;

            var newVariety = new Variety
            {
                Variety_NameRu = EditNameRu,
                Variety_NameLatin = EditNameLatin,
                Variety_Grain = SelectedGrain!.Grain_ID,
                Variety_MaturityGroup = EditMaturityGroup,
                Variety_MaltingPurpose = EditMaltingPurpose
            };

            await _repo.AddAsync(newVariety);
            await LoadData();

            EditNameRu = string.Empty;
            EditNameLatin = string.Empty;
            EditMaturityGroup = string.Empty;
            EditMaltingPurpose = string.Empty;
            SelectedGrain = null;
            SelectedVariety = null;
            UpdateButtons();
        }

        private async Task SaveData()
        {
            if (SelectedVariety == null || !CanSave()) return;

            SelectedVariety.Variety_NameRu = EditNameRu;
            SelectedVariety.Variety_NameLatin = EditNameLatin;
            SelectedVariety.Variety_Grain = SelectedGrain!.Grain_ID;
            SelectedVariety.Variety_MaturityGroup = EditMaturityGroup;
            SelectedVariety.Variety_MaltingPurpose = EditMaltingPurpose;

            // Явно помечаем как изменённый и сохраняем
            _repo.Update(SelectedVariety);
            await _repo.SaveChangesAsync();

            await LoadData();
            CancelEdit();
        }

        private async Task DeleteData()
        {
            if (SelectedVariety == null || !CanDelete()) return;

            _repo.Remove(SelectedVariety);
            await _repo.SaveChangesAsync();

            await LoadData();
            CancelEdit();
        }

        private void CancelEdit()
        {
            SelectedVariety = null;
            EditNameRu = string.Empty;
            EditNameLatin = string.Empty;
            EditMaturityGroup = string.Empty;
            EditMaltingPurpose = string.Empty;
            SelectedGrain = null;
            UpdateButtons();
        }
    }
}