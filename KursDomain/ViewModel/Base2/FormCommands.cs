using System.Windows.Input;
using Core.WindowsManager;
using Prism.Commands;

namespace KursDomain.ViewModel.Base2;

public abstract class FormCommands : IFormCommands
{
    public FormCommands()
    {
        SaveDataCommand = new DelegateCommand(OnSaveData, CanSaveData);
        RefreshDataCommand = new DelegateCommand(OnRefreshData, CanRefreshData);
        DocNewCopyCommand = new DelegateCommand(OnDocNewCopy, CanDocNewCopy);
        DocNewCopyRequisiteCommand = new DelegateCommand(OnDocNewCopyRequisite,CanDocNewCopyRequisite);
        DocNewEmptyCommand = new DelegateCommand(OnDocNewEmpty, CanDocNewEmpty);
        CloseWindowCommand = new DelegateCommand(OnCloseWindow, CanCloseWindow);
        DocumentOpenCommand = new DelegateCommand(OnDocumentOpen, CanDocumentOpen);
        PrintCommand = new DelegateCommand(OnPrint, CanPrint);
        ResetLayoutCommand = new DelegateCommand(OnResetLayout, CanResetLayout);
        ShowHistoryCommand = new DelegateCommand(OnShowHistory, CanShowHistory);
        DocNewCommand = new DelegateCommand(OnDocNew,CanDocNew);
        DoсDeleteCommand = new DelegateCommand(OnDoсDelete, CanDoсDelete);
        RedoCommand = new DelegateCommand(OnRedo, CanRedo);
        CreateLinkDocumentCommand = new DelegateCommand(OnCreateLinkDocument, CanCreateLinkDocument);

    }
    
    #region Command

    public ICommand CloseWindowCommand { get; }
    public ICommand DocumentOpenCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand DocNewCommand { get; }
    public ICommand DocNewEmptyCommand { get; }
    public ICommand DocNewCopyRequisiteCommand { get; }
    public ICommand DocNewCopyCommand { get; }
    public ICommand RefreshDataCommand { get; }
    public ICommand SaveDataCommand { get; }
    public ICommand ResetLayoutCommand { get; }
    public ICommand ShowHistoryCommand { get; }
    public ICommand DoсDeleteCommand { get; }

    public ICommand UndoCommand { get; }
    public ICommand CreateLinkDocumentCommand { get; }


    public ICommand RedoCommand { get; }

    //OnCreateLinkDocument, CanCreateLinkDocument
    private void OnCreateLinkDocument()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    private bool CanCreateLinkDocument()
    {
        return false;
    }

    private bool CanSaveData()
    {
        return false;
    }

    private void OnSaveData()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual  bool CanCloseWindow()
    {
        return true;
    }

    protected virtual void OnCloseWindow()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanDocumentOpen()
    {
        return false;
    }

    protected virtual void OnDocumentOpen()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanPrint()
    {
        return false;
    }

    protected virtual void OnPrint()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual  bool CanDocNewEmpty()
    {
        return false;
    }

    protected virtual void OnDocNewEmpty()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanDocNewCopyRequisite()
    {
        return false;
    }

    protected virtual void OnDocNewCopyRequisite()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanDocNewCopy()
    {
        return false;
    }

    protected virtual void OnDocNewCopy()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanRefreshData()
    {
        return false;
    }

    protected virtual void OnRefreshData()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanResetLayout()
    {
        return true;
    }

    protected virtual void OnResetLayout()
    {
        WindowManager.ShowFunctionNotReleased();
    }
    protected virtual bool CanShowHistory()
    {
        return false;
    }

    protected virtual void OnShowHistory()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanDocNew()
    {
        return true;
    }

    protected virtual void OnDocNew()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual void OnDoсDelete()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    protected virtual bool CanDoсDelete()
    {
        return false;
    }

    protected virtual bool CanRedo()
    {
        return false;
    }

    protected virtual void OnRedo()
    {
        WindowManager.ShowFunctionNotReleased();
    }

    #endregion

}
