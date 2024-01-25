using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Utils.CommonDialogs.Internal;
using DevExpress.Xpf.Grid;
using Helper;
using KursDomain;
using KursDomain.Menu;
using LayoutManager;

namespace Core.ViewModel.Base;

public interface IKursDialog : IKursForm
{
    DialogResult Result { set; get; }
    void Ok();
    void Cancel();
}

public interface IKursForm
{
    Window Form { set; get; }
    UserControl ModelView { set; get; }
}

public interface IKursBaseControlViewModel
{
    UserControl ModelView { set; get; }
    ObservableCollection<MenuButtonInfo> RightMenuBar { get; set; }
    ObservableCollection<MenuButtonInfo> LeftMenuBar { get; set; }
    string WindowName { get; set; }
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class KursBaseControlViewModel : KursBaseViewModel, IKursBaseControlViewModel, IKursDialog, KursDomain.ViewModel.Base2.IFormOperation
{
    #region Fields

    private bool myIsCanSaveData;

    [Display(AutoGenerateField = false)]
    protected ILayoutSerializationService LayoutSerializationService
        => GetService<ILayoutSerializationService>();

    [Display(AutoGenerateField = false)] public global::Helper.LayoutManager LayoutManager { get; set; }

    #endregion

    #region Constructors

    #endregion

    #region Properties

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

    [Display(AutoGenerateField = false)] public string DatabaseName => GlobalOptions.DataBaseName;

    [Display(AutoGenerateField = false)]
    public UserControl ModelView
    {
        get => GetValue<UserControl>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = false)] public ObservableCollection<MenuButtonInfo> RightMenuBar { get; set; }

    [Display(AutoGenerateField = false)] public ObservableCollection<MenuButtonInfo> LeftMenuBar { get; set; }

    [Display(AutoGenerateField = false)] public virtual string WindowName { get; set; }
    [Display(AutoGenerateField = false)] public virtual string LayoutName { get; set; }

    #endregion

    #region Methods

    #endregion

    #region Commands

#pragma warning disable 693
    public virtual void ObservableCollection<MenuButtonInfo>(object data)
#pragma warning restore 693
#pragma warning restore 693
    {
    }

    private bool IsSummaryExists(GridSummaryItemCollection tsums, string fname, SummaryItemType sumType)
    {
        foreach (var s in tsums)
            if (s.FieldName == fname && s.SummaryType == sumType)
                return true;

        return false;
    }

    [Display(AutoGenerateField = false)]
    public ICommand DocNewCommand
    {
        get { return new Command(DocNew, _ => IsDocNewEmptyAllow); }
    }

    public virtual void DocNew(object obj)
    {
        if (!IsDocNewCopyAllow && !IsDocNewCopyAllow)
            DocNewEmpty(obj);
    }

    public virtual void OnWindowClosing(object obj)
    {
        LayoutManager?.Save();
    }

    private void OnLayoutInitial(object obj)
    {
        if (Form != null)
            LayoutManager ??= new global::Helper.LayoutManager(Form, LayoutSerializationService,
                LayoutName, null, GlobalOptions.KursSystemDBContext);
        else
            LayoutManager = new global::Helper.LayoutManager(GlobalOptions.KursSystem(), LayoutSerializationService,
                LayoutName, null);
    }

    public virtual void OnWindowLoaded(object obj)
    {
        if (LayoutManager == null) OnLayoutInitial(null);
        LayoutManager.Load();
        if (Form == null) return;
        var grids = Form.FindVisualChildren<GridControl>();
        var trees = Form.FindVisualChildren<TreeListControl>();
        if (grids != null)
            foreach (var grid in grids)
            {
                grid.FilterString = null;
                foreach (var col in grid.Columns)
                {
                    col.Name = col.FieldName;
                    if (col.FieldType == typeof(decimal) ||
                        col.FieldType == typeof(decimal?)
                        || col.FieldType == typeof(float) || col.FieldType == typeof(float?)
                        || col.FieldType == typeof(double) || col.FieldType == typeof(double?))
                    {
                        if (IsSummaryExists(grid.TotalSummary, col.FieldName, SummaryItemType.Sum)) continue;
                        grid.TotalSummary.Add(new GridSummaryItem
                        {
                            DisplayFormat = "n2",
                            FieldName = col.FieldName,
                            SummaryType = SummaryItemType.Sum,
                            ShowInColumn = col.FieldName
                        });
                    }

                    if (col.FieldType == typeof(int) || col.FieldType == typeof(int?))
                    {
                        if (IsSummaryExists(grid.TotalSummary, col.FieldName, SummaryItemType.Count)) continue;
                        grid.TotalSummary.Add(new GridSummaryItem
                        {
                            DisplayFormat = "n0",
                            FieldName = col.FieldName,
                            SummaryType = SummaryItemType.Count,
                            ShowInColumn = col.FieldName
                        });
                    }
                }
            }

        if (trees != null)
            foreach (var t in trees)
            foreach (var col in t.Columns)
                col.Name = col.FieldName;
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand RefreshDataCommand
    {
        // ReSharper disable once UnusedParameter.Local
        get { return new Command(Load, param => CanLoad(null)); }
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

    public virtual bool CanLoad(object o)
    {
        return true;
    }

    [Command]
    //public virtual void Load()
    //{
    //}
    public virtual void Load(object obj)
    {
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand CloseWindowCommand
    {
        get { return new Command(CloseWindow, param => true); }
    }

    public virtual void CloseWindow(object form)
    {
        if (CanSave())
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

        Form.Close();
    }

    [Display(AutoGenerateField = false)]

    public Window Form
    {
        get => GetValue<Window>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = false)]
    public UserControl Control
    {
        get => GetValue<UserControl>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand SaveDataCommand
    {
        get { return new Command(SaveData, param => CanSave()); }
    }

    [Display(AutoGenerateField = false)]
    public virtual ICommand DoneCommand
    {
        get { return new Command(Done, param => IsCanDone); }
    }

    [Display(AutoGenerateField = false)] public virtual bool IsCanDone { get; set; } = false;

    public virtual void Done(object obj)
    {
        //WindowManager.ShowFunctionNotReleased();
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

    [Display(AutoGenerateField = false)] public virtual bool IsDocumentOpenAllow { get; set; }

    public virtual void DocumentOpen(object obj)
    {
        //WindowManager.ShowFunctionNotReleased();
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

    public virtual bool CanSave()
    {
        return true;
    }

    [Command]
    public virtual void Save()
    {
    }

    public virtual void SaveData(object data)
    {
        Save();
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
        get { return new Command(DocNewCopy, param => IsDocNewCopyAllow); }
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

    public ICommand ShowHistoryCommand { get; }

    [Display(AutoGenerateField = false)]
    public virtual ICommand DoсDeleteCommand
    {
        get { return new Command(DocDelete, param => IsDocDeleteAllow); }
    }

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
    private bool myIsCanRefresh;

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
        get { return new Command(Print, param => IsPrintAllow); }
    }


    public virtual bool IsOkAllow()
    {
        return true;
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
        if (this is IKursLayoutManager l)
        {
            l.LayoutManager.ResetLayout();
            return;
        }

        var layman = form as ILayout;
        layman?.LayoutManager?.ResetLayout();
    }

    [Display(AutoGenerateField = false)] public DialogResult Result { get; set; }


    [Command]
    public virtual void Ok()
    {
    }

    public virtual bool CanOk()
    {
        return true;
    }

    [Command]
    public virtual void Cancel()
    {
        Form.Close();
    }

    public virtual bool CanCancel()
    {
        return true;
    }

    #endregion

    public abstract void RefreshData(object obj);
}
