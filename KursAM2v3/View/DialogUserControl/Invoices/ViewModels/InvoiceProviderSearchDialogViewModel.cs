using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using Helper;
using KursAM2.View.DialogUserControl.Invoices.UserControls;

namespace KursAM2.View.DialogUserControl.Invoices.ViewModels
{
    public sealed class InvoiceProviderSearchDialogViewModel : RSWindowViewModelBase, IUpdatechildItems
    {
        #region Fields

        private ALFAMEDIAEntities context;
        private readonly InvoiceProviderSearchType loadType;
        private InvoiceProviderHead myCurrent;
        private ICurrentWindowService winCurrentService;
        private Visibility myPositionVisibility;
        private string sqlBase = @"SELECT  *
                                                FROM InvoicePostQuery p  ";
        private bool myIsAllowPositionSelected;
        private InvoiceProviderRow myCurrentSelectedItem;
        private InvoiceProviderRow myCurrentPosition;

        #endregion

        #region Constructors

        public InvoiceProviderSearchDialogViewModel(ALFAMEDIAEntities context = null)
        {
            this.context = context;
        }


        public InvoiceProviderSearchDialogViewModel(bool isAllowPosition, bool isAllowMultipleSchet,
            InvoiceProviderSearchType loadType, ALFAMEDIAEntities context = null) :
            this(context)
        {
            this.loadType = loadType;
            IsAllowPositionSelected = isAllowPosition;
            IsAllowMultipleSchet = isAllowMultipleSchet;
            if (isAllowPosition)
                CustomDataUserControl = new InvoiceListSearchMultipleView();
            else CustomDataUserControl = new InvoiceListSearchView();
            LayoutName = isAllowPosition ? "InvoiceProviderListSearchMultipleView2" : "InvoiceProviderListSearchView2";
        }

       #endregion

        #region Properties

        private List<InvoicePostQuery> Data = new List<InvoicePostQuery>();

        public ObservableCollection<InvoiceProviderHead> ItemsCollection { set; get; } =
            new ObservableCollection<InvoiceProviderHead>();

        public ObservableCollection<InvoiceProviderRow> ItemPositionsCollection { set; get; } =
            new ObservableCollection<InvoiceProviderRow>();

        public ObservableCollection<InvoiceProviderRow> ItemPositionsSelSelectedCollection { set; get; } =
            new ObservableCollection<InvoiceProviderRow>();

        public ObservableCollection<InvoiceProviderRow> SelectedItems { set; get; } =
            new ObservableCollection<InvoiceProviderRow>();

        public ObservableCollection<InvoiceProviderHead> SelectItems { set; get; } =
            new ObservableCollection<InvoiceProviderHead>();

        public Visibility PositionVisibility
        {
            get => myPositionVisibility;
            set
            {
                if (myPositionVisibility == value) return;
                myPositionVisibility = value;
                RaisePropertyChanged();
            }
        }
        public bool IsAllowMultipleSchet { get; set; }
        public bool IsAllowPositionSelected
        {
            get => myIsAllowPositionSelected;
            set
            {
                if (myIsAllowPositionSelected == value) return;
                myIsAllowPositionSelected = value;
                PositionVisibility = myIsAllowPositionSelected ? Visibility.Visible : Visibility.Hidden;
                RaisePropertyChanged();
            }
        }

        public InvoiceProviderHead CurrentItem
        {
            get => myCurrent;
            set
            {
                if (myCurrent == value) return;
                myCurrent = value;
                LoadPositions();
                RaisePropertyChanged();
            }
        }

        public InvoiceProviderRow CurrentSelectedItem
        {
            get => myCurrentSelectedItem;
            set
            {
                if (myCurrentSelectedItem == value) return;
                myCurrentSelectedItem = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProviderRow CurrentPosition
        {
            get => myCurrentPosition;
            set
            {
                if (myCurrentPosition == value) return;
                myCurrentPosition = value;
                RaisePropertyChanged();
            }
        }


        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? KontragentDC { get; set; }

        public new MessageResult DialogResult = MessageResult.No;

        public UserControl CustomDataUserControl { get; }

        #endregion

        #region Commands

        protected override void OnWindowLoaded(object obj)
        {
            if (IsLayoutLoaded) return;
            base.OnWindowLoaded(obj);
            //Запускается один раз при отсутствии сохраненной разметки
            if (LayoutName == "InvoiceProviderListSearchSingleSchetView")
            {
                ((InvoiceListSearchView)CustomDataUserControl).rowListGroup.Visibility = Visibility.Collapsed;
                ((InvoiceListSearchView)CustomDataUserControl).headListGroup.Height =
                    ((InvoiceListSearchView)CustomDataUserControl).Height - 2;
            }
        }

        public override bool IsOkAllow()
        {
            return CurrentItem != null;
        }

        public override void Ok(object obj)
        {
            winCurrentService = GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                DialogResult = MessageResult.OK;
                winCurrentService.Close();
            }
        }

        public override void Cancel(object obj)
        {
            winCurrentService = GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                CurrentItem = null;
                foreach (var item in ItemsCollection) item.IsSelected = false;
                DialogResult = MessageResult.Cancel;
                winCurrentService.Close();
            }
        }

