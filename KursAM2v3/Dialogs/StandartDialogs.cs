using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Finance.UC;
using KursAM2.View.Logistiks.UC;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Finance.controls;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Management.Projects;
using KursAM2.ViewModel.Reference;
using KursAM2.ViewModel.Reference.Dialogs;
using KursAM2.ViewModel.StockHolder;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Documents.Cash;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.NomenklManagement;
using KursDomain.Documents.Periods;
using KursDomain.Documents.StockHolder;
using KursDomain.Documents.Systems;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.Repository;

namespace KursAM2.Dialogs
{
    public static class StandartDialogs
    {
        /// <summary>
        ///     Выбор контрагента
        /// </summary>
        /// <param name="crs">
        ///     Currency.Если указан, то выбираются только контрагенты с соответствующей валютой учета, если нет, то
        ///     все
        /// </param>
        /// <returns></returns>
        // ReSharper disable once InvalidXmlDocComment
        public static Kontragent SelectKontragent(Currency crs = null, bool? isBalans = null)
        {
            //MainReferences.CheckUpdateKontragentAndLoad();
            var ctx = new KontragentSelectDialog(crs, false, isBalans);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentKontragent;
        }

        //public static SelectStockHolder()
        //{

        //}

        public static List<CashOrder> SelectCashOrders(BankAccount bankAcc, SelectCashOrdersDelegate selectMethod)
        {
            var ret = new List<CashOrder>();
            var ctx = new CashOrdersForBankSelectDialog(bankAcc, selectMethod);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            foreach (var d in ctx.SelectedDocuments)
                ret.Add(d);
            return ret;
        }

        public static List<BankAccount> SelectBankAccounts()
        {
            var ret = new List<BankAccount>();
            var ctx = new BankAccountSelectDialog();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            //foreach (var d in ctx.SelectedDocuments)
            //    ret.Add(d);
            return ret;
        }

