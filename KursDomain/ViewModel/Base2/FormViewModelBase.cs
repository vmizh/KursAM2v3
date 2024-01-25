using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using Helper;
using KursDomain.Repository.LayoutRepository;
using KursDomain.View.Base;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace KursDomain.ViewModel.Base2;

public delegate void GridControlColumnsAutogenerating(object sender, AutoGeneratingColumnEventArgs e);

public abstract class FormViewModelBase : ViewModelBase, IForm, ILayout
{
    #region Constructors

    public FormViewModelBase()
    {
        _layoutRepository = new LayoutRepository(GlobalOptions.KursSystem());
         
        OnWindowClosingCommand = new DelegateCommand(OnWindowClosing);
        OnInitializeCommand = new DelegateCommand(OnInitialize);
        OnWindowLoadedCommand = new DelegateCommand(async () => await OnWindowLoaded());
    }

    public FormViewModelBase(ILayoutRepository layoutRepository) : this()
    {
        _layoutRepository = layoutRepository;
    }

    #endregion

    #region Properties

    public ILayoutSerializationService LayoutSerializationService => GetService<ILayoutSerializationService>();
    private ILayoutRepository _layoutRepository { get; }
    protected BaseWindow Form { private set; get; }

    public UserControl FormControl { get; set; }
    public string Title { get; set; }
    public string LayoutName { get; set; }

    #endregion

    #region Methods

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

    protected async Task OnWindowLoaded()
    {
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

    protected  async void OnWindowClosing()
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
            var writer = XmlWriter.Create(sb, new XmlWriterSettings()
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

    #endregion
}
