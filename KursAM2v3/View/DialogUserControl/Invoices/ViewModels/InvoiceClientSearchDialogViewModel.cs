﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.View.DialogUserControl.Invoices.UserControls;
using KursDomain;

namespace KursAM2.View.DialogUserControl.Invoices.ViewModels
{
    public sealed class InvoiceClientSearchDialogViewModel : RSWindowViewModelBase, IUpdatechildItems
    {
        /// <summary>
        /// Рижим работы диалога выбора. При контрагенте - выбор только одного счета,
        /// при не выбранном контрагенте фильтрация счетов по первому выбранному в диалоге контрагенту
        /// </summary>
        public enum OperatingMode
        {
            SelectKontragent = 0,
            AllKontragent = 1
        }

        #region Fields

        private ALFAMEDIAEntities context;
        private readonly InvoiceClientSearchType loadType;
        private InvoiceClientHead myCurrent;
        private ICurrentWindowService winCurrentService;
        private Visibility myPositionVisibility;
        private string sqlBase = "SELECT * FROM InvoiceClientQuery ";
        private bool myIsAllowPositionSelected;
        private InvoiceClientRow myCurrentSelectedItem;
        private InvoiceClientRow myCurrentPosition;
        private readonly List<decimal> myExclude;

        private readonly OperatingMode _operatingMode;
        private bool _IsAllowMultipleSchet;



        #endregion

        #region Constructors

        public InvoiceClientSearchDialogViewModel(ALFAMEDIAEntities context = null)
        {
            this.context = context;
        }


        public InvoiceClientSearchDialogViewModel(bool isAllowPosition, bool isAllowMultipleSchet,
            InvoiceClientSearchType loadType, OperatingMode mode = OperatingMode.AllKontragent,
            List<decimal> exclude = null, ALFAMEDIAEntities context = null) :
            this(context)
        {
            _IsAllowMultipleSchet = isAllowMultipleSchet;
            _operatingMode = mode;
            this.loadType = loadType;
            myExclude = exclude;
            IsAllowPositionSelected = isAllowPosition;
            if (isAllowMultipleSchet)
                CustomDataUserControl = new InvoiceListSearchMultipleView();
            else CustomDataUserControl = new InvoiceListSearchView();
            if (isAllowMultipleSchet)
                LayoutName = "InvoiceClientListSearchMultipleView";
            else
                LayoutName = isAllowPosition
                    ? "InvoiceClientListSearchPositionView"
                    : "InvoiceClientListSearchSingleSchetView";
        }

        #endregion

        #region Properties

        private List<InvoiceClientQuery> Data = new List<InvoiceClientQuery>();

        public ObservableCollection<InvoiceClientHead> ItemsCollection { set; get; } =
            new ObservableCollection<InvoiceClientHead>();

        public ObservableCollection<InvoiceClientHead> HiddenItemsCollection { set; get; } =
            new ObservableCollection<InvoiceClientHead>();

        public ObservableCollection<InvoiceClientRow> ItemPositionsCollection { set; get; } =
            new ObservableCollection<InvoiceClientRow>();

        public ObservableCollection<InvoiceClientRow> SelectedItems { set; get; } =
            new ObservableCollection<InvoiceClientRow>();

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

        public InvoiceClientHead CurrentItem
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