        public static List<Nomenkl> SelectNomenkls(Currency crs = null, bool isNotUsluga = false)
        {
            //MainReferences.UpdateNomenkl();
            var ctx = new AddNomenklViewModel(crs, isNotUsluga)
            {
                WindowName = "Выбор номенклатур"
            };
            var dlg = new SelectDialogView2 {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.SelectedNomenkl.ToList();
        }

        public static List<WarehouseOrderOutRowSelect> SelectNomenklsFromRashodOrder(Warehouse store)
        {
            //MainReferences.UpdateNomenkl();
            var ctx = new AddNomenklFromRashOrderViewModel(store)
            {
                WindowName = "Выбор номенклатур"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult
                ? null
                : new List<WarehouseOrderOutRowSelect>(ctx.ItemsCollection.Where(_ => _.IsSelected));
        }

        public static List<EXT_USERSViewModel> SelectUsers()
        {
            var ctx = new UserUCViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.ListSelectedUsers;
        }

        public static Employee SelectEmployee()
        {
            var ctx = new EmployeesUCViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static Region SelectRegion()
        {
            var ctx = new RegionUCViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static List<PERIOD_GROUPSViewModel> SelectPeriodGroups()
        {
            var ctx = new PeriodGroupsUCViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.ListSelectedUsers;
        }

        public static BankOperationsViewModel AddNewBankOperation(decimal docCode, BankOperationsViewModel row,
            BankAccount bankAcc)
        {
            var ctx = new AddBankOperionUC(docCode, row, bankAcc, true);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentBankOperations;
        }

        public static BankOperationsViewModel OpenBankOperation(decimal docCode, BankOperationsViewModel row,
            BankAccount bankAcc)
        {
            var ctx = new AddBankOperionUC(docCode, row, bankAcc, false);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            ctx.SetBrushForPrihodRashod();
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentBankOperations;
        }

        public static BankPaymentRow SelectBankOperationForClientInvoice(decimal kontrDC)
        {
            var ctx = new AddPaymentBankClientInvoice(kontrDC)
            {
                WindowName =
                    $"Выбор банковского платежа для {((IName) GlobalOptions.ReferencesCache.GetKontragent(kontrDC)).Name}"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static CashPaymentRow SelectCashOperationForClientInvoice(decimal kontrDC)
        {
            var ctx = new AddPaymentCashClientInvoice(kontrDC)
            {
                WindowName =
                    $"Выбор кассового прихода для {((IName) GlobalOptions.ReferencesCache.GetKontragent(kontrDC)).Name}"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static VZPaymentRow SelectVZOperationForClientInvoice(decimal kontrDC)
        {
            var ctx = new AddPaymentVZClientInvoice(kontrDC)
            {
                WindowName =
                    $"Выбор проводки акта вазимозачета для {((IName) GlobalOptions.ReferencesCache.GetKontragent(kontrDC)).Name}"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static BankPaymentRow SelectBankOperationForProviderInvoice(decimal kontrDC)
        {
            var ctx = new AddPaymentBankProviderInvoice(kontrDC)
            {
                WindowName =
                    $"Выбор банковского платежа для {((IName) GlobalOptions.ReferencesCache.GetKontragent(kontrDC)).Name}"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static CashPaymentRow SelectCashOperationForProviderInvoice(decimal kontrDC)
        {
            var ctx = new AddPaymentCashProviderInvoice(kontrDC)
            {
                WindowName =
                    $"Выбор кассового расхода для {((IName) GlobalOptions.ReferencesCache.GetKontragent(kontrDC)).Name}"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static VZPaymentRow SelectVZOperationForProviderInvoice(decimal kontrDC)
        {
            var ctx = new AddPaymentVZClientInvoice(kontrDC)
            {
                WindowName =
                    $"Выбор проводки акта вазимозачета для {((IName) GlobalOptions.ReferencesCache.GetKontragent(kontrDC)).Name}"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static Project SelectProject()
        {
            var ctx = new ProjectSelectReferenceWindowViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentProject;
        }

        public static BankAccount SelectBankAccount(Currency crs)
        {
            var ctx = new BankAccountSelectedDialog(crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentChildItem;
        }

        /// <summary>
        ///     Новый иерархический выбор счета с учетом валюты
        /// </summary>
        /// <returns></returns>
        public static BankAccountSelect SelectBankAccount2()
        {
            var ctx = new BankAccountSelectedDialog2();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentChildItem;
        }

        public static BankAccount SelectBankAccount(decimal dcOut)
        {
            var ctx = new BankAccountSelectedDialog(dcOut);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentChildItem;
        }

        public static BankOperationForSelectDialog SelectBankStatement(decimal dcOut)
        {
            var ctx = new BankAccountOperationSelectedDialog(dcOut);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static StockHolderViewModel SelectStockHolder(IDialogService service)
        {
            //DefaultTableSelectUC
            var ctx = new SelectStockHolderDialogViewModel();
            if (service.ShowDialog(MessageButton.OKCancel, "Выбрать акционера", ctx) == MessageResult.OK
                || ctx.DialogResult == MessageResult.OK)
                return ctx.CurrentItem;
            return null;
        }

        public static CashBox SelectCash(List<CashBox> exclude)
        {
            var ctx = new CashSelectedDialog(exclude);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        /// <summary>
        ///     Выбор расходных ордеров для приходного ордера
        ///     при перемещенни денег из кассы в кассу
        /// </summary>
        /// <returns></returns>
        public static CashOut SelectCashRashOrderForPrihod(CashInViewModel ord)
        {
            var ctx = new CashRashOrderSelectDialog(ord);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        /// <summary>
        ///     Выбор расходных ордеров для приходного ордера
        ///     при перемещенни денег из кассы в кассу
        /// </summary>
        /// <returns></returns>
        public static CashOut SelectCashOrders(CashInViewModel ord)
        {
            var ctx = new CashRashOrderSelectDialog(ord);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static List<CashStartRemains> SetCashRemains(CashBox cash)
        {
            var ret = new List<CashStartRemains>();
            var ctx = new CashSetRemainsDialog(cash);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            foreach (var d in ctx.CashRemainsCollection) ret.Add(d);
            return !ctx.DialogResult ? null : ret;
        }

        public static InvoiceProvider SelectInvoiceProvider(CashOutWindowViewModel cashOut, bool isUsePayment,
            bool isUseAccepted, bool isOnlyLastYear = false)
        {
            if (cashOut == null) return null;
            if (cashOut.Document.KONTRAGENT_DC != null)
            {
                var ctx = new InvoiceProviderSearchDialog((decimal) cashOut.Document.KONTRAGENT_DC, isUsePayment,
                    isUseAccepted, isOnlyLastYear);
                var dlg = new SelectDialogView {DataContext = ctx};
                ctx.Form = dlg;
                dlg.ShowDialog();
                return !ctx.DialogResult ? null : ctx.CurrentItem;
            }
            else
            {
                var ctx = new InvoiceProviderSearchDialog(isUsePayment, isUseAccepted, isOnlyLastYear);
                var dlg = new SelectDialogView {DataContext = ctx};
                ctx.Form = dlg;
                dlg.ShowDialog();
                return !ctx.DialogResult ? null : ctx.CurrentItem;
            }
        }

        public static InvoiceProvider SelectInvoiceProvider(bool isUsePayment, bool isUseAccepted,
            bool isLastYearOnly = false, Currency crs = null)
        {
            var ctx = new InvoiceProviderSearchDialog(isUsePayment, isUseAccepted, isLastYearOnly, crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }
        public static InvoiceProvider SelectInvoiceProvider(bool isUsePayment, bool isUseAccepted,
            List<decimal> excludeSfDCs, bool isLastYearOnly = false, Currency crs = null)
        {
            var ctx = new InvoiceProviderSearchDialog(isUsePayment, isUseAccepted, excludeSfDCs, isLastYearOnly, crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }


        public static InvoiceProvider SelectInvoiceProvider(decimal kontrDC, bool isUsePayment, bool isUseAccepted,
            bool isLastYearOnly = false)
        {
            var ctx = new InvoiceProviderSearchDialog(kontrDC, isUsePayment, isUseAccepted, isLastYearOnly);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }
        public static InvoiceProvider SelectInvoiceProvider(decimal kontrDC, bool isUsePayment, bool isUseAccepted,
            List<decimal> excludeSfDcs,  bool isLastYearOnly = false)
        {
            var ctx = new InvoiceProviderSearchDialog(kontrDC, isUsePayment, isUseAccepted, isLastYearOnly);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClientViewModel SelectInvoiceClient(CashInWindowViewModel cashIn, bool isUsePayment,
            bool isUseAccepted)
        {
            if (cashIn == null) return null;
            var ctx = new InvoiceClientSearchDialog(cashIn.Document.KONTRAGENT_DC ?? 0, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClientViewModel SelectInvoiceClient(Waybill waybill)
        {
            if (waybill == null) return null;
            var ctx = new InvoiceClientSearchDialog(waybill);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClientViewModel SelectInvoiceClient(bool isUsePayment, bool isUseAccepted,
            Currency crs = null)
        {
            var ctx = new InvoiceClientSearchDialog(isUsePayment, isUseAccepted,  crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }
        public static InvoiceClientViewModel SelectInvoiceClient(bool isUsePayment, bool isUseAccepted,
            List<decimal> excludeSfDCs,
            Currency crs = null)
        {
            var ctx = new InvoiceClientSearchDialog(isUsePayment, isUseAccepted, excludeSfDCs,  crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClientViewModel SelectInvoiceClient(decimal kontrDC, bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceClientSearchDialog(kontrDC, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClientViewModel SelectInvoiceClient(decimal kontrDC, bool isUsePayment, bool isUseAccepted, 
            List<decimal> excludeSfDCs)
        {
            var ctx = new InvoiceClientSearchDialog(kontrDC, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static RSViewModelBase SelectAllInvoiceClient(decimal kontrDc, bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceAllSearchDialog(kontrDc, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (ctx.DialogResult)
                using (var dbctx = GlobalOptions.GetEntities())
                {
                    if (ctx.CurrentClientItem != null)
                    {
                        var doc = new InvoiceClientViewModel(dbctx.SD_84.First(_ =>
                            _.DOC_CODE == ctx.CurrentClientItem.DocCode));
                        return doc;
                    }

                    if (ctx.CurrentProviderItem != null)
                    {
                        var d = dbctx.SD_26
                            .Include(_ => _.TD_26)
                            .Include(_ => _.ProviderInvoicePay)
                            .Include(_ => _.ProviderInvoicePay.Select(x => x.TD_101))
                            .Include(_ => _.ProviderInvoicePay.Select(x => x.SD_34))
                            .First(_ => _.DOC_CODE == ctx.CurrentProviderItem.DocCode);
                        var doc = new InvoiceProvider(d, new UnitOfWork<ALFAMEDIAEntities>(dbctx));
                        return doc;
                    }

                    return null;
                }

            return null;
        }

        public static RSViewModelBase SelectAllInvoiceClient(bool isUsePayment, bool isUseAccepted, Currency crs = null)
        {
            var ctx = new InvoiceAllSearchDialog(isUsePayment, isUseAccepted, crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (ctx.DialogResult)
                using (var dbctx = GlobalOptions.GetEntities())
                {
                    if (ctx.CurrentClientItem != null)
                    {
                        var doc = new InvoiceClientViewModel(dbctx.SD_84.First(_ =>
                            _.DOC_CODE == ctx.CurrentClientItem.DocCode));
                        return doc;
                    }

                    if (ctx.CurrentProviderItem != null)
                    {
                        var d = dbctx.SD_26
                            .Include(_ => _.TD_26)
                            .Include(_ => _.ProviderInvoicePay)
                            .Include(_ => _.ProviderInvoicePay.Select(x => x.TD_101))
                            .Include(_ => _.ProviderInvoicePay.Select(x => x.SD_34))
                            .First(_ => _.DOC_CODE == ctx.CurrentProviderItem.DocCode);
                        var doc = new InvoiceProvider(d);
                        return doc;
                    }

                    return null;
                }

            return null;
        }

        public static List<Nomenkl> SelectNomenklsDialog()
        {
            //MainReferences.UpdateNomenkl();
            var ctx = new NomenklSelectedDialogViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return new List<Nomenkl>(ctx.SelectedNomenkls);
        }

        public static Warehouse SelectWarehouseDialog()
        {
            var ctx = new WarehouseSelectDialogViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.CurrentWarehouse;
        }

        public static Currency SelectCurrency(IEnumerable<Currency> withoutCrs = null)
        {
            var ctx = new CurrencySelectDialogViewModel(withoutCrs);
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.CurrentItem;
        }

        public static Bank SelectBank()
        {
            var ctx = new BankSelectDialogViewModel();
            var dlg = new SelectDialogView2 {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.CurrentItem;
        }

        #region Temp class для иерархического выбора банковских счетов

        public class BankSelect
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Global
            public ObservableCollection<BankAccountSelect> AccountList = new ObservableCollection<BankAccountSelect>();
            public decimal DocCode { set; get; }
            public string Name { set; get; }
        }

        public class BankAccountSelect
        {
            public decimal DocCode { set; get; }
            public string Name { set; get; }
            public Currency Currency { set; get; }
            public string Account { set; get; }
        }

        #endregion
    }
}
