using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.View.DialogUserControl.Invoices.UserControls;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.View.DialogUserControl.Invoices.ViewModels
{
    public class InvoicePostQueryConvertCurrency : InvoicePostQuery
    {
        public Guid? CurrencyConvertId { set; get; }
    }

    public sealed class InvoiceProviderSearchDialogViewModel : RSWindowViewModelBase, IUpdatechildItems
    {
        #region Fields

        private ALFAMEDIAEntities context;
        private readonly InvoiceProviderSearchType loadType;
        private InvoiceProviderHead myCurrent;
        private ICurrentWindowService winCurrentService;
        private Visibility myPositionVisibility;
        private string sqlBase = @"SELECT * FROM InvoicePostQuery p (NOLOCK) ";
        private bool myIsAllowPositionSelected;
        private InvoiceProviderRow myCurrentSelectedItem;
        private InvoiceProviderRow myCurrentPosition;
        public Currency myCurrency;
        public bool IsLoadNotDistributeCurrencyConvert = false;

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
        }

        #endregion

        #region Properties

        public List<Tuple<decimal, decimal, int>> ExistingRows { set; get; }

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

        public InvoiceProviderHead SelectItem
        {
            get => mySelectItem;
            set
            {
                if (mySelectItem == value) return;
                mySelectItem = value;
                RaisePropertyChanged();
            }
        }

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
        private InvoiceProviderHead mySelectItem;

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

            if (Form is SelectInvoiceMultipleDialogView frm)
            {
                var profitFormatCondition = new FormatCondition
                {
                   Expression = "[IsNotCanSelected]",
                   Format = new Format
                   {
                       Foreground = Brushes.Red
                   }
                };
                frm.gridViewPosition.FormatConditions.Add(profitFormatCondition);
            }
        }

        public override bool IsOkAllow()
        {
            return CurrentItem != null;
        }

        public override void Ok(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                DialogResult = MessageResult.OK;
                winCurrentService.Close();
            }
        }

        public override void Cancel(object obj)
        {
            winCurrentService = this.GetService<ICurrentWindowService>();
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
            var currency = (loadType & InvoiceProviderSearchType.IsCurrencyUsed) ==
                           InvoiceProviderSearchType.IsCurrencyUsed
                           && myCurrency != null
                ? $" CurrencyDC = {CustomFormat.DecimalToSqlDecimal(myCurrency.DocCode)} "
                : null;
            var RemoveNaklad =
                (loadType & InvoiceProviderSearchType.RemoveNakladRashod) ==
                InvoiceProviderSearchType.RemoveNakladRashod
                    ? " IsUsluga != 1 AND IsAccepted = 1 and RowId NOT IN (SELECT isnull(d.TovarInvoiceRowId,newid()) FROM DistributeNakladRow d)  " +
                      @" AND RowId NOT IN (SELECT ISNULL(dd.TransferRowId, NEWID()) 
                            FROM DistributeNakladRow dd
                                INNER JOIN TD_26 t ON dd.TransferRowId = t.Id) "
                    : null;

            try
            {
                var andString = " AND ";
                if (!isExistContext)
                    context = new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
                if (DataRange != null || NotPay != null || NotShipped != null || OneKontragent != null ||
                    OnlyDistribute != null || RemoveNaklad != null)
                {
                    sqlBase += " WHERE " + (DataRange != null ? DataRange + andString : null)
                                         + (NotPay != null ? NotPay + andString : null)
                                         + (NotShipped != null ? NotShipped + andString : null)
                                         + (OneKontragent != null ? OneKontragent + andString : null)
                                         + (OnlyDistribute != null ? OnlyDistribute + andString : null)
                                         + (RemoveNaklad != null ? RemoveNaklad + andString : null)
                                         + (currency != null ? currency + andString : null);
                    sqlBase = sqlBase.Remove(sqlBase.Length - andString.Length, andString.Length);
                }

                Data.Clear();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Data = ctx.Database.SqlQuery<InvoicePostQuery>(sqlBase).Where(h => ExistingRows == null ||
                        !ExistingRows.Any(_ =>
                            _.Item1 == h.NomenklDC && _.Item2 == h.DocCode
                                                   && _.Item3 == h.CODE)).ToList();

                    if (IsLoadNotDistributeCurrencyConvert)
                    {
                        var convData = new List<TD_26_CurrencyConvert>();
                        if (currency == null)
                        {
                            convData = ctx.TD_26_CurrencyConvert
                                .Include(_ => _.TD_26)
                                .Include(_ => _.TD_26.SD_26)
                                .Include(_ => _.DistributeNakladRow)
                                .Where(_ => _.TD_26 != null && _.DistributeNakladRow.Count == 0).ToList();
                        }

                        else
                        {
                            var data = ctx.TD_26_CurrencyConvert
                                .Include(_ => _.TD_26)
                                .Include(_ => _.TD_26.SD_26)
                                .Include(_ => _.DistributeNakladRow)
                                .ToList().Where(_ => _.TD_26 != null && _.DistributeNakladRow.Count == 0);
                            foreach (var d in data)
                            {
                                var n = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklId);
                                if (((IDocCode)n.Currency).DocCode != myCurrency.DocCode) continue;
                                convData.Add(d);
                            }
                        }

                        foreach (var c in convData)
                        {
                            var nom = GlobalOptions.ReferencesCache.GetNomenkl(c.NomenklId) as Nomenkl;
                            if (ExistingRows != null && ExistingRows.Any(d => d.Item1 == nom.DocCode &&
                                                                              d.Item2 == c.TD_26.SD_26.DOC_CODE
                                                                              && d.Item3 == (c.CODE ?? 0)))
                                continue;
                            Data.Add(new InvoicePostQueryConvertCurrency
                            {
                                Currency = ((IName)nom.Currency)?.Name,
                                Quantity = c.Quantity,
                                Summa = c.Summa,
                                Creator = c.TD_26.SD_26.CREATOR,
                                CurrencyDC = ((IDocCode)nom.Currency).DocCode,
                                DocCode = c.TD_26.SD_26.DOC_CODE,
                                CODE = (int)c.CODE,
                                Id = c.TD_26.SD_26.Id,
                                InNum = c.TD_26.SD_26.SF_IN_NUM,
                                IsAccepted = c.TD_26.SD_26.SF_ACCEPTED == 1,
                                PostSumma = c.Summa,
                                PostavNum = c.TD_26.SD_26.SF_POSTAV_NUM,
                                Date = c.TD_26.SD_26.SF_POSTAV_DATE,
                                PrihodDate = c.Date,
                                RegisterDate = c.TD_26.SD_26.SF_POSTAV_DATE,
                                Price = c.Price,
                                Note = c.Note,
                                CurrencyConvertId = c.Id,
                                NomenklNumber = nom.NomenklNumber,
                                Nomenkl = nom.Name,
                                NomenklDC = nom.DocCode,
                                PostDC = c.TD_26.SD_26.SF_POST_DC,
                                Post = ((IName)GlobalOptions.ReferencesCache.GetKontragent(c.TD_26.SD_26.SF_POST_DC))
                                    ?.Name
                            });
                        }
                    }
                }

                foreach (var d in Data)
                {
                    if (ItemsCollection.Any(_ => _.DocCode == d.DocCode)) continue;
                    var newItem = new InvoiceProviderHead(d)
                    {
                        IsSelected = false
                    };
                    ItemsCollection.Add(newItem);
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

            CurrentItem.IsSelected = null;
            if (ItemPositionsCollection.All(_ => _.IsSelected))
                CurrentItem.IsSelected = true;
            if (ItemPositionsCollection.All(_ => !_.IsSelected))
                CurrentItem.IsSelected = false;
            RaisePropertyChanged(nameof(ItemsCollection));
        }

        public void UpdateInvoiceItem()
        {
            if (CurrentItem == null) return;
            if (CurrentItem.IsSelected == true)
            {
                if (!IsAllowMultipleSchet)
                    foreach (var d in ItemsCollection.Where(_ =>
                                 (_.IsSelected ?? false) && _.PostDC != CurrentItem.PostDC))
                    {
                        d.IsSelected = false;
                        var dcs = SelectedItems.Where(_ => _.DocCode == d.DocCode).ToList();
                        foreach (var r in dcs) SelectedItems.Remove(r);
                    }

                foreach (var pos in ItemPositionsCollection)
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
            else
            {
                foreach (var pos in ItemPositionsCollection)
                {
                    pos.IsSelected = false;
                    SelectedItems.Remove(pos);
                }

                CurrentItem.IsSelected = false;
            }

            RaisePropertyChanged(nameof(ItemPositionsCollection));
            RaisePropertyChanged(nameof(SelectedItems));
        }

        private void LoadPositions()
        {
            ItemPositionsCollection.Clear();
            if (CurrentItem == null) return;
            foreach (var pos in Data.Where(_ => _.DocCode == CurrentItem.DocCode))
            {
                var newItem = new InvoiceProviderRow
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
                    Note = pos.RowNote
                };
                newItem.IsNotCanSelected = ExistingRows.Any(_ => _.Item1 == pos.NomenklDC);
                ItemPositionsCollection.Add(newItem);
            }
        }

        #endregion
    }
}
