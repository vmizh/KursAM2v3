using System.Collections.Generic;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Finance.UC;
using KursAM2.View.Logistiks.UC;
using KursAM2.ViewModel.Finance.controls;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Management.Projects;
using KursAM2.ViewModel.Reference;
using KursAM2.ViewModel.Reference.Dialogs;

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
        public static Kontragent SelectKontragent(Currency crs = null)
        {
            //MainReferences.CheckUpdateKontragentAndLoad();
            var ctx = new KontragentSelectDialog(crs);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentKontragent;
        }

        
        public static List<CashOrder> SelectCashOrders(BankAccount bankAcc, SelectCashOrdersDelegate selectMethod)
        {
            var ret = new List<CashOrder>();
            var ctx = new CashOrdersForBankSelectDialog(bankAcc, selectMethod);
            var dlg = new SelectDialogView { DataContext = ctx};
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
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            //foreach (var d in ctx.SelectedDocuments)
            //    ret.Add(d);
            return ret;
        }


        public static List<Nomenkl> SelectNomenkls(Currency crs = null, bool isUsluga = false)
        {
            //MainReferences.UpdateNomenkl();
            var ctx = new AddNomenklViewModel(crs, isUsluga)
            {
                WindowName = "Выбор номенклатур"
            };
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.ListNomenklCollection;
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

        public static BankOperationsViewModel AddNewBankOperation(decimal docCode, BankOperationsViewModel row, BankAccount bankAcc)
        {
            var ctx = new AddBankOperionUC(docCode, row, bankAcc);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentBankOperations;
        }

        public static Project SelectProject()
        {
            var ctx = new ProjectSelectReferenceWindowViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentProject;
        }

        public static BankAccount SelectBankAccount()
        {
            var ctx = new BankAccountSelectedDialog();
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }
        public static BankAccount SelectBankAccount(decimal dcOut)
        {
            var ctx = new BankAccountSelectedDialog(dcOut);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static BankOperationForSelectDialog SelectBankStatement(decimal dcOut)
        {
            var ctx = new BankAccountOperationSelectedDialog(dcOut);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static Cash SelectCash(List<Cash> exclude)
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
        public static CashOut SelectCashRashOrderForPrihod(CashIn ord)
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
        public static CashOut SelectCashOrders(CashIn ord)
        {
            var ctx = new CashRashOrderSelectDialog(ord);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static List<TD_22ViewModel> SetCashRemains(Cash cash)
        {
            var ret = new List<TD_22ViewModel>();
            var ctx = new CashSetRemainsDialog(cash);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            foreach (var d in ctx.CashRemainsCollection) ret.Add(d);
            return !ctx.DialogResult ? null : ret;
        }

        public static InvoiceProvider SelectInvoiceProvider(CashOutWindowViewModel cashOut, bool isUsePayment, bool isUseAccepted)
        {
            if (cashOut == null) return null;
            if (cashOut.Document.KONTRAGENT_DC != null)
            {
                var ctx = new InvoiceProviderSearchDialog((decimal) cashOut.Document.KONTRAGENT_DC,isUsePayment, isUseAccepted);
                var dlg = new SelectDialogView {DataContext = ctx};
                ctx.Form = dlg;
                dlg.ShowDialog();
                return !ctx.DialogResult ? null : ctx.CurrentItem;
            }
            else
            {
                var ctx = new InvoiceProviderSearchDialog(isUsePayment, isUseAccepted);
                var dlg = new SelectDialogView { DataContext = ctx };
                ctx.Form = dlg;
                dlg.ShowDialog();
                return !ctx.DialogResult ? null : ctx.CurrentItem;
            }
        }

        public static InvoiceProvider SelectInvoiceProvider(bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceProviderSearchDialog(isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceProvider SelectInvoiceProvider(decimal kontrDC, bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceProviderSearchDialog(kontrDC, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClient SelectInvoiceClient(CashInWindowViewModel cashIn, bool isUsePayment, bool isUseAccepted)
        {
            if (cashIn == null) return null;
            var ctx = new InvoiceClientSearchDialog(cashIn.Document.KONTRAGENT_DC ?? 0, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClient SelectInvoiceClient(bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceClientSearchDialog(isUsePayment,isUseAccepted);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }

        public static InvoiceClient SelectInvoiceClient(decimal kontrDC, bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceClientSearchDialog(kontrDC, isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : ctx.CurrentItem;
        }


        public static RSViewModelBase SelectAllInvoiceClient(decimal kontrDc, bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceAllSearchDialog(kontrDc, isUsePayment,isUseAccepted);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : (RSViewModelBase) ctx.CurrentClientItem ?? ctx.CurrentProviderItem;
        }

        public static RSViewModelBase SelectAllInvoiceClient(bool isUsePayment, bool isUseAccepted)
        {
            var ctx = new InvoiceAllSearchDialog(isUsePayment, isUseAccepted);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            return !ctx.DialogResult ? null : (RSViewModelBase)ctx.CurrentClientItem ?? ctx.CurrentProviderItem;
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
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.CurrentItem;
        }

        public static Bank SelectBank()
        {
            var ctx = new BankSelectDialogViewModel();
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return ctx.CurrentItem;
        }
    }
}