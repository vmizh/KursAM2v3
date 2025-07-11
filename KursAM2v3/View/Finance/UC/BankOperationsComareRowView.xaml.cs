﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Xpf.Editors;
using DevExpress.XtraSpreadsheet.Import.OpenXml;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.DialogUserControl;
using KursAM2.ViewModel.Finance.controls;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.References;
using ServiceStack;

namespace KursAM2.View.Finance.UC
{
    /// <summary>
    ///     Interaction logic for BankOperationsComareRowView.xaml
    /// </summary>
    public partial class BankOperationsComareRowView
    {
        private readonly WindowManager winMan = new WindowManager();
        private decimal maxSumma = decimal.MaxValue;
        public bool StartLoad { set; get; } = true;

        public BankOperationsComareRowView()
        {
            InitializeComponent();

            DataContextChanged += BankOperationsView_DataContextChanged;
            
        }

        private void BankOperationsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is AddBankOperionUC dtx)
            {
                dtx.SetBrushForPrihodRashod();
            }
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
                        "Перед выбором контрагента, необходимо выбрать валюту операции", "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var k = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions(){ Currency = dtx.Currency});
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
                        "Перед выбором контрагента, необходимо выбрать валюту операции", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var k = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions(){Currency= dtc.Currency});
                if (k == null) return;
                dtc.Payment = k;
                Payment.Text = k.Name;
                if (dtc.KontragentViewModel != null) return;
                if (dtc.BankOperationType == BankOperationType.Kontragent)
                {
                    dtc.KontragentViewModel = k;
                    Kontragent.Text = dtc.KontragentViewModel.Name;
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
                    case BankOperationType.Employee:
                        var emp = StandartDialogs.SelectEmployee();
                        break;
                    case BankOperationType.Kontragent:
                        var k = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions() { Currency = dtx.Currency });
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
                        var bb = StandartDialogs.SelectBankAccount(dtx.BankAccount.DocCode, true);
                        if (bb == null) return;
                        dtx.VVT_VAL_RASHOD = 0;
                        dtx.VVT_DOC_NUM = bb.Name + " " + bb.RashAccount;
                        break;
                    case BankOperationType.BankOut:
                        var bb2 = StandartDialogs.SelectBankStatement(dtx.BankAccount.DocCode);
                        if (bb2 == null) return;
                        dtx.VVT_VAL_PRIHOD = bb2.Summa;
                        dtx.VVT_DOC_NUM = bb2.Bank.Name + " " + bb2.Bank.RashAccount;
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
                    case BankOperationType.Employee:
                        var emp = StandartDialogs.SelectEmployee();
                        if (emp is null) return;
                        dtc.CurrentBankOperations.Employee = emp;
                        break;
                    case BankOperationType.Kontragent:
                        var k = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions() { Currency = dtc.Currency });
                        if (k == null) return;
                        if (dtc.BankOperationType == BankOperationType.Kontragent)
                        {
                            dtc.KontragentViewModel = k;
                            Kontragent.Text = dtc.KontragentViewModel.Name;
                            dtc.Payment = k;
                            Payment.Text = k.Name;
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
                        var bb = StandartDialogs.SelectBankAccount(dtc.BankAccount.DocCode, true);
                        if (bb == null) return;
                        dtc.VVT_VAL_RASHOD = 0;
                        dtc.VVT_DOC_NUM = bb.Name + " " + bb.RashAccount;
                        dtc.BankAccountIn = bb;
                        break;
                    case BankOperationType.BankOut:
                        var bb2 = StandartDialogs.SelectBankStatement(dtc.BankAccount.DocCode);
                        if (bb2 == null) return;
                        dtc.VVT_VAL_PRIHOD = bb2.Summa;
                        dtc.VVT_DOC_NUM = bb2.Bank.Name + " " + bb2.Bank.RashAccount;
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
            if ((BankOperationType)TypeKontragent.EditValue == BankOperationType.Kontragent)
                if ((decimal)e.NewValue > maxSumma)
                    Consumption.Value = maxSumma;
        }

        private void Incoming_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Incoming.Value > 0) Consumption.Value = 0;
            if ((BankOperationType)TypeKontragent.EditValue == BankOperationType.Kontragent)
                if ((decimal)e.NewValue > maxSumma)
                    Incoming.Value = maxSumma;
        }

        private void BaseEdit_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            var t = (BankOperationType)TypeKontragent.EditValue;
            if (!StartLoad)
            {
                var dtx = DataContext as AddBankOperionUC;
                if (dtx == null) return;
                dtx.SFName = null;
                dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC = null;
                dtx.Payment = null;
                SFNameButtonItem.IsEnabled = false;
                Kontragent.Text = null;
                dtx.CurrentBankOperations.Employee = null;
                if (dtx.CurrentBankOperations.AccuredId is null)
                {
                    dtx.VVT_VAL_PRIHOD = 0;
                    dtx.VVT_VAL_RASHOD = 0;
                }

                dtx.VVT_DOC_NUM = null;
            }

            switch (t)
            {
                
                case BankOperationType.CashOut:
                case BankOperationType.CashIn:
                    Incoming.IsReadOnly = true;
                    Consumption.IsReadOnly = true;
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
                case BankOperationType.Kontragent:
                    SFNameButtonItem.IsEnabled = true;
                    Incoming.IsReadOnly = false;
                    Consumption.IsReadOnly = false;
                    break;
                case BankOperationType.Employee:
                    Incoming.IsReadOnly = false;
                    Consumption.IsReadOnly = false;
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
            try
            {
                if (dtx != null && dtx.KontragentViewModel != null)
                    item1 = StandartDialogs.SelectAllInvoiceClient(dtx.KontragentViewModel.DocCode, true, true);
                else
                    item1 = StandartDialogs.SelectAllInvoiceClient(true, true, dtx?.Currency);
                if (item1 == null) return;
                var d2 = item1 as InvoiceClientViewModel;
                var d1 = item1 as InvoiceProvider;
                if (d1 == null && d2 == null) return;
                if (d2 != null)
                {
                    Incoming.IsEnabled = true;
                    Consumption.IsEnabled = false;
                    TypeKontragent.IsEnabled = false;
                    if (dtx != null)
                    {
                        if (d2.DocDate != dtx.Date)
                            if (winMan.ShowWinUIMessageBox(
                                    "Даты операции и счета не совпадают. Переустановить дату, как в счете?",
                                    "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                dtx.Date = d2.DocDate;
                        maxSumma = d2.Summa - d2.PaySumma;
                        // ReSharper disable once PossibleInvalidOperationException
                        dtx.VVT_VAL_PRIHOD = d2.Summa - d2.PaySumma;
                        dtx.VVT_DOC_NUM = d2.ToString();
                        dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC = d2.DocCode;
                        dtx.Payment = d2.Client;
                        dtx.SFName = d2.ToString();
                        Payment.Text = dtx.Payment.Name;
                        dtx.Currency = GlobalOptions.ReferencesCache.GetCurrency(d2.Entity.SF_CRS_DC) as Currency;
                        dtx.BankOperationType = BankOperationType.Kontragent;
                        dtx.KontragentViewModel = d2.Client;
                        Kontragent.Text = dtx.KontragentViewModel.Name;
                        dtx.CurrentBankOperations.SFName =
                            $"С/ф №{d2.InnerNumber}/{d2.OuterNumber} от {d2.DocDate.ToShortDateString()} на {d2.Summa:n2} {GlobalOptions.ReferencesCache.GetCurrency(d2.Entity.SF_CRS_DC) as Currency}";
                    }

                    if (dtx2 != null)
                    {
                        maxSumma = d2.Summa - d2.PaySumma;
                        if (d2.DocDate != dtx2.Date) dtx2.Date = d2.DocDate;
                        // ReSharper disable once PossibleInvalidOperationException
                        dtx2.VVT_VAL_PRIHOD = d2.Summa - d2.PaySumma;
                        dtx2.VVT_DOC_NUM = d2.ToString();
                        dtx2.VVT_SFACT_CLIENT_DC = d2.DocCode;
                        dtx2.Payment = d2.Client;
                        Payment.Text = dtx2.Payment.Name;
                        dtx2.SFName = d2.ToString();
                        dtx2.Currency = GlobalOptions.ReferencesCache.GetCurrency(d2.Entity.SF_CRS_DC) as Currency;
                        dtx2.Kontragent = d2.Client;
                        Kontragent.Text = dtx2.Kontragent.Name;
                        dtx2.SFName =
                            $"С/ф №{d2.InnerNumber}/{d2.OuterNumber} от {d2.DocDate.ToShortDateString()} на {d2.Summa:n2} " +
                            $"{GlobalOptions.ReferencesCache.GetCurrency(d2.Entity.SF_CRS_DC) as Currency}";
                    }
                }

                if (d1 != null)
                {
                    Incoming.IsEnabled = false;
                    Consumption.IsEnabled = true;
                    if (dtx != null)
                    {
                        maxSumma = d1.Summa - d1.PaySumma;
                        if (d1.DocDate != dtx.Date)
                            if (winMan.ShowWinUIMessageBox(
                                    "Даты операции и счета не совпадают. Переустановить дату, как в счете?",
                                    "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                dtx.Date = d1.DocDate;
                        dtx.VVT_VAL_RASHOD = d1.Summa - d1.PaySumma;
                        dtx.VVT_DOC_NUM = d1.ToString();
                        dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC = d1.DocCode;
                        dtx.SFName = d1.ToString();
                        dtx.CurrentBankOperations.VVT_DOC_NUM = d1.ToString();
                        dtx.BankOperationType = BankOperationType.Kontragent;
                        dtx.KontragentViewModel = d1.Kontragent;
                        Kontragent.Text = dtx.KontragentViewModel.Name;
                        dtx.Payment = d1.Kontragent;
                        Payment.Text = dtx.Payment.Name;
                    }

                    if (dtx2 != null)
                    {
                        maxSumma = d1.Summa - d1.PaySumma;
                        if (d1.DocDate != dtx2.Date) dtx2.Date = d1.DocDate;
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
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка выбора счета фактуры");
            }
        }

        private void PaymentCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var dtx = DataContext as AddBankOperionUC;
            var dtx2 = DataContext as BankOperationsViewModel;
            if (dtx == null && dtx2 == null) return;
            TypeKontragent.IsEnabled = true;

            if (dtx != null)
            {
                if (dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC == null &&
                    dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC == null) return;

                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC != null)
                        ctx.Database.ExecuteSqlCommand(
                            $"EXEC dbo.GenerateSFProviderCash {Convert.ToString(dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC.Value, CultureInfo.InvariantCulture).Replace(",", ".")}");

                    if (dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC != null)
                        ctx.Database.ExecuteSqlCommand(
                            $"EXEC dbo.GenerateSFClientCash {Convert.ToString(dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC.Value, CultureInfo.InvariantCulture).Replace(",", ".")}");
                }

                dtx.CurrentBankOperations.VVT_SFACT_CLIENT_DC = null;
                dtx.CurrentBankOperations.VVT_SFACT_POSTAV_DC = null;
                dtx.SFName = null;
                dtx.Payment = null;
                dtx.CurrentBankOperations.Kontragent = null;
                

                dtx.VVT_VAL_RASHOD = 0;
                dtx.VVT_VAL_PRIHOD = 0;
                dtx.VVT_DOC_NUM = null;

            }

            if (dtx2 != null)
            {
                if (dtx2.VVT_SFACT_CLIENT_DC == null && dtx2.VVT_SFACT_POSTAV_DC == null) return;
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (dtx2.VVT_SFACT_POSTAV_DC != null)
                        ctx.Database.ExecuteSqlCommand(
                            $"EXEC dbo.GenerateSFProviderCash {Convert.ToString(dtx2.VVT_SFACT_POSTAV_DC.Value, CultureInfo.InvariantCulture).Replace(",", ".")}");

                    if (dtx2.VVT_SFACT_CLIENT_DC != null)
                        ctx.Database.ExecuteSqlCommand(
                            $"EXEC dbo.GenerateSFClientCash {Convert.ToString(dtx2.VVT_SFACT_CLIENT_DC.Value, CultureInfo.InvariantCulture).Replace(",", ".")}");
                }

                dtx2.VVT_SFACT_CLIENT_DC = null;
                dtx2.VVT_SFACT_POSTAV_DC = null;
                dtx2.SFName = null;
                dtx2.Payment = null;
                dtx2.VVT_VAL_RASHOD = 0;
                dtx2.VVT_VAL_PRIHOD = 0;
                dtx2.Kontragent.Name = null;
                dtx2.VVT_DOC_NUM = null;


            }

            Kontragent.Text = null;
            Consumption.IsEnabled = true;
            Incoming.IsEnabled = true;
        }

        private void AccuredEdit_OnDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            var dtx = DataContext as AddBankOperionUC;
            if (dtx == null)
                return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var row = ctx.AccuredAmountOfSupplierRow.FirstOrDefault(
                    _ => _.Id == dtx.CurrentBankOperations.AccuredId);
                if (row == null) return;
                DocumentsOpenManager.Open(
                    DocumentType.AccruedAmountOfSupplier, 0, row.DocId, this);
            }
        }
    }
}