        public override void RefreshData(object obj)
        {
            var isExistContext = context != null;
            var DataRange = (loadType & InvoiceProviderSearchType.DataRange) == InvoiceProviderSearchType.DataRange
                ? $" DocDate >= '{CustomFormat.DateToString(StartDate)}' and DocDate <= '{CustomFormat.DateToString(EndDate)}' "
                : null;
            var NotPay = (loadType & InvoiceProviderSearchType.NotPay) == InvoiceProviderSearchType.NotPay
                ? " isnull(Summa,0) > isnull(PaySumma,0) "
                : null;
            var NotShipped = (loadType & InvoiceProviderSearchType.NotShipped) == InvoiceProviderSearchType.NotShipped
                ? " isnull(Quantity,0) > isnull(Shipped,0) AND IsUsluga = 0 "
                : null;
            var OneKontragent = KontragentDC != null && (loadType & InvoiceProviderSearchType.OneKontragent) ==
                InvoiceProviderSearchType.OneKontragent
                    ? $" PostDC = {CustomFormat.DecimalToSqlDecimal(KontragentDC)}"
                    : null;
            var OnlyDistribute =
                (loadType & InvoiceProviderSearchType.OnlyNakladDistrubuted) ==
                InvoiceProviderSearchType.OnlyNakladDistrubuted
                    ? " IsInvoiceNakladId = 1 AND round(Summa,2) - round(NakladDistributedSumma,2) != 0 "
                    : null;
            var RemoveNaklad =
                (loadType & InvoiceProviderSearchType.RemoveNakladRashod) ==
                InvoiceProviderSearchType.RemoveNakladRashod
                    ? " IsUsluga != 1 AND IsAccepted = 1 and RowId NOT IN (SELECT isnull(d.TovarInvoiceRowId,newid()) FROM DistributeNakladRow d)  " +
                      @" AND RowId NOT IN (SELECT ISNULL(dd.TransferRowId, NEWID()) 
                            FROM DistributeNakladRow dd
                                INNER JOIN TD_26 t ON dd.TransferRowId = t.Id) " : null;

            try
            {
                var andString = " AND ";
                if (!isExistContext)
                    context = new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
                if (DataRange != null || NotPay != null || NotShipped != null || OneKontragent != null || OnlyDistribute != null || RemoveNaklad != null)
                {
                    sqlBase += " WHERE " + (DataRange != null ? DataRange + andString : null)
                               + (NotPay != null ? NotPay + andString : null)
                               + (NotShipped != null ? NotShipped + andString : null)
                               + (OneKontragent != null ? OneKontragent + andString: null)
                               + (OnlyDistribute != null ? OnlyDistribute + andString : null)
                               + (RemoveNaklad != null ? RemoveNaklad + andString : null);
                    sqlBase = sqlBase.Remove(sqlBase.Length - andString.Length, andString.Length);
                }

                Data.Clear();
                // ReSharper disable once PossibleNullReferenceException
                Data = context.Database.SqlQuery<InvoicePostQuery>(sqlBase).ToList();
                foreach (var d in Data)
                {
                    if (ItemsCollection.Any(_ => _.DocCode == d.DocCode)) continue;
                    ItemsCollection.Add(new InvoiceProviderHead(d));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            finally
            {
                if (!isExistContext && context != null)
                    context.Dispose();
            }
        }

        #endregion

        #region Methods

        public void UpdateSelectedItems()
        {
            SelectedItems.Remove(CurrentSelectedItem);
        }

        public void UpdatePositionItem()
        {
            if (CurrentPosition.IsSelected)
            {
                if (SelectedItems.Any(_ => _.NomenklDC == CurrentItem.NomenklDC))
                {
                    CurrentItem.IsSelected = false;
                    return;
                }

                SelectedItems.Add(CurrentPosition);
            }
            else
            {
                SelectedItems.Remove(CurrentPosition);
            }
        }

        public void UpdateInvoiceItem()
        {
            if (CurrentItem == null) return;
            if (CurrentItem.IsSelected)
            {
                if (!IsAllowMultipleSchet)
                {
                    foreach (var d in ItemsCollection.Where(_ => _.IsSelected && _.PostDC != CurrentItem.PostDC))
                    {
                        d.IsSelected = false;
                        var dcs = SelectedItems.Where(_ => _.DocCode == d.DocCode).ToList();
                        foreach (var r in dcs)
                        {
                            SelectedItems.Remove(r);
                        }
                    }
                }

                foreach (var pos in ItemPositionsCollection)
                {
                    if (SelectedItems.All(_ => _.NomenklDC != pos.NomenklDC))
                    {
                        pos.IsSelected = true;
                        SelectedItems.Add(pos);
                    }
                    else
                    {
                        pos.IsSelected = false;
                        SelectedItems.Remove(pos);
                    }
                }
            }
            else
                foreach (var pos in ItemPositionsCollection)
                {
                    pos.IsSelected = false;
                    SelectedItems.Remove(pos);
                }

            RaisePropertyChanged(nameof(ItemPositionsCollection));
            RaisePropertyChanged(nameof(SelectedItems));
        }

        private void LoadPositions()
        {
            ItemPositionsCollection.Clear();
            if (CurrentItem == null) return;
            foreach (var pos in Data.Where(_ => _.DocCode == CurrentItem.DocCode))
                ItemPositionsCollection.Add(new InvoiceProviderRow
                {
                    IsSelected = SelectedItems.Any(_ => _.RowId == pos.RowId),
                    PostDC = pos.PostDC,
                    DocCode = pos.DocCode,
                    CODE = pos.CODE,
                    Id = pos.Id,
                    RowId = pos.RowId,
                    Nomenkl = pos.Nomenkl,
                    NomenklNumber = pos.NomenklNumber,
                    IsUsluga = pos.IsUsluga,
                    Quantity = pos.Quantity,
                    Price = pos.Price,
                    Shipped = pos.Shipped,
                    NDSPercent = pos.NDSPercent,
                    NDSSumma = pos.NDSSumma,
                    SHPZ = pos.SHPZ,
                    NomenklDC = pos.NomenklDC,
                    Note = pos.RowNote,
                });
        }

        #endregion
    }
}