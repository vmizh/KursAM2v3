using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core.EntityViewModel.Invoices;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.WindowsManager;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using LayoutManager;

namespace Core.ViewModel.Base
{
    [POCOViewModel]
    public abstract class RSWindowViewModelBase : RSViewModelBase
    {
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        public readonly WindowManager WinManager = new WindowManager();
        private bool myDialogResult;
        private Window myForm;

        // ReSharper disable once MemberInitializerValueIgnored
        private bool myIsCanRefresh = true;

        private bool myIsLoading = true;

        // ReSharper disable once InconsistentNaming
        protected string mySearchText;
        private string myWindowName;

        public RSWindowViewModelBase()
        {
            myWindowName = "Без имени";
            myIsCanRefresh = true;
        }

        public RSWindowViewModelBase(Window form)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Form = form;
            myIsCanRefresh = true;
        }

        public bool IsLoading
        {
            get => myIsLoading;
            set
            {
                if (myIsLoading == value) return;
                myIsLoading = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)] public global::Helper.LayoutManager LayoutManager { get; set; }

        private ILayoutSerializationService LayoutSerializationService
            => GetService<ILayoutSerializationService>(ServiceSearchMode.LocalOnly);

        public virtual string LayoutName { set; get; }
        [Display(AutoGenerateField = false)]
        public StandartErrorManager ErrorManager { set; get; }
        public ObservableCollection<MenuButtonInfo> RightMenuBar { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<MenuButtonInfo> LeftMenuBar { set; get; }

        //public ReportManager ReportManager { set; get; }
        [Display(AutoGenerateField = false)]
        public virtual Window Form
        {
            get => myForm;
            set
            {
                if (Equals(myForm, value)) return;
                myForm = value;
                RaisePropertyChanged();
            }
        }

        public virtual string WindowName
        {
            get => myWindowName;
            set
            {
                if (myWindowName == value) return;
                myWindowName = value;
                RaisePropertyChanged();
            }
        }

        public virtual string FooterText
        {
            set
            {
                if (myFooterText == value) return;
                myFooterText = value;
                RaisePropertyChanged();
            }
            get => myFooterText;
        }

        public bool IsCanSave { get; set; }

        [Command]
        public void OnWindowClosing()
        {
            LayoutManager?.Save();
        }

        [Command]
        public void OnWindowLoaded()
        {
            LayoutManager = new global::Helper.LayoutManager(Form, LayoutSerializationService,
                LayoutName, null);
            LayoutManager.Load();
        }

        public void RefreshData()
        {
            //throw new System.NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public bool Check()
        {
            throw new NotImplementedException();
        }

        public InvoiceClient NewDocument()
        {
            throw new NotImplementedException();
        }

        public InvoiceClient CopyDocument()
        {
            throw new NotImplementedException();
        }

        public InvoiceClient CopyRequisite()
        {
            throw new NotImplementedException();
        }

        public void UnDeleteRows()
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateDocumentOpen()
        {

        }

        public virtual void UpdatePropertyChangies()
        {
            RaisePropertyChanged("Document");
            RaisePropertyChanged("Rows");
        }

        #region Command

        [Display(AutoGenerateField = false)]
        public virtual ICommand CommandGridControlExport
        {
            get { return new Command(GridControlExport, param => true); }
        }

        public string DatabaseName => GlobalOptions.DataBaseName;

        [Display(AutoGenerateField = false)] public Brush DatabaseColor => GlobalOptions.DatabaseColor;

        private void GridControlExport(object obj)
        {
            if (obj is GridControl grid)
            {
                var view = grid.View;
                view.ShowPrintPreview(Application.Current.MainWindow);
                return;
            }

            var tgrid = obj as TreeListControl;
            var view1 = tgrid?.View;
            view1?.ShowPrintPreview(Application.Current.MainWindow);
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand CloseWindowCommand
        {
            // ReSharper disable once UnusedParameter.Local
            get { return new Command(CloseWindow, param => true); }
        }

        public virtual bool IsSearchTextNull => string.IsNullOrEmpty(SearchText);

        public virtual void CloseWindow(object form)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            if (Form != null)
            {
                Form.Close();
                return;
            }

            var frm = form as Window;
            frm?.Close();
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand RefreshDataCommand
        {
            get { return new Command(RefreshData, param => IsCanRefresh); }
        }

        public virtual bool IsCanRefresh
        {
            get => myIsCanRefresh;
            set
            {
                if (myIsCanRefresh == value) return;
                myIsCanRefresh = value;
                RaisePropertyChanged();
            }
        }

        public virtual void RefreshData(object obj)
        {
            MainReferences.Refresh();
        }

        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        [Display(AutoGenerateField = false)]
        public AsyncCommand AsyncRefreshDataCommand => new AsyncCommand(AsyncTaskRefresh, IsCanRefresh);


        public async Task AsyncTaskRefresh()
        {
            await Task.Factory.StartNew(AsyncRefresh());
        }

        public virtual Action AsyncRefresh()
        {
            return null;
        }

        // ReSharper disable once InconsistentNaming
        public virtual void ObservableCollection<MenuButtonInfo>(object data)
        {
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand SaveDataCommand
        {
            get { return new Command(SaveData, param => IsCanSaveData); }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand DoneCommand
        {
            get { return new Command(Done, param => IsCanDone); }
        }

        public virtual bool IsCanDone { get; set; } = false;

        public virtual void Done(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand DocumentOpenCommand
        {
            get { return new Command(DocumentOpen, param => IsDocumentOpenAllow); }
        }

        private ColumnBase myCurrentColumn;

        [Display(AutoGenerateField = false)]
        public ColumnBase CurrentColumn
        {
            get => myCurrentColumn;
            set
            {
                if (Equals(myCurrentColumn, value)) return;
                myCurrentColumn = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand VisualControlExportCommand
        {
            get { return new Command(VisualControlExport, param => true); }
        }

        public virtual bool IsGetColumnSumma()
        {
            if (string.IsNullOrWhiteSpace(CurrentColumn?.TotalSummaryText) ||
                string.IsNullOrEmpty(CurrentColumn.TotalSummaryText))
                return false;
            return true;
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand GetColumnSummaCommand
        {
            get { return new Command(GetColumnSumma, param => IsGetColumnSumma()); }
        }

        private void GetColumnSumma(object obj)
        {
            var tbl = obj as TableView;
            var col = tbl?.Grid.CurrentColumn;
            if (col == null) return;
            Clipboard.SetText(col.TotalSummaryText);
        }

        private void VisualControlExport(object obj)
        {
            var ctrl = obj as DataViewBase;
            ctrl?.ShowPrintPreview(Application.Current.MainWindow);
        }

        public virtual bool IsDocumentOpenAllow { get; set; }

        public virtual void DocumentOpen(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        private string mySaveInfo;

        public virtual string SaveInfo
        {
            get => mySaveInfo;
            set
            {
                if (mySaveInfo == value) return;
                mySaveInfo = value;
                RaisePropertyChanged();
            }
        }

        private bool myIsCanDocNew = true;

        public virtual bool IsCanDocNew
        {
            get => myIsCanDocNew;
            set
            {
                if (myIsCanDocNew == value) return;
                myIsCanDocNew = value;
                RaisePropertyChanged();
            }
        }

        private bool myIsCanSaveData;

        public virtual bool IsCanSaveData
        {
            get => myIsCanSaveData;
            set
            {
                if (myIsCanSaveData == value) return;
                myIsCanSaveData = value;
                RaisePropertyChanged();
            }
        }

        public virtual void SaveData(object data)
        {
        }

        [Display(AutoGenerateField = false)]
        public ICommand DocNewEmptyCommand
        {
            get { return new Command(DocNewEmpty, param => IsDocNewEmptyAllow); }
        }

        public virtual void DocNewEmpty(object form)
        {
        }

        // ReSharper disable once InconsistentNaming
        private bool myIsDocNewEmptyAllow = true;

        public virtual bool IsDocNewEmptyAllow
        {
            get => myIsDocNewEmptyAllow;
            set
            {
                if (myIsDocNewEmptyAllow == value) return;
                myIsDocNewEmptyAllow = value;
                RaisePropertyChanged();
            }
        }

        private bool myIsDocNewCopyAllow = true;

        public virtual bool IsDocNewCopyAllow
        {
            get => myIsDocNewCopyAllow;
            set
            {
                if (myIsDocNewCopyAllow == value) return;
                myIsDocNewCopyAllow = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand DocNewCopyCommand
        {
            get { return new Command(DocNewCopy, param => IsDocNewCopyAllow); }
        }

        public virtual void DocNewCopy(object form)
        {
        }

        private bool myIsDocNewCopyRequisiteAllow = true;

        public virtual bool IsDocNewCopyRequisiteAllow
        {
            get => myIsDocNewCopyRequisiteAllow;
            set
            {
                if (myIsDocNewCopyRequisiteAllow == value) return;
                myIsDocNewCopyRequisiteAllow = value;
                RaisePropertyChanged();
            }
        }

        private bool myIsDocDeleteAllow = true;

        public virtual bool IsDocDeleteAllow
        {
            get => myIsDocDeleteAllow;
            set
            {
                if (myIsDocDeleteAllow == value) return;
                myIsDocDeleteAllow = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand DoсDeleteCommand
        {
            get { return new Command(DocDelete, param => IsDocDeleteAllow); }
        }

        public virtual void DocDelete(object form)
        {
        }

        private bool myIsRedoAllow = true;

        public virtual bool IsRedoAllow
        {
            get => myIsRedoAllow;
            set
            {
                if (myIsRedoAllow == value) return;
                myIsRedoAllow = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand RedoCommand
        {
            get { return new Command(Redo, param => IsRedoAllow); }
        }

        public virtual void Redo(object form)
        {
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand DocNewCopyRequisiteCommand
        {
            get { return new Command(DocNewCopyRequisite, param => IsDocNewCopyRequisiteAllow); }
        }

        public virtual void DocNewCopyRequisite(object form)
        {
        }

        private bool myIsPrintAllow = true;

        public virtual bool IsPrintAllow
        {
            get => myIsPrintAllow;
            set
            {
                if (myIsPrintAllow == value) return;
                myIsPrintAllow = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand PrintCommand
        {
            get { return new Command(Print, param => IsPrintAllow); }
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand OkCommand
        {
            get { return new Command(Ok, param => IsOkAllow()); }
        }

        public virtual bool IsOkAllow()
        {
            return true;
        }

        public bool DialogResult
        {
            get => myDialogResult;
            set
            {
                if (myDialogResult == value) return;
                myDialogResult = value;
                RaisePropertyChanged();
            }
        }

        public virtual void Ok(object obj)
        {
            DialogResult = true;
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand CancelCommand
        {
            get { return new Command(Cancel, param => true); }
        }

        public virtual void Cancel(object obj)
        {
            DialogResult = false;
        }

        public virtual void Print(object form)
        {
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand ResetLayoutCommand
        {
            get { return new Command(ResetLayout, param => true); }
        }

        public virtual void ResetLayout(object form)
        {
            if (Form is ILayout layman)
                layman.LayoutManager?.ResetLayout();
            else
                LayoutManager.ResetLayout();
        }

        [Display(AutoGenerateField = false)]
        public virtual ICommand ShowHistoryCommand
        {
            get { return new Command(ShowHistory, param => IsAllowHistoryShow); }
        }

        public virtual void ShowHistory(object data)
        {
        }

        public virtual bool IsAllowHistoryShow { set; get; } = true;

        public virtual string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(IsSearchTextNull));
            }
        }

        private bool myIsCanSearch;
        private string myFooterText;

        public virtual bool IsCanSearch
        {
            get => myIsCanSearch;
            set
            {
                if (myIsCanSearch == value) return;
                myIsCanSearch = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand SearchCommand
        {
            get { return new Command(Search, _ => IsCanSearch); }
        }

        public virtual void Search(object obj)
        {
        }

        [Display(AutoGenerateField = false)]
        public ICommand SearchClearCommand
        {
            get { return new Command(SearchClear, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public virtual void SearchClear(object obj)
        {
            SearchText = null;
        }

        [Display(AutoGenerateField = false)]
        public ICommand SearchKeyDownCommand
        {
            get { return new Command(SearchOnEnter, _ => true); }
        }

        public virtual void SearchOnEnter(object obj)
        {
            Search(SearchText);
        }

        #endregion Command
    }
}