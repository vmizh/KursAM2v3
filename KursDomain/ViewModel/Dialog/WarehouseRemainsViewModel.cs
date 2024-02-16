using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KursDomain.References;
using KursDomain.Repository.WarehouseRepository;
using KursDomain.RepositoryHelper;
using KursDomain.View;
using KursDomain.ViewModel.Base2;
using Prism.Commands;

namespace KursDomain.ViewModel.Dialog;

public class WarehouseRemainsViewModel : DialogViewModelBase<NomenklStoreRemainItemWrapper>
{

    #region Fields

    private NomenklStoreRemainItemWrapper myCurrentItem;
    private readonly IWarehouseRepository myWarehouseRepository;
    private NomenklStoreRemainItemWrapper myCurrentSelectDocumentItem;

    #endregion
    

    public WarehouseRemainsViewModel(DateTime remainDate, Warehouse warehouse, IWarehouseRepository warehouseRepository)
    {
        RemainDate = remainDate;
        Warehouse = warehouse;
        myWarehouseRepository = warehouseRepository;

        LayoutName = "WarehouseRemainsViewModel";

        FormControl = new WarehouseRemainsView();

        IncludeCommand = new DelegateCommand(OnIncludeExecute, CanInclude);
        ExcludeCommand = new DelegateCommand(OnExcludeExecute, CanExclude);

        IncludeSelectedCommand = new DelegateCommand(OnIncludeSelectedExecute, CanIncludeSelected);
        ExcludeSelectedCommand = new DelegateCommand(OnExcludeSelectedExecute, CanExcludeSelected);
    }

    #region Methods


    public override async Task InitializingAsync()
    {
        Dialog.loadingIndicator.Visibility = Visibility.Visible;
        Items.Clear();
        var data = await myWarehouseRepository.GetNomenklsOnWarehouseAsync(RemainDate, Warehouse.DocCode);
        foreach (var rem in data)
        {
            Items.Add(new NomenklStoreRemainItemWrapper(rem));
        }

        Dialog.loadingIndicator.Visibility = Visibility.Hidden;
    }

    #endregion

    #region Commands

    public ICommand IncludeCommand { get; }
    public ICommand ExcludeCommand { get; }
    public ICommand IncludeSelectedCommand { get; }
    public ICommand ExcludeSelectedCommand { get; }
    private bool CanExclude()
    {
        return CurrentSelectDocumentItem != null;
    }

    private void OnExcludeSelectedExecute()
    {
        var items = SelectedDocumentItems.Select(_ => _.Nomenkl.DocCode).ToList();
        foreach (var old in items.Select(item => SelectDocumentItems.FirstOrDefault(_ => _.Nomenkl.DocCode == item))
                     .Where(old => old != null))
        {
            SelectDocumentItems.Remove(old);
        }
    }

    private bool CanExcludeSelected()
    {
        return SelectedDocumentItems is { Count: > 0 };
    }

    private void OnExcludeExecute()
    {
        SelectDocumentItems.Remove(CurrentSelectDocumentItem);
    }


    private bool CanIncludeSelected()
    {
        return SelectedItems is { Count: > 0 };
    }

    private void OnIncludeSelectedExecute()
    {
        foreach (var doc in SelectedItems)
        {
            var old = SelectDocumentItems.FirstOrDefault(_ => _.Nomenkl.DocCode == doc.Nomenkl.DocCode);
            if(old == null) 
                SelectDocumentItems.Add(doc);
        }
    }



    private bool CanInclude()
    {
        return CurrentItem != null;
    }

    private void OnIncludeExecute()
    {
        var old = SelectDocumentItems.FirstOrDefault(_ => _.Nomenkl.DocCode == CurrentItem.Nomenkl.DocCode);
        if (old != null)
        {
            return;
        }
        SelectDocumentItems.Add(CurrentItem);
    }


    protected override bool CanOk()
    {
        return CurrentItem != null;
    }

    #endregion

    #region Properties

    public ObservableCollection<NomenklStoreRemainItemWrapper> SelectDocumentItems { set; get; } =
        new ObservableCollection<NomenklStoreRemainItemWrapper>();

    public ObservableCollection<NomenklStoreRemainItemWrapper> SelectedItems { set; get; } =
        new ObservableCollection<NomenklStoreRemainItemWrapper>();

    public ObservableCollection<NomenklStoreRemainItemWrapper> SelectedDocumentItems { set; get; } =
        new ObservableCollection<NomenklStoreRemainItemWrapper>();



    public DateTime RemainDate { get; set; }
    public Warehouse Warehouse { get; private set; }

    public override NomenklStoreRemainItemWrapper CurrentItem
    {
        get => myCurrentItem;
        set
        {
            if (Equals(value, myCurrentItem)) return;
            myCurrentItem = value;
            RaisePropertyChanged(nameof(CurrentItem));
            ((DelegateCommand)OkCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)IncludeCommand).RaiseCanExecuteChanged();

        }
    }

    public NomenklStoreRemainItemWrapper CurrentSelectDocumentItem
    {
        get => myCurrentSelectDocumentItem;
        set
        {
            if (Equals(value, myCurrentSelectDocumentItem)) return;
            myCurrentSelectDocumentItem = value;
            RaisePropertyChanged(nameof(myCurrentSelectDocumentItem));
            ((DelegateCommand)ExcludeCommand).RaiseCanExecuteChanged();
        }
    }

    #endregion
}

