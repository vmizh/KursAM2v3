using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using Helper;
using KursDomain.Repository.LayoutRepository;
using KursDomain.View.Base;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace KursDomain.ViewModel.Base2;

public delegate void GridControlColumnsAutogenerating(object sender, AutoGeneratingColumnEventArgs e);

public abstract class FormViewModelBase<T, I> : ViewModelBase, IForm, ILayout
{
    #region Constructors

    public FormViewModelBase()
    {
        _layoutRepository = new LayoutRepository(GlobalOptions.KursSystem());

        OnWindowClosingCommand = new DelegateCommand(OnWindowClosing);
        OnInitializeCommand = new DelegateCommand(OnInitialize);
        OnWindowLoadedCommand = new DelegateCommand(async () => await OnWindowLoaded());

        DocNewEmptyCommand = new DelegateCommand(async () => await OnDocNewEmptyExecueAsync(), CanDocNewEmpty);
        DocNewCopyRequisiteCommand =
            new DelegateCommand(async () => await OnDocNewCopyRequisiteExecueAsync(), CanDocNewCopyRequisite);
        DocNewCopyCommand = new DelegateCommand(async () => await OnDocNewCopyExecueAsync(), CanNewCopyEmpty);
        DoсDeleteCommand = new DelegateCommand(async () => await OnDoсDeleteExecuteAsync(), CanDoсDelete);
        UndoCommand = new DelegateCommand(OnUndoExecute, CanUndo);

        RefreshDataCommand = new DelegateCommand(async () => await OnRefreshDataAsync(), CanRefreshData);
        SaveDataCommand = new DelegateCommand(async () => await OnSaveDataAsync(), CanSaveData);
        CloseWindowCommand = new DelegateCommand(OnCloseWindow, CanCloseWindow);
        ResetLayoutCommand = new DelegateCommand(OnResetLayoutExecute);

        ShowHistoryCommand = new DelegateCommand(OnShowHistoryExecute);
    }

    #endregion

    #region Fields

    private bool _hasChanges;
    private string _startLayout;

    #endregion

    #region Properties

    public I DocumentId { get; set; }

    public T Document { set; get; }

