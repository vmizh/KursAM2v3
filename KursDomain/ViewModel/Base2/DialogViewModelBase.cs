using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Helper;
using KursDomain.View.Base;
using KursDomain.Repository.LayoutRepository;
using Prism.Events;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace KursDomain.ViewModel.Base2;

public enum KursDialogResult
{
    Ok,
    Cancel,
}

public interface IInitialize
{
    Task InitializingAsync();
}

public abstract class DialogViewModelBase<T> : ViewModelBase, IDialog<T>, ILayout, IInitialize
{
    #region Fields

    private readonly ILayoutRepository _layoutRepository;
    private string _startLayout;

    #endregion

    #region Constructors

    public DialogViewModelBase()
    {
        _layoutRepository = new LayoutRepository(GlobalOptions.KursSystem());
        OnWindowClosingCommand = new DelegateCommand(OnWindowClosing);
        OnInitializeCommand = new DelegateCommand(OnInitialize);
        OnWindowLoadedCommand = new DelegateCommand(async () => await OnWindowLoaded());

        OkCommand = new DelegateCommand(OnOkExecute, CanOk);
        CancelCommand = new DelegateCommand(OnCancelExecute, CanCancel);

        ResetLayoutCommand = new DelegateCommand(OnResetLayoutExecute, CanResetLayout);
        KeyEnterCommand = new DelegateCommand(OnKeyEnterExecute);
    }

    protected virtual bool CanResetLayout()
    {
        return true;
    }

    protected virtual void OnResetLayoutExecute()
    {
        LayoutSerializationService.Deserialize(_startLayout);
    }

    protected virtual bool CanCancel()
    {
        return true;
    }

    protected virtual void OnCancelExecute()
    {
        Result = KursDialogResult.Cancel;
        Dialog.Close();
    }

    protected virtual bool CanOk()
    {
        return CurrentItem != null;
    }

    protected virtual void OnOkExecute()
    {
        Result = KursDialogResult.Ok;
        Dialog.Close();
    }

    #endregion

    #region Properties
    public ILayoutSerializationService LayoutSerializationService => GetService<ILayoutSerializationService>();
    public KursDialogResult Result { set; get; } = KursDialogResult.Cancel;
    public BaseDialogWindow Dialog { set; get; }

    public UserControl FormControl { get; set; }
    public string Title { get; set; }

    public virtual Task InitializingAsync()
    {
        throw new NotImplementedException();
    }

    public ICollection<T> Items { get; set; } = new ObservableCollection<T>();
    public virtual T CurrentItem { get; set; }

    public IEventAggregator EventAggregator => new EventAggregator();

    public string LayoutName { get; set; }

    #endregion

    #region Methods

    public async virtual Task ShowAsync()
    {
        Dialog = new BaseDialogWindow
        {
            DataContext = this
        };
        Dialog.ShowDialog();
    }

    public virtual void Show()
    {
        Dialog = new BaseDialogWindow
        {
            DataContext = this
        };
        Dialog.ShowDialog();
    }

    public void Close()
    {
        Dialog.Close();
    }

    public virtual void LoadReferncesAsync()
    {
        
    }

    #endregion

    #region Commands

    public ICommand KeyEnterCommand { get; }

    protected virtual void OnKeyEnterExecute()
    {
        
    }

    public ICommand OnWindowClosingCommand { get; }
    public ICommand OnInitializeCommand { get; }
    public ICommand OnWindowLoadedCommand { get; }
    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ResetLayoutCommand { get; }

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

    protected virtual void OnInitialize()
    {
    }

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

    #endregion
}
