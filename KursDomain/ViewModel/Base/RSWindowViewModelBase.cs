using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using Helper;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.ViewModel.Base2;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;
using ILayout = LayoutManager.ILayout;

namespace Core.ViewModel.Base;

[POCOViewModel]
public abstract class RSWindowViewModelBase : RSViewModelBase, ISupportLogicalLayout, ISupportServices,
    IFormCommands,
    IDialogOperation
{
    private bool _versionUpdateStatus;
    protected bool IsDialog = false;
    protected bool IsDeleting = false;
    [Display(AutoGenerateField = false)] protected bool IsLayoutLoaded;

    // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
    //public readonly WindowManager WinManager = new WindowManager();
    private bool myDialogResult;
    public Window myForm;

    // ReSharper disable once MemberInitializerValueIgnored
    private bool myIsCanRefresh = true;

    private bool myIsLoading;
    private Visibility myIsMenuInfoVisibility;
    private string myMenuInfoString;

    // ReSharper disable once InconsistentNaming
    protected string mySearchText;

    private string myWindowName;

    //public IServiceContainer ServiceContainer => null;
    //public IServiceContainer ISupportServices.ServiceContainer { get { return ServiceContainer; } }
    private IServiceContainer serviceContainer;

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

    public RSWindowViewModelBase(ThemedWindow form)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Form = form;
        myIsCanRefresh = true;
    }

    [Display(AutoGenerateField = false)]
    public bool IsVersionUpdateStatus
    {
        get => _versionUpdateStatus;
        set
        {
            if (_versionUpdateStatus == value) return;
            _versionUpdateStatus = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    protected IDispatcherService DispatcherService => this.GetService<IDispatcherService>();

    [Display(AutoGenerateField = false)]
    protected ISplashScreenService SplashScreenService => this.GetService<ISplashScreenService>();

    [Display(AutoGenerateField = false)] protected IDialogService DialogService => this.GetService<IDialogService>();

    [Display(AutoGenerateField = false)]
    protected ILayoutSerializationService LayoutSerializationService => this.GetService<ILayoutSerializationService>();

    [Display(AutoGenerateField = false)]
    protected INotificationService KursNotyficationService => this.GetService<INotificationService>();

    [Display(AutoGenerateField = false)]
    protected INavigationService NavigationService => this.GetRequiredService<INavigationService>();


    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)] public RSWindowViewModelBase ParentFormViewModel { set; get; }

    [Display(AutoGenerateField = false)] public virtual int DocumentId { set; get; }

    [Display(AutoGenerateField = false)] public virtual string ToolTipForSave { set; get; } = "Сохранение";

    [Display(AutoGenerateField = false)] public global::Helper.LayoutManager LayoutManager { get; set; }
    public virtual string LayoutName { set; get; }

    // [Display(AutoGenerateField = false)] public StandartErrorManager ErrorManager { set; get; }

    [Display(AutoGenerateField = false)] public ObservableCollection<MenuButtonInfo> RightMenuBar { set; get; }

    [Display(AutoGenerateField = false)]
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


    [Display(AutoGenerateField = false)]
    public virtual string MenuInfoString
    {
        get => myMenuInfoString;
        set
        {
            if (myMenuInfoString == value) return;
            myMenuInfoString = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public virtual Visibility IsMenuInfoVisibility
    {
        get => myIsMenuInfoVisibility;
        set
        {
            if (myIsMenuInfoVisibility == value) return;
            myIsMenuInfoVisibility = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
    public ICommand UnboundColumnDataCommand
    {
        get { return new Command(UnboundColumnData, _ => true); }
    }


    [Display(AutoGenerateField = false)] public bool IsCanSave { get; set; }

    [Display(AutoGenerateField = false)]
    public ICommand OnWindowClosingCommand
    {
        get { return new Command(OnWindowClosing, _ => IsLayoutLoaded); }
    }

    [Display(AutoGenerateField = false)]
    public ICommand OnWindowLoadedCommand
    {
        get { return new Command(OnWindowLoaded, _ => !IsLayoutLoaded); }
    }

    [Display(AutoGenerateField = false)]
    public ICommand OnLayoutInitialCommand
    {
        get { return new Command(OnLayoutInitial, _ => IsLayoutLoaded); }
    }

    protected IServiceContainer ServiceContainer
    {
        get
        {
            if (serviceContainer == null)
                serviceContainer = new ServiceContainer(this);
            return serviceContainer;
        }
    }

    public bool CanSerialize => true;
    public IDocumentManagerService DocumentManagerService => null;
    public IEnumerable<object> LookupViewModels => null;
    IServiceContainer ISupportServices.ServiceContainer => ServiceContainer;

    protected virtual void UnboundColumnData(object obj)
    {
    }

    private bool IsSummaryExists(GridSummaryItemCollection tsums, string fname, SummaryItemType sumType)
    {
        foreach (var s in tsums)
            if (s.FieldName == fname && s.SummaryType == sumType)
                return true;

        return false;
    }

    public virtual void OnWindowClosing(object obj)
    {
        LayoutManager?.Save();
    }

    protected virtual void OnLayoutInitial(object obj)
    {
        if (Form != null)
            LayoutManager ??= new global::Helper.LayoutManager(Form, LayoutSerializationService,
                LayoutName, null, GlobalOptions.KursSystemDBContext);
        else
            LayoutManager = new global::Helper.LayoutManager(GlobalOptions.KursSystemDBContext,
                LayoutSerializationService,
                LayoutName, null);
    }

    /// <summary>
    ///     Устанавливае особенные свойства для таблиц, колонок или других
    ///     визуальных объектов после загрузки Layout
    /// </summary>
    public virtual void UpdateVisualObjects()
    {
        if (Form is null) return;
        var grids = Form.FindVisualChildren<GridControl>().ToList();
        var trees = Form.FindVisualChildren<TreeListControl>().ToList();
        if (grids.Count > 0)
            foreach (var grid in grids)
            {
                grid.FilterString = null;
                grid.TotalSummary.Clear();
                foreach (var col in grid.Columns)
                {
                    if (col.FieldName == "State")
                        col.Visible = false;
                    col.AutoFilterCondition = AutoFilterCondition.Contains;
                    col.ColumnFilterMode = ColumnFilterMode.DisplayText;
                    col.SortMode = ColumnSortMode.DisplayText;

                    if (col.FieldType == typeof(string))
                    {
                        col.EditSettings = new TextEditSettings
                        {
                            SelectAllOnMouseUp = true
                        };
                    }
                    else if (col.FieldType == typeof(decimal) ||
                             col.FieldType == typeof(decimal?)
                             || col.FieldType == typeof(float) || col.FieldType == typeof(float?)
                             || col.FieldType == typeof(double) || col.FieldType == typeof(double?))
                    {
                        col.ColumnFilterMode = ColumnFilterMode.Value;
                        col.SortMode = ColumnSortMode.Value;
                        if (col.EditSettings == null || col.EditSettings.GetType() != typeof(CalcEditSettings))
                            col.EditSettings = new CalcEditSettings();
                        if (col.Name.ToLower().Contains("quan"))
                        {
                            col.EditSettings.DisplayFormat =
                                GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                            if (!col.IsEnabled || col.ReadOnly)
                                ((CalcEditSettings)col.EditSettings).AllowDefaultButton = false;
                            grid.TotalSummary.Add(new GridSummaryItem
                            {
                                DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat(),
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                ShowInColumn = col.FieldName
                            });
                        }
                        else if (col.Name.ToLower().Contains("price") ||
                                 col.Name.ToLower().Contains("summa")
                                 || col.Name.ToLower().Contains("sum") || CheckCurrencyColumn(col.FieldName))
                        {
                            col.EditSettings.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                            grid.TotalSummary.Add(new GridSummaryItem
                            {
                                DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat(),
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                ShowInColumn = col.FieldName
                            });
                        }
                        else
                        {
                            col.EditSettings.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                            grid.TotalSummary.Add(new GridSummaryItem
                            {
                                DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat(),
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                ShowInColumn = col.FieldName
                            });
                        }
                    }
                    else if (col.FieldType == typeof(DateTime) ||
                             col.FieldType == typeof(DateTime?))

                    {
                        col.ColumnFilterMode = ColumnFilterMode.Value;
                        col.SortMode = ColumnSortMode.Value;
                    }

                    //if (col.FieldType == typeof(int) || col.FieldType == typeof(int?))
                    //{
                    //    if (IsSummaryExists(grid.TotalSummary, col.FieldName, SummaryItemType.Count)) continue;
                    //    grid.TotalSummary.Add(new GridSummaryItem
                    //    {
                    //        DisplayFormat = "n0",
                    //        FieldName = col.FieldName,
                    //        SummaryType = SummaryItemType.Count,
                    //        ShowInColumn = col.FieldName
                    //    });
                    //}
                }
            }

        if (trees.Count > 0)
            foreach (var t in trees)
            foreach (var col in t.Columns)
            {
                if (col.FieldName == "State")
                    col.Visible = false;
                col.AutoFilterCondition = AutoFilterCondition.Contains;
                col.ColumnFilterMode = ColumnFilterMode.Value;
                col.SortMode = ColumnSortMode.Value;
                col.Name = col.FieldName;
                if (col.EditSettings is CalcEditSettings ed)
                {
                    if (col.Name.ToLower().Contains("quan"))
                        ed.DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                    else if (col.Name.ToLower().Contains("price") ||
                             col.Name.ToLower().Contains("summa")
                             || col.Name.ToLower().Contains("sum"))
                        ed.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                    if (!col.IsEnabled || col.ReadOnly)
                        ed.AllowDefaultButton = false;
                }

                if (col.FieldType == typeof(string))
                    col.EditSettings = new TextEditSettings
                    {
                        SelectAllOnMouseUp = true
                    };
            }
    }

    private bool CheckCurrencyColumn(string name)
    {
        if (name == "RUB")
            name = "RUR";
        return GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<IName>().Select(_ => _.Name).Contains(name);
    }

    protected virtual void OnWindowLoaded(object obj)
    {
        if (IsLayoutLoaded || Form is null) 
            if(!IsDialog) return;
        var grids = Form is null ? new List<GridControl>() : Form.FindVisualChildren<GridControl>().ToList();
        var trees =Form is null ? new List<TreeListControl>() : Form.FindVisualChildren<TreeListControl>().ToList();
        try
        {
            foreach (var col in grids.SelectMany(grid => grid.Columns)) col.Name = col.FieldName;
            foreach (var col in trees.SelectMany(grid => grid.Columns)) col.Name = col.FieldName;
            if (LayoutManager == null) OnLayoutInitial(null);
            LayoutManager?.Load();
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }

        if (Form == null)
        {
            IsLayoutLoaded = true;
            if(!IsDialog)
                return;
        }

        UpdateVisualObjects();
        IsLayoutLoaded = true;
    }

    public virtual void RefreshData()
    {
        //throw new System.NotImplementedException();
    }

    public virtual void Save()
    {
        //throw new NotImplementedException();
    }

    public virtual bool CanSave()
    {
        throw new NotImplementedException();
    }

    public virtual void Load(object o)
    {
        throw new NotImplementedException();
    }

    public virtual bool CanLoad(object o)
    {
        throw new NotImplementedException();
    }

    public virtual bool Check()
    {
        throw new NotImplementedException();
    }

    public InvoiceClientViewModel NewDocument()
    {
        throw new NotImplementedException();
    }

    public InvoiceClientViewModel CopyDocument()
    {
        throw new NotImplementedException();
    }

    public InvoiceClientViewModel CopyRequisite()
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

    public virtual void RefreshData(object obj)
    {
        //throw new NotImplementedException();
    }

    #region Command

    [Display(AutoGenerateField = false)]
    public virtual ICommand CommandGridControlExport
    {
        get { return new Command(GridControlExport, _ => true); }
    }

    [Display(AutoGenerateField = false)] public string DatabaseName => GlobalOptions.DataBaseName;

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
        // ReSharper disable once Unused_eter.Local
        get { return new Command(CloseWindow, _ => true); }
    }

    [Display(AutoGenerateField = false)] public virtual bool IsSearchTextNull => string.IsNullOrEmpty(SearchText);

    public virtual void CloseWindow(object form)
    {
        if (IsCanSaveData && !IsDeleting)
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
        get { return new Command(RefreshData, _ => IsCanRefresh); }
    }

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
    public virtual ICommand SaveDataCommand
    {
        get { return new Command(SaveData, _ => IsCanSaveData); }
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand DoneCommand
    {
        get { return new Command(Done, _ => IsCanDone); }
    }

    [Display(AutoGenerateField = false)] public virtual bool IsCanDone { get; set; } = false;

    public virtual void Done(object obj)
    {
        //WindowManager.ShowFunctionNotReleased();
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand DocumentOpenCommand
    {
        get { return new Command(DocumentOpen, _ => IsDocumentOpenAllow); }
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
            //RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand VisualControlExportCommand
    {
        get { return new Command(VisualControlExport, _ => true); }
    }

    public virtual bool IsGetColumnSumma()
    {
        return !string.IsNullOrWhiteSpace(CurrentColumn?.TotalSummaryText) &&
               !string.IsNullOrEmpty(CurrentColumn.TotalSummaryText);
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand GetColumnSummaCommand
    {
        get { return new Command(GetColumnSumma, _ => IsGetColumnSumma()); }
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

    [Display(AutoGenerateField = false)] public virtual bool IsDocumentOpenAllow { get; set; }

    protected virtual void DocumentOpen(object obj)
    {
        //WindowManager.ShowFunctionNotReleased();
    }

    public virtual Task DocumentOpenAsync(object obj)
    {
        throw new NotImplementedException();
    }

    private string mySaveInfo;

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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
    public ICommand DocNewCommand
    {
        get { return new Command(DocNew, _ => IsDocNewEmptyAllow); }
    }

    public virtual void DocNew(object obj)
    {
        if (!IsDocNewCopyAllow && !myIsDocNewCopyRequisiteAllow)
            DocNewEmpty(obj);
    }


    [Display(AutoGenerateField = false)]
    public ICommand DocNewEmptyCommand
    {
        get { return new Command(DocNewEmpty, _ => IsDocNewEmptyAllow); }
    }

    public virtual void DocNewEmpty(object form)
    {
    }

    // ReSharper disable once InconsistentNaming
    private bool myIsDocNewEmptyAllow = true;

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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
        get { return new Command(DocNewCopy, _ => IsDocNewCopyAllow); }
    }

    public virtual void DocNewCopy(object form)
    {
    }

    private bool myIsDocNewCopyRequisiteAllow = true;

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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
        get { return new Command(DocDelete, _ => IsDocDeleteAllow); }
    }

    [Display(AutoGenerateField = false)]
    public ICommand UndoCommand
    {
        get { return new Command(Undo, _ => true); }
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand AddSearchListCommand
    {
        get { return new Command(AddSearchList, _ => true); }
    }

    public virtual void AddSearchList(object obj)
    {
        WindowManager.ShowFunctionNotReleased();
    }

    public virtual void Undo(object obj)
    {
    }


    [Display(AutoGenerateField = false)]
    public ICommand CreateLinkDocumentCommand
    {
        get { return new Command(CreateLinkDocument, _ => CanCreateLinkDocument); }
    }

    [Display(AutoGenerateField = false)]
    public ICommand SetCustomizeFormDocumentCommand
    {
        get { return new Command(SetCustomizeFormDocument, _ => true); }
    }

    [Display(AutoGenerateField = false)]
    public bool CanCustomize
    {
        set
        {
            if (value == myCanCustomize) return;
            myCanCustomize = value;
            RaisePropertyChanged();
        }
        get => myCanCustomize;
    }

    protected virtual void SetCustomizeFormDocument(object obj)
    {
        CanCustomize = !CanCustomize;
    }


    [Display(AutoGenerateField = false)]
    public ICommand SaveCustomizedFormDocumentCommand
    {
        get { return new Command(SaveCustomizedFormDocument, _ => true); }
    }

    protected virtual void SaveCustomizedFormDocument(object obj)
    {
        //WindowManager.ShowFunctionNotReleased("Не реализована сохранение измененной разметки");
    }

    public virtual void CreateLinkDocument(object obj)
    {
        WindowManager.ShowFunctionNotReleased("Не реализована генерация документа");
    }

    [Display(AutoGenerateField = false)] public virtual bool CanCreateLinkDocument { set; get; } = true;


    public virtual void DocDelete(object form)
    {
    }

    private bool myIsRedoAllow = true;

    [Display(AutoGenerateField = false)]
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
        get { return new Command(Redo, _ => IsRedoAllow); }
    }

    public virtual void Redo(object form)
    {
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand DocNewCopyRequisiteCommand
    {
        get { return new Command(DocNewCopyRequisite, _ => IsDocNewCopyRequisiteAllow); }
    }

    public virtual void DocNewCopyRequisite(object form)
    {
    }

    private bool myIsPrintAllow = true;

    [Display(AutoGenerateField = false)]
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
        get { return new Command(Print, _ => IsPrintAllow); }
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand OkCommand
    {
        get { return new Command(Ok, _ => IsOkAllow()); }
    }

    public virtual bool IsOkAllow()
    {
        return true;
    }

    [Display(AutoGenerateField = false)]
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
        get { return new Command(Cancel, _ => true); }
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
        get { return new Command(ResetLayout, _ => true); }
    }

    public virtual void ResetLayout(object form)
    {
        if (Form is ILayout layman)
            layman.LayoutManager?.ResetLayout();
        else
            LayoutManager?.ResetLayout();
        UpdateVisualObjects();
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand ShowHistoryCommand
    {
        get { return new Command(ShowHistory, _ => IsAllowHistoryShow); }
    }

    public virtual void ShowHistory(object data)
    {
    }

    [Display(AutoGenerateField = false)] public virtual bool IsAllowHistoryShow { set; get; } = true;

    [Display(AutoGenerateField = false)]
    public virtual string SearchText
    {
        get => mySearchText;
        set
        {
            if (mySearchText == value) return;
            mySearchText = value;
            RaisePropertyChanged();
        }
    }

    private bool myIsCanSearch;
    private string myFooterText;
    private bool myCanCustomize;

    [Display(AutoGenerateField = false)]
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