    public bool HasChanges
    {
        get => _hasChanges;
        set
        {
            if (_hasChanges == value) return;
            _hasChanges = value;
            RaisePropertyChanged();
            ((DelegateCommand)SaveDataCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsNewDocument { get; set; }

    public ILayoutSerializationService LayoutSerializationService => GetService<ILayoutSerializationService>();
    private ILayoutRepository _layoutRepository { get; }
    protected BaseWindow Form { private set; get; }

    public UserControl FormControl { get; set; }
    public string Title { get; set; }
    public string LayoutName { get; set; }

    #endregion

    #region Methods

    public abstract Task InitializeAsync(I id, DocumentNewState newState = DocumentNewState.None);
    public abstract void Initialize(I id, DocumentNewState newState = DocumentNewState.None);

    public virtual void Show()
    {
        var win = new BaseWindow
        {
            DataContext = this
        };
        win.Show();
        Form = win;
    }

    public void Close()
    {
        Form?.Close();
    }

    public virtual void LoadReferncesAsync()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Commands

    public ICommand OnWindowClosingCommand { get; }
    public ICommand OnInitializeCommand { get; }
    public ICommand OnWindowLoadedCommand { get; }

    protected virtual async Task OnWindowLoaded()
    {
        _startLayout = LayoutSerializationService.Serialize();
        // ReSharper disable once PossibleNullReferenceException
        var l = await _layoutRepository.GetByFormNameAsync(LayoutName);
        if (l == null) return;
        LayoutSerializationService.Deserialize(l.Layout);
        var currentWindow = ((CurrentWindowService)ServiceContainer.GetService<ICurrentWindowService>()).Window;
        if (l.WindowState != null && currentWindow != null)
        {
            var d = new DataContractSerializer(typeof(WindowsScreenState));
            if (d.ReadObject(XmlReader.Create(new StringReader(l.WindowState))) is not WindowsScreenState p) return;
            currentWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            currentWindow.WindowState = p.FormState;
            currentWindow.Height = p.FormHeight;
            currentWindow.Width = p.FormWidth;
            currentWindow.Left = p.FormLeft < 0 ? 0 : p.FormLeft;
            currentWindow.Top = p.FormTop < 0 ? 0 : p.FormTop;
        }
    }

    protected virtual void OnInitialize()
    {
    }

    protected virtual async void OnWindowClosing()
    {
        string winState = null;
        var currentWindow = ((CurrentWindowService)ServiceContainer.GetService<ICurrentWindowService>()).Window;
        if (currentWindow != null)
        {
            var saveLayout = new WindowsScreenState
            {
                FormHeight = currentWindow.Height,
                FormWidth = currentWindow.Width,
                FormLeft = currentWindow.WindowState == WindowState.Maximized ? 0 : currentWindow.Left,
                FormTop = currentWindow.WindowState == WindowState.Maximized ? 0 : currentWindow.Top,
                FormState = currentWindow.WindowState
            };
            var sb = new StringBuilder();
            var ser1 =
                new DataContractSerializer(typeof(WindowsScreenState));
            var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Async = true
            });
            ser1.WriteObject(writer, saveLayout);
            await writer.FlushAsync();
            winState = sb.ToString();
        }

        var layout = LayoutSerializationService.Serialize();
        // ReSharper disable once AssignNullToNotNullAttribute
        await _layoutRepository.SaveLayoutAsync(LayoutName, layout, winState);
    }

    protected virtual bool CanDelete()
    {
        return !IsNewDocument && !IsBusy;
    }

    protected bool IsBusy { get; set; }

    protected virtual void OnDelete()
    {
    }

    public ICommand CloseWindowCommand { get; }
    public ICommand DocumentOpenCommand { get; }

    public virtual bool CanCloseWindow()
    {
        return !IsBusy;
    }

    private async void OnCloseWindow()
    {
        if (HasChanges)
        {
            var wm = new WindowManager();
            var dlgRslt = wm.ShowKursDialog("Внесены изменения, сохранить?", "Запрос",
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

        Close();
    }

    public ICommand PrintCommand { get; }

    public ICommand DocNewCommand { get; }


    public ICommand DocNewEmptyCommand { get; }

    protected virtual bool CanDocNewEmpty()
    {
        return true;
    }

    protected virtual async Task OnDocNewEmptyExecueAsync()
    {
        throw new NotImplementedException();
    }

    public ICommand DocNewCopyRequisiteCommand { get; }

    protected virtual async Task OnDocNewCopyRequisiteExecueAsync()
    {
        throw new NotImplementedException();
    }

    protected virtual bool CanDocNewCopyRequisite()
    {
        return false;
    }

    public ICommand DocNewCopyCommand { get; }

    protected virtual bool CanNewCopyEmpty()
    {
        return true;
    }

    protected virtual async Task OnDocNewCopyExecueAsync()
    {
        throw new NotImplementedException();
    }

    public ICommand RefreshDataCommand { get; }

    protected virtual bool CanRefreshData()
    {
        return true;
    }


    public virtual async Task OnRefreshDataAsync()
    {
    }

    public ICommand SaveDataCommand { get; }

    protected virtual bool CanSaveData()
    {
        return false;
    }

    protected virtual async Task OnSaveDataAsync()
    {
        throw new NotImplementedException();
    }


    public ICommand ResetLayoutCommand { get; }

    protected virtual void OnResetLayoutExecute()
    {
        LayoutSerializationService.Deserialize(_startLayout);
    }

    public ICommand ShowHistoryCommand { get; }

    protected virtual void OnShowHistoryExecute()
    {
        
    }

    public ICommand DoсDeleteCommand { get; }

    protected virtual bool CanDoсDelete()
    {
        return !IsNewDocument;
    }

    protected virtual async Task OnDoсDeleteExecuteAsync()
    {
    }

    public ICommand UndoCommand { get; }

    protected virtual bool CanUndo()
    {
        return _hasChanges;
    }

    protected virtual void OnUndoExecute()
    {
        throw new NotImplementedException();
    }

    #endregion
}