        public InvoiceClientRow CurrentSelectedItem
        {
            get => myCurrentSelectedItem;
            set
            {
                if (myCurrentSelectedItem == value) return;
                myCurrentSelectedItem = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceClientRow CurrentPosition
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
            if (LayoutName == "InvoiceClientListSearchSingleSchetView")
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
            HiddenItemsCollection.Clear();
            var isExistContext = context != null;
            var DataRange = (loadType & InvoiceClientSearchType.DataRange) == InvoiceClientSearchType.DataRange
                ? $" DocDate >= '{CustomFormat.DateToString(StartDate)}' and DocDate <= '{CustomFormat.DateToString(EndDate)}' "
                : null;
            var NotPay = (loadType & InvoiceClientSearchType.NotPay) == InvoiceClientSearchType.NotPay
                ? " isnull(Summa,0) > isnull(PaySumma,0) "
                : null;
            var NotShipped = (loadType & InvoiceClientSearchType.NotShipped) == InvoiceClientSearchType.NotShipped
                ? " isnull(Quantity,0) > isnull(Shipped,0) AND IsUsluga = 0 "
                : null;
            var OneKontragent = KontragentDC != null && (loadType & InvoiceClientSearchType.OneKontragent) ==
                InvoiceClientSearchType.OneKontragent
                    ? $" ClientDC = {CustomFormat.DecimalToSqlDecimal(KontragentDC)}"
                    : null;
            var OnlyAccepted = (loadType & InvoiceClientSearchType.OnlyAccepted) == InvoiceClientSearchType.OnlyAccepted
                ? "IsAccepted = 1"
                : null;
            try
            {
                if (!isExistContext)
                    context = new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
                if (DataRange != null || NotPay != null || NotShipped != null || OneKontragent != null)
                {
                    sqlBase += " WHERE " + (DataRange != null ? DataRange + " AND " : null) +
                               (NotPay != null ? NotPay + " AND " : null) +
                               (NotShipped != null ? NotShipped + " AND " : null) +
                               (OneKontragent != null ? OneKontragent + " AND " : null) +
                               (OnlyAccepted != null ? OnlyAccepted + " AND " : null);
                    sqlBase = sqlBase.Remove(sqlBase.Length - 4, 4);
                }

                Data.Clear();
                // ReSharper disable once PossibleNullReferenceException
                Data = context.Database.SqlQuery<InvoiceClientQuery>(sqlBase).ToList();
                foreach (var d in Data)
                {
                    if (ItemsCollection.Any(_ => _.DocCode == d.DocCode)) continue;
                    //if (myExclude is { Count: > 0 } && myExclude.Any(_ => _ == d.NomenklDC)) continue; 
                    ItemsCollection.Add(new InvoiceClientHead(d));
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

        public void UpdateVisible()
        {
            if (_operatingMode == OperatingMode.SelectKontragent) return;
            var kontr = ItemsCollection.FirstOrDefault(_ => _.IsSelected);
            if (kontr != null)
            {
                if (HiddenItemsCollection.Any()) return;
                foreach (var row in ItemsCollection.Where(_ => _.ClientDC != kontr.ClientDC))
                    HiddenItemsCollection.Add(row);

                foreach (var row in HiddenItemsCollection)
                {
                    var sel = ItemPositionsCollection.Where(_ => _.DocCode == row.DocCode).ToList();

                    foreach (var item in sel) ItemPositionsCollection.Remove(item);

                    ItemsCollection.Remove(row);
                }

                return;
            }

            var posRow = ItemPositionsCollection.FirstOrDefault(_ => _.IsSelected);
            if (posRow != null)
            {
                if (HiddenItemsCollection.Any()) return;
                foreach (var row in ItemsCollection.Where(_ => _.ClientDC != posRow.ClientDC))
                    HiddenItemsCollection.Add(row);
                foreach (var row in HiddenItemsCollection)
                {
                    var sel = ItemPositionsCollection.Where(_ => _.DocCode == row.DocCode).ToList();

                    foreach (var item in sel) ItemPositionsCollection.Remove(item);

                    ItemsCollection.Remove(row);
                }

                return;
            }

            foreach (var row in HiddenItemsCollection) ItemsCollection.Add(row);
            HiddenItemsCollection.Clear();
        }

        public void UpdateInvoiceItem()
        {
            if (CurrentItem == null) return;
            if (!_IsAllowMultipleSchet)
            {
                foreach (var item in ItemsCollection.Where(_ => _.DocCode != CurrentItem.DocCode))
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                    SelectedItems.Clear();
                }

                var frm = CustomDataUserControl as InvoiceListSearchView;
                if (CustomDataUserControl is InvoiceListSearchMultipleView frmMulti)
                {
                    frmMulti.gridControlSearch.RefreshData();
                }

                frm?.gridControlSearch.RefreshData();

                RaisePropertyChanged(nameof(ItemsCollection));
            }
            if (CurrentItem.IsSelected)
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
            {
                if (myExclude is { Count: > 0 } && myExclude.Any(_ => _ == pos.NomenklDC)) continue;
                ItemPositionsCollection.Add(new InvoiceClientRow
                {
                    IsSelected = SelectedItems.Any(_ => _.Row2d == pos.Row2d),
                    DocCode = pos.DocCode,
                    RowCode = pos.RowCode,
                    Row2d = pos.Row2d,
                    DocId = pos.DocId,
                    Nomenkl = pos.Nomenkl,
                    NomNomenkl = pos.NomNomenkl,
                    IsUsluga = pos.IsUsluga,
                    Quantity = pos.Quantity,
                    Price = pos.Price,
                    SFT_NACENKA_DILERA = pos.SFT_NACENKA_DILERA,
                    Shipped = pos.Shipped,
                    NDSPercent = pos.NDSPercent,
                    SFT_SUMMA_NDS = pos.SFT_SUMMA_NDS,
                    SDRSchet = pos.SDRSchet,
                    GruzoDeclaration = pos.GruzoDeclaration,
                    ReceiverDC = pos.ReceiverDC,
                    CentOtvetstDC = pos.CentOtvetstDC,
                    VzaimoraschetTypeDC = pos.VzaimoraschetTypeDC,
                    FormRaschetDC = pos.FormRaschetDC,
                    PayConditionDC = pos.PayConditionDC,
                    ClientDC = pos.ClientDC,
                    CurrencyDC = pos.CurrencyDC,
                    DilerDC = pos.DilerDC,
                    NomenklDC = pos.NomenklDC
                });
            }
        }

        #endregion
    }
}
