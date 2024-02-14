using System;
using System.Threading.Tasks;
using System.Windows;
using KursDomain.References;
using KursDomain.Repository.WarehouseRepository;
using KursDomain.RepositoryHelper;
using KursDomain.View;
using KursDomain.ViewModel.Base2;
using Prism.Commands;

namespace KursDomain.ViewModel.Dialog;

public class WarehouseRemainsViewModel : DialogViewModelBase<NomenklStoreRemainItemWrapper>
{
    private NomenklStoreRemainItemWrapper myCurrentItem;
    private readonly IWarehouseRepository myWarehouseRepository;

    public WarehouseRemainsViewModel(DateTime remainDate, Warehouse warehouse, IWarehouseRepository warehouseRepository)
    {
        RemainDate = remainDate;
        Warehouse = warehouse;
        myWarehouseRepository = warehouseRepository;

        LayoutName = "WarehouseRemainsViewModel";

        FormControl = new WarehouseRemainsView();
    }    

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
        }
    }

    protected override bool CanOk()
    {
        return CurrentItem != null;
    }

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
}
