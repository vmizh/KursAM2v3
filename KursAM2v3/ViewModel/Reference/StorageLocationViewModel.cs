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
    [SuppressMessage("ReSharper", "AsyncVoidLambda")]
    public class StorageLocationViewModel : FormViewModelBase, IFormMenu, IFormOperation
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

            RefreshDataCommand = new DelegateCommand(async () => await OnRefreshData(), CanRefreshData);
            SaveDataCommand = new DelegateCommand(async () => await OnSaveData(), CanSaveData);
            CloseWindowCommand = new DelegateCommand(OnCloseWindow, CanCloseWindow);

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

        private bool _hasChanges;
        private readonly IStorageLocationsRepositiry _StorageLocationsRepositiry;

        private StorageLocationsWrapper _CurrentItem;

        #endregion


        #region Properties

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (_hasChanges == value) return;
                _hasChanges = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
            }
        }

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

        public readonly ICommand AddCopyCommand;

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


        public readonly ICommand AddCommand;

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

        public readonly ICommand DeleteCommand;


        private bool CanDelete()
        {
            return CurrentItem != null;
        }

        private void OnDelete()
        {
            _StorageLocationsRepositiry.Remove(CurrentItem.Model);
            Rows.Remove(CurrentItem);
            HasChanges = _StorageLocationsRepositiry.HasChanges();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        public ICommand CloseWindowCommand { get; }

        private bool CanCloseWindow()
        {
            return true;
        }

        private async void OnCloseWindow()
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
                        await OnSaveData();
                        break;
                }
            }

            Close();
        }

        public ICommand DocumentOpenCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand DocNewCommand { get; }
        public ICommand DocNewEmptyCommand { get; }
        public ICommand DocNewCopyRequisiteCommand { get; }
        public ICommand DocNewCopyCommand { get; }
        public ICommand RefreshDataCommand { get; }

        private bool CanRefreshData()
        {
            return true;
        }

        internal async Task OnRefreshData()
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
                        await OnSaveData();
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
            _StorageLocationsRepositiry.Rollback();
            _hasChanges = false;
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        public ICommand SaveDataCommand { get; }

        private bool CanSaveData()
        {
            return _StorageLocationsRepositiry.HasChanges();
        }

        private async Task OnSaveData()
        {
            await _StorageLocationsRepositiry.SaveAsync();
            _hasChanges = false;
            ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddCopyCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }

        public ICommand ResetLayoutCommand { get; }
        public ICommand ShowHistoryCommand { get; }
        public ICommand DoсDeleteCommand { get; }
        public ICommand RedoCommand { get; }

        #endregion
    }
}
