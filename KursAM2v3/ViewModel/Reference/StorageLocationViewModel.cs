using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using KursAM2.View.Base;
using KursDomain;
using KursDomain.Base;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.StorageLocationsRepositury;
using KursDomain.ViewModel.Base2;
using KursDomain.Wrapper;
using Prism.Commands;

namespace KursAM2.ViewModel.Reference
{
    public class StorageLocationViewModel : FormViewModelBase<StorageLocationsWrapper,Guid>, IFormMenu
    {
        #region Constructors

        public StorageLocationViewModel(IStorageLocationsRepositiry storageLocationsRepositiry)
        {
            LayoutName = "StorageLocation";
            Title = "Справочник мест хранения";
            Rows = new ObservableCollection<StorageLocationsWrapper>();
            _StorageLocationsRepositiry = storageLocationsRepositiry;

            AddCommand = new DelegateCommand(OnAdd, CanAdd);
            AddCopyCommand = new DelegateCommand(OnAddCopy, CanAddCopy);
            DeleteCommand = new DelegateCommand(OnDelete, CanDelete);

            FormControl = new TableAddDeleteUserControl(OnColumnGenerate);
            linkCommandToControl(FormControl as TableAddDeleteUserControl);
            
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        }

        #endregion


        #region Methods

        private void OnColumnGenerate(object sender, AutoGeneratingColumnEventArgs e)
        {
            switch (e.Column.FieldName)
            {
                case nameof(StorageLocationsWrapper.Employee):
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        ItemsSource = GlobalOptions.ReferencesCache.GetEmployees().ToList(),
                        DisplayMember = "Name",
                        AutoComplete = true
                    };
                    break;
                case nameof(StorageLocationsWrapper.Warehouse):
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        ItemsSource = GlobalOptions.ReferencesCache.GetWarehousesAll().ToList(),
                        DisplayMember = "Name",
                        AutoComplete = true
                    };
                    break;
            }
        }

        private void Wrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!HasChanges) HasChanges = _StorageLocationsRepositiry.HasChanges();
            if (e.PropertyName == nameof(StorageLocationsWrapper.HasErrors))
                ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        private void linkCommandToControl(TableAddDeleteUserControl form)
        {
            if (form == null) return;
            form.ButtonAdd.Command = AddCommand;
            form.ButtonDelete.Command = DeleteCommand;
            form.menuAdd.Command = AddCommand;
            form.menuAddCopy.Command = AddCopyCommand;
            form.menuDelete.Command = DeleteCommand;
        }

        #endregion

        #region Fields

        private readonly IStorageLocationsRepositiry _StorageLocationsRepositiry;

        private StorageLocationsWrapper _CurrentItem;

        #endregion


        #region Properties

        public List<Region> Regions { get; private set; } =
            new List<Region>(GlobalOptions.ReferencesCache.GetRegionsAll().Cast<Region>());

        public ObservableCollection<StorageLocationsWrapper> Rows { get; }
        public ObservableCollection<MenuButtonInfo> RightMenuBar { get; set; }
        public ObservableCollection<MenuButtonInfo> LeftMenuBar { get; set; }

        public StorageLocationsWrapper CurrentItem
        {
            get => _CurrentItem;
            set
            {
                if (_CurrentItem == value) return;
                _CurrentItem = value;
                OnPropertyChanged();
                ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Command

        public readonly ICommand AddCommand;
        public readonly ICommand AddCopyCommand;
        public readonly ICommand DeleteCommand;

        private bool CanAddCopy()
        {
            return CurrentItem != null;
        }

        private void OnAddCopy()
        {
            var newItem = new StorageLocations
            {
                Id = Guid.NewGuid(),
                Name = "Новое"
            };
            _StorageLocationsRepositiry.Add(newItem);
            var newStorage = new StorageLocationsWrapper(newItem)
            {
                Warehouse = CurrentItem.Warehouse,
                Region = CurrentItem.Region,
                Employee = CurrentItem.Employee,
                Note = CurrentItem.Note
            };
            newStorage.PropertyChanged += Wrapper_PropertyChanged;
            Rows.Add(newStorage);
            this.GetRequiredService<IStartEditingService>().StartEditing(newStorage, "Name");
            HasChanges = _StorageLocationsRepositiry.HasChanges();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }
        

        private bool CanAdd()
        {
            return true;
        }

        private void OnAdd()
        {
            var newItem = new StorageLocations
            {
                Id = Guid.NewGuid(),
                Name = "Новое"
            };
            _StorageLocationsRepositiry.Add(newItem);
            var newStorage = new StorageLocationsWrapper(newItem);
            newStorage.PropertyChanged += Wrapper_PropertyChanged;
            Rows.Add(newStorage);
            this.GetRequiredService<IStartEditingService>().StartEditing(newStorage, "Name");
            HasChanges = _StorageLocationsRepositiry.HasChanges();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        public override Task InitializeAsync(Guid id, DocumentNewState newState = DocumentNewState.None)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(Guid id, DocumentNewState newState = DocumentNewState.None)
        {
            throw new NotImplementedException();
        }

        protected override bool CanDelete()
        {
            return CurrentItem != null;
        }
        protected override void OnDelete()
        {
            _StorageLocationsRepositiry.Remove(CurrentItem.Model);
            Rows.Remove(CurrentItem);
            HasChanges = _StorageLocationsRepositiry.HasChanges();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        protected override bool CanSaveData()
        {
            return _StorageLocationsRepositiry.HasChanges();
        }

        protected override async Task OnSaveDataAsync()
        {
            await _StorageLocationsRepositiry.SaveAsync();
            HasChanges = false;
            ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddCopyCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        public override async Task OnRefreshDataAsync()
        {
            if (HasChanges)
            {
                var wm = new WindowManager();
                var dlgRslt = wm.ShowKursDialog("В справочник внесены изменения, сохранить?", "Запрос",
                    Brushes.Blue, WindowManager.YesNoCancel);
                switch (dlgRslt)
                {
                    case WindowManager.KursDialogResult.Cancel:
                        return;
                    case WindowManager.KursDialogResult.No:
                        break;
                    case WindowManager.KursDialogResult.Yes:
                        await OnSaveDataAsync();
                        break;
                }
            }

            var items = await _StorageLocationsRepositiry.GetAllAsync();
            Form.Dispatcher.BeginInvoke(new Action(() =>
            {
                Rows.Clear();
                foreach (var item in items)
                {
                    var newItem = new StorageLocationsWrapper(item);
                    Rows.Add(newItem);
                    newItem.PropertyChanged += Wrapper_PropertyChanged;
                }
            }));
            HasChanges = false;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        #endregion
    }
}
