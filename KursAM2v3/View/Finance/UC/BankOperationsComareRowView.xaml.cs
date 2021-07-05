using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.Bank;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.Invoices;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.Editors;
using KursAM2.Dialogs;
using KursAM2.ViewModel.Finance.controls;

namespace KursAM2.View.Finance.UC
{
    /// <summary>
    ///     Interaction logic for BankOperationsComareRowView.xaml
    /// </summary>
    public partial class BankOperationsComareRowView
    {
        private readonly WindowManager winMan = new WindowManager();

        public BankOperationsComareRowView()
        {
            InitializeComponent();
            DataContextChanged += BankOperationsView_DataContextChanged;
        }

        private void BankOperationsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext is AddBankOperionUC dtx)
                dtx.SetBrushForPrihodRashod();
        }

        private void Payment_OnDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is BankOperationsViewModel dtx)
            {
                if (dtx.BankOperationType == BankOperationType.NotChoice)
                {
                    winMan.ShowWinUIMessageBox(
                        "Перед выбором контрагента, необходимо выбрать тип контрагента", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                if (dtx.Currency == null)
                {
                    winMan.ShowWinUIMessageBox(
                        "Перед выбором контрагента, необходимо выбрать валюту опреции", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                var k = StandartDialogs.SelectKontragent(dtx.Currency);
                if (k == null) return;
                dtx.Payment = k;
                Payment.Text = k.Name;
                if (dtx.Kontragent == null && dtx.BankOperationType == BankOperationType.Kontragent)
                {
                    dtx.Kontragent = k;
                    Kontragent.Text = dtx.Kontragent.Name;
                }
            }
            else
            {
                var dtc = DataContext as AddBankOperionUC;
                if (dtc == null) return;
                if (dtc.BankOperationType == BankOperationType.NotChoice)
                {
                    winMan.ShowWinUIMessageBox(
                        "Перед выбором контрагента, необходимо выбрать тип контрагента", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                if (dtc.Currency == null)
                {
                    winMan.ShowWinUIMessageBox("Предупреждение",
                        "Перед выбором контрагента, необходимо выбрать валюту опреции", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                var k = StandartDialogs.SelectKontragent(dtc.Currency);
                if (k == null) return;
                dtc.Payment = k;
                Payment.Text = k.Name;
                if (dtc.Kontragent != null) return;
                if (dtc.BankOperationType == BankOperationType.Kontragent)
                {
                    dtc.Kontragent = k;
                    Kontragent.Text = dtc.Kontragent.Name;
                }
            }
        }

        private List<CashOrder> SelectCashIn(decimal bankdc, DateTime start, DateTime end)
        {
            var ret = new List<CashOrder>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_33
                        .Include(_ => _.SD_34)
                        .Include(_ => _.TD_101).Where(_ => _.BANK_RASCH_SCHET_DC == bankdc
                                                           && _.DATE_ORD >= start && _.DATE_ORD <= end
                                                           && _.TD_101.All(t =>
                                                               t.VVT_KASS_PRIH_ORDER_DC != _.DOC_CODE));
                    foreach (var d in data)
                        ret.Add(new CashOrder(d)
                        );
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            return ret;
        }

        private List<CashOrder> SelectCashOut(decimal bankdc, DateTime start, DateTime end)
        {
            var ret = new List<CashOrder>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_34
                        .Include(_ => _.TD_101).Where(_ => _.BANK_RASCH_SCHET_DC == bankdc
                                                           && _.DATE_ORD >= start && _.DATE_ORD <= end
                                                           && _.TD_101.All(t =>
                                                               t.VVT_RASH_KASS_ORDER_DC != _.DOC_CODE));
                    foreach (var d in data)
                        ret.Add(new CashOrder(d)
                        );
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            return ret;
        }

        private void Kontragent_OnDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is BankOperationsViewModel dtx)
            {
                if (dtx.BankOperationType == BankOperationType.NotChoice)
                {
                    winMan.ShowWinUIMessageBox(
                        "Перед выбором контрагента, необходимо выбрать тип контрагента", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                if (dtx.Currency == null)
                {
                    winMan.ShowWinUIMessageBox(
                        "Перед выбором контрагента, необходимо выбрать валюту опреции", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                switch (dtx.BankOperationType)
                {
                    case BankOperationType.Kontragent:
                        var k = StandartDialogs.SelectKontragent(dtx.Currency);
                        if (k == null) return;
                        if (dtx.Payment == null)
                        {
                            dtx.Payment = k;
                            Payment.Text = k.Name;
                        }
                        if (dtx.BankOperationType == BankOperationType.Kontragent)
                        {
                            dtx.Kontragent = k;
                            Kontragent.Text = dtx.Kontragent.Name;
                        }
                        break;
                    case BankOperationType.CashOut:
                        var c = StandartDialogs.SelectCashOrders(dtx.BankAccount, SelectCashOut);
                        if (c == null || c.Count == 0) return;
                        dtx.CashOut = c.First();
                        dtx.VVT_VAL_PRIHOD = c.First().SummaOrder;
                        dtx.VVT_DOC_NUM = c.First().ToString();
                        break;
                    case BankOperationType.CashIn:
                        var c1 = StandartDialogs.SelectCashOrders(dtx.BankAccount, SelectCashIn);
                        if (c1 == null || c1.Count == 0) return;
                        dtx.CashIn = c1.First();
                        dtx.VVT_VAL_RASHOD = c1.First().SummaOrder;
                        dtx.VVT_DOC_NUM = c1.First().ToString();
                        break;
                    case BankOperationType.BankIn:
                        var bb = StandartDialogs.SelectBankAccount(dtx.BankAccount.DocCode);
                        if (bb == null) return;
                        dtx.VVT_VAL_RASHOD = 0;
                        dtx.VVT_DOC_NUM = bb.BankName + " " + bb.Account;
                        break;
                    case BankOperationType.BankOut:
                        var bb2 = StandartDialogs.SelectBankStatement(dtx.BankAccount.DocCode);
                        if (bb2 == null) return;
                        dtx.VVT_VAL_PRIHOD = bb2.Summa;
                        dtx.VVT_DOC_NUM = bb2.Bank.BankName + " " + bb2.Bank.Account;
                        dtx.Currency = bb2.Currency;
                        dtx.SHPZ = bb2.SHPZ;
                        dtx.BankAccountOut = bb2.Bank;
                        dtx.BankFromTransactionCode = bb2.Code;
                        break;
                }
                Kontragent.Text = dtx.KontragentName;
                // ReSharper disable once NotResolvedInText
                dtx.RaisePropertyChanged("KontragentName");
            }
            else
            {
                if (!(DataContext is AddBankOperionUC dtc)) return;
                if (dtc.BankOperationType == BankOperationType.NotChoice)
                {
                    winMan.ShowWinUIMessageBox(
                        "Перед выбором контрагента, необходимо выбрать тип контрагента", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                if (dtc.Currency == null)
                {
                    winMan.ShowWinUIMessageBox("Предупреждение",
                        "Перед выбором контрагента, необходимо выбрать валюту опреции", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                switch (dtc.BankOperationType)
                {
                    case BankOperationType.Kontragent:
                        var k = StandartDialogs.SelectKontragent(dtc.Currency);
                        if (k == null) return;
                        if (dtc.Payment == null)
                        {
                            dtc.Payment = k;
                            Payment.Text = k.Name;
                        }
                        if (dtc.Kontragent != null) return;
                        if (dtc.BankOperationType == BankOperationType.Kontragent)
                        {
                            dtc.Kontragent = k;
                            Kontragent.Text = dtc.Kontragent.Name;
                        }
                        break;
                    case BankOperationType.CashOut:
                        var c = StandartDialogs.SelectCashOrders(dtc.BankAccount, SelectCashOut);
                        if (c == null || c.Count == 0) return;
                        dtc.CurrentBankOperations.CashOut = c.First();
                        dtc.VVT_VAL_PRIHOD = c.First().SummaOrder;
                        dtc.VVT_DOC_NUM = c.First().ToString();
                        break;
                    case BankOperationType.CashIn:
                        var c1 = StandartDialogs.SelectCashOrders(dtc.BankAccount, SelectCashIn);
                        if (c1 == null || c1.Count == 0) return;
                        dtc.CurrentBankOperations.CashIn = c1.First();
                        dtc.VVT_VAL_RASHOD = c1.First().SummaOrder;
                        dtc.CurrentBankOperations.VVT_VAL_RASHOD = c1.First().SummaOrder;
                        dtc.VVT_DOC_NUM = c1.First().ToString();
                        dtc.CurrentBankOperations.VVT_DOC_NUM = c1.First().ToString();
                        break;
                    case BankOperationType.BankIn:
                        var bb = StandartDialogs.SelectBankAccount(dtc.BankAccount.DocCode);
                        if (bb == null) return;
                        dtc.VVT_VAL_RASHOD = 0;
                        dtc.VVT_DOC_NUM = bb.BankName + " " + bb.Account;
                        dtc.BankAccountIn = bb;
                        break;
                    case BankOperationType.BankOut:
                        var bb2 = StandartDialogs.SelectBankStatement(dtc.BankAccount.DocCode);
                        if (bb2 == null) return;
                        dtc.VVT_VAL_PRIHOD = bb2.Summa;
                        dtc.VVT_DOC_NUM = bb2.Bank.BankName + " " + bb2.Bank.Account;
                        dtc.Currency = bb2.Currency;
                        dtc.SHPZ = bb2.SHPZ;
                        dtc.BankAccountOut = bb2.Bank;
                        dtc.CurrentBankOperations.BankFromTransactionCode = bb2.Code;
                        break;
                }
                Kontragent.Text = dtc.KontragentName;
                // ReSharper disable once NotResolvedInText
                dtc.RaisePropertyChanged("KontragentName");
            }
        }

        private void Consumption_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Consumption.Value > 0) Incoming.Value = 0;
        }

        private void Incoming_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Incoming.Value > 0) Consumption.Value = 0;
        }

        private void BaseEdit_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            Kontragent.Text = null;
            var dtx = DataContext as AddBankOperionUC;
            if (dtx == null) return;
            var t = (BankOperationType) TypeKontragent.EditValue;
            switch (t)
            {
                case BankOperationType.CashOut:
                    Incoming.IsReadOnly = false;
                    Consumption.IsReadOnly = true;
                    break;
                case BankOperationType.CashIn:
                    Incoming.IsReadOnly = true;
                    Consumption.IsReadOnly = false;
                    break;
                case BankOperationType.BankIn:
                    Incoming.IsReadOnly = true;
                    Consumption.IsReadOnly = false;
                    break;
                case BankOperationType.BankOut:
                case BankOperationType.NotChoice:
                    Incoming.IsReadOnly = true;
                    Consumption.IsReadOnly = true;
                    break;
                default:
                    Incoming.IsReadOnly = false;
                    Consumption.IsReadOnly = false;
                    break;
            }
        }

        private void SFName_OnDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            var dtx = DataContext as AddBankOperionUC;
            var dtx2 = DataContext as BankOperationsViewModel;
            if (dtx == null && dtx2 == null) return;
            RSViewModelBase item1;
            if(dtx != null && dtx.Kontragent != null)
                item1 =  StandartDialogs.SelectAllInvoiceClient(dtx.Kontragent.DocCode,true, true);
            else
            {
                item1 = StandartDialogs.SelectAllInvoiceClient(true,true);
            }
            if (item1 == null) return;
            var d2 = item1 as InvoiceClient;
            var d1 = item1 as InvoiceProvider;
            if (d1 == null && d2 == null) return;
            if (d2 != null)
            {
                if (dtx != null)
                {
                    if (d2.DocDate != dtx.Date)
                    {
                        if (winMan.ShowWinUIMessageBox(
                            "Даты операции и счета не совпадают. Переустановить дату, как в счете?",
                            "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            dtx.Date = d2.DocDate;
                        }
                    }

                    // ReSharper disable once PossibleInvalidOperationException
                    dtx.VVT_VAL_PRIHOD = (decimal) d2.Summa - d2.PaySumma;
                    dtx.VVT_DOC_NUM = d2.ToString();
                    dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC = d2.DocCode;
                    dtx.Payment = d2.Client;
                    dtx.SFName = d2.ToString();
                    Payment.Text = dtx.Payment.Name;
                    dtx.Currency = MainReferences.Currencies[d2.Entity.SF_CRS_DC];
                    dtx.BankOperationType = BankOperationType.Kontragent;
                    dtx.Kontragent = d2.Client;
                    Kontragent.Text = dtx.Kontragent.Name;
                    dtx.CurrentBankOperations.SFName =
                        $"С/ф №{d2.InnerNumber}/{d2.OuterNumber} от {d2.DocDate.ToShortDateString()} на {d2.Summa} {MainReferences.Currencies[d2.Entity.SF_CRS_DC]}";

                }

                if (dtx2 != null)
                {
                    if (d2.DocDate != dtx2.Date)
                    {
                        dtx2.Date = d2.DocDate;
                    }
                    // ReSharper disable once PossibleInvalidOperationException
                    dtx2.VVT_VAL_PRIHOD =  (decimal)d2.Summa - d2.PaySumma;
                    dtx2.VVT_DOC_NUM = d2.ToString();
                    dtx2.VVT_SFACT_CLIENT_DC = d2.DocCode;
                    dtx2.Payment = d2.Client;
                    Payment.Text = dtx2.Payment.Name;
                    dtx2.SFName = d2.ToString();
                    dtx2.Currency = MainReferences.Currencies[d2.Entity.SF_CRS_DC];
                    dtx2.Kontragent = d2.Client;
                    Kontragent.Text = dtx2.Kontragent.Name;
                    dtx2.SFName = $"С/ф №{d2.InnerNumber}/{d2.OuterNumber} от {d2.DocDate.ToShortDateString()} на {d2.Summa} " +
                                  $"{MainReferences.Currencies[d2.Entity.SF_CRS_DC]}";
                }
            }
            if (d1 != null)
            {
                if (dtx != null)
                {
                    if (d1.SF_POSTAV_DATE != dtx.Date)
                    {
                        if (winMan.ShowWinUIMessageBox(
                            "Даты операции и счета не совпадают. Переустановить дату, как в счете?",
                            "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            dtx.Date = d1.SF_POSTAV_DATE;
                        }
                    }
                    dtx.VVT_VAL_RASHOD = d1.Summa - d1.PaySumma;
                    dtx.VVT_DOC_NUM = d1.ToString();
                    dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC = d1.DocCode;
                    dtx.SFName = d1.ToString();
                    dtx.CurrentBankOperations.VVT_DOC_NUM = d1.ToString();
                    dtx.BankOperationType = BankOperationType.Kontragent;
                    dtx.Kontragent = d1.Kontragent;
                    Kontragent.Text = dtx.Kontragent.Name;
                    dtx.Payment = d1.Kontragent;
                    Payment.Text = dtx.Payment.Name;
                }
                if (dtx2 != null)
                {
                    if (d1.SF_POSTAV_DATE != dtx2.Date)
                    {
                        dtx2.Date = d1.SF_POSTAV_DATE;
                    }
                    dtx2.VVT_VAL_RASHOD = d1.Summa - d1.PaySumma;
                    dtx2.VVT_DOC_NUM = d1.ToString();
                    dtx2.VVT_SFACT_POSTAV_DC = d1.DocCode;
                    dtx2.SFName = d1.ToString();
                    dtx2.Payment = GlobalOptions.SystemProfile.OwnerKontragent;
                    dtx2.BankOperationType = BankOperationType.Kontragent;
                    dtx2.Kontragent = d1.Kontragent;
                    Kontragent.Text = dtx2.Kontragent.Name;
                    Payment.Text = dtx2.Payment.Name;
                }
            }
        }

        private void PaymentCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var dtx = DataContext as AddBankOperionUC;
            var dtx2 = DataContext as BankOperationsViewModel;
            if (dtx == null && dtx2 == null) return;
            if (dtx != null)
            {
                dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC = null;
                dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC = null;
                dtx.SFName = null;
                dtx.Payment = null;
            }
            if (dtx2 != null)
            {
                dtx2.VVT_SFACT_CLIENT_DC = null;
                dtx2.VVT_SFACT_POSTAV_DC = null;
                dtx2.SFName = null;
                dtx2.Payment = null;
            }

        }
    }
}