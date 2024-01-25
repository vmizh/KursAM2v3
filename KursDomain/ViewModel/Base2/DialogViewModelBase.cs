using KursDomain.View.Base;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace KursDomain.ViewModel.Base2;

public abstract class DialogViewModelBase : ViewModelBase, IForm, ILayout
{
    #region Constructors

    public DialogViewModelBase()
    {
        OnWindowClosingCommand = new DelegateCommand(OnWindowClosing);
        OnInitializeCommand = new DelegateCommand(OnInitialize);
        OnWindowLoadedCommand = new DelegateCommand(OnWindowLoaded);
    }
    

    #endregion

    #region Properties

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
    }

    public void Close()
    {
        throw new NotImplementedException();
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

    protected virtual void OnWindowLoaded()
    {
        
    }

    protected virtual void OnInitialize()
    {
        
    }

    protected virtual void OnWindowClosing()
    {
       
    }

    #endregion
}
