using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using Helper;
using KursDomain.Repository.LayoutRepository;
using KursDomain.View.Base;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace KursDomain.ViewModel.Base2;

public abstract class FormSearchViewModelBase<T> : ViewModelBase, IFormSearchCommands, ILayout
{
    #region Fields

    private readonly ILayoutRepository _layoutRepository;
    private DateTime _DateEnd;
    private DateTime _DateStart;
    private string _SearchText;
    private string _startLayout;

    #endregion


    #region Constructors

    public FormSearchViewModelBase()
    {
        _layoutRepository = new LayoutRepository(GlobalOptions.KursSystem());

        OnWindowClosingCommand = new DelegateCommand(OnWindowClosing);
        OnInitializeCommand = new DelegateCommand(OnInitialize);
        OnWindowLoadedCommand = new DelegateCommand(async () => await OnWindowLoaded());

        RefreshDataCommand = new DelegateCommand(async () => await OnRefreshDataAsync(), CanRefreshData);
        CloseWindowCommand = new DelegateCommand(OnCloseWindow, CanCloseWindow);
        ResetLayoutCommand = new DevExpress.Mvvm.DelegateCommand(OnResetLayoutExecute);

        DocumentOpenCommand = new DelegateCommand(async () => await OnDocumentOpenAsync(), CanDocumentOpen);
        DocNewEmptyCommand = new DelegateCommand(async () => await OnDocNewEmptyExecute(), CanDocNewEmpty);
        DocNewCopyRequisiteCommand =
            new DelegateCommand(async () => await OnDocNewCopyRequisiteExecute(), CanDocNewCopyRequisite);
        DocNewCopyCommand = new DelegateCommand(async () => await OnDocNewCopyExecute(), CanDocNewCopy);
    }


    public FormSearchViewModelBase(ILayoutRepository layoutRepository) : this()
    {
        _layoutRepository = layoutRepository;
    }

    #endregion

    #region Command

    protected virtual bool CanDocNewEmpty()
    {
        return true;
    }

    protected virtual async Task OnDocNewEmptyExecute()
    {
    }

    protected virtual bool CanDocNewCopyRequisite()
    {
        return true;
    }

    protected virtual async Task OnDocNewCopyRequisiteExecute()
    {
    }

    protected virtual bool CanDocNewCopy()
    {
        return true;
    }

    protected virtual async Task OnDocNewCopyExecute()
    {
    }

    public ICommand CloseWindowCommand { get; }

    protected async void OnWindowClosing()
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

    private bool CanCloseWindow()
    {
        return true;
    }

    private async void OnCloseWindow()
    {
        Close();
    }

    public ICommand DocumentOpenCommand { get; }

    public virtual bool CanDocumentOpen()
    {
        return CurrentDocument != null;
    }

    public virtual async Task OnDocumentOpenAsync()
    {
    }

    public ICommand DocNewCommand { get; }
    public ICommand DocNewEmptyCommand { get; }
    public ICommand DocNewCopyRequisiteCommand { get; }
    public ICommand DocNewCopyCommand { get; }
    public ICommand RefreshDataCommand { get; }

    protected virtual bool CanRefreshData()
    {
        return true;
    }

    public virtual async Task OnRefreshDataAsync()
    {
        throw new NotImplementedException();
    }

    public ICommand ResetLayoutCommand { get; }

    private void OnResetLayoutExecute()
    {
        LayoutSerializationService.Deserialize(_startLayout);
    }

    public string LayoutName { get; set; }
    public ICommand OnWindowClosingCommand { get; }
    public ICommand OnInitializeCommand { get; }

    protected virtual void OnInitialize()
    {
    }

    public ICommand OnWindowLoadedCommand { get; }

    protected virtual async Task OnWindowLoaded()
    {
        _startLayout = LayoutSerializationService.Serialize();
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

    #endregion

    #region Properties

    protected BaseSearchWindow Form { private set; get; }
    public ILayoutSerializationService LayoutSerializationService => GetService<ILayoutSerializationService>();
    public string WindowName { get; set; }

    public virtual ObservableCollection<T> Documents { get; set; }
    public virtual ObservableCollection<T> SelectedDocuments { get; set; }

    public virtual T CurrentDocument { set; get; }


    public string SearchText
    {
        get => _SearchText;
        set
        {
            if (_SearchText == value) return;
            _SearchText = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DateStart
    {
        get => _DateStart;
        set
        {
            if (_DateStart == value) return;
            _DateStart = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DateEnd
    {
        get => _DateEnd;
        set
        {
            if (_DateEnd == value) return;
            _DateEnd = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    public virtual void Show()
    {
        Form = new BaseSearchWindow
        {
            DataContext = this
        };
        Form.Show();
    }

    public void Close()
    {
        Form?.Close();
    }

    public bool IsCanSearch { get; set; } = true;

    #endregion
}
