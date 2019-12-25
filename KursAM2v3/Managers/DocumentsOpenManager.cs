using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers.Base;
using KursAM2.View.Finance;
using KursAM2.View.Finance.Cash;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.KursReferences;
using KursAM2.View.Logistiks;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.View.Personal;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Logistiks.Warehouse;
using KursAM2.ViewModel.Personal;
using KursAM2.ViewModel.Reference;

namespace KursAM2.Managers
{
    public class DocumentsOpenManager
    {
        public static string GetDocTypeName(DocumentType dt)
        {
            // ReSharper disable once InvokeAsExtensionMethod
            return AttributeHelper.GetDisplayName(dt);
        }

        public static bool IsDocumentOpen(DocumentType docTypeCode)
        {
            switch (docTypeCode)
            {
                case DocumentType.MutualAccounting:
                case DocumentType.CurrencyConvertAccounting:
                case DocumentType.InvoiceClient:
                case DocumentType.ProjectsReference:
                case DocumentType.CashIn:
                case DocumentType.CurrencyChange:
                case DocumentType.CashOut:
                case DocumentType.InvoiceProvider:
                case DocumentType.StoreOrderIn:
                case DocumentType.Bank:
                case DocumentType.Waybill:
                case DocumentType.NomenklTransfer:
                case DocumentType.PayRollVedomost:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Возвращает тип документа по коду в таблице SD_201
        /// </summary>
        /// <returns></returns>
        public static DocumentType GetMaterialDocTypeFromDC(decimal dc)
        {
            switch (dc)
            {
                case 2010000001:
                    return DocumentType.StoreOrderIn;
                case 2010000005:
                    return DocumentType.InventoryList;
                case 2010000012:
                    return DocumentType.Waybill;
            }
            return DocumentType.None;
        }

        public static void Open(DocumentType docType, decimal dc, Guid? id = null,
            object parent = null)
        {
            if (!IsDocumentOpen(docType)) return;
            switch (docType)
            {
                case DocumentType.MutualAccounting:
                    OpenMutualAccounting(dc);
                    break;
                case DocumentType.CurrencyConvertAccounting:
                    OpenCurrencyConvertAccounting(dc);
                    break;
                case DocumentType.InvoiceClient:
                    OpenSFClient(dc);
                    break;
                case DocumentType.InvoiceProvider:
                    OpenSFProvider(dc);
                    break;
                case DocumentType.ProjectsReference:
                    OpenProjectsReferences();
                    break;
                case DocumentType.CashIn:
                    OpenCashIn(dc, parent);
                    break;
                case DocumentType.CashOut:
                    OpenCashOut(dc, parent);
                    break;
                case DocumentType.CurrencyChange:
                    OpenCurrencyChange(dc, parent);
                    break;
                case DocumentType.NomenklTransfer:
                    OpenNomenklTransfer(id.Value);
                    break;
                case DocumentType.StoreOrderIn:
                    OpenStoreIn(dc);
                    break;
                case DocumentType.Bank:
                    OpenBank(dc);
                    break;
                case DocumentType.Waybill:
                    OpenWayBill(dc);
                    break;
                case DocumentType.PayRollVedomost:
                    OpenPayroll(id);
                    break;
                default:
                    return;
            }
        }

        private static void OpenPayroll(Guid? id)
        {
            var ctx = new PayRollVedomostWindowViewModel(id.ToString());
            var form = new PayRollVedomost();
            form.Show();
            form.DataContext = ctx;
        }

        public static void Open(DocumentType docType, RSWindowViewModelBase vm, object parent = null)
        {
            if (!IsDocumentOpen(docType)) return;
            switch (docType)
            {
                case DocumentType.MutualAccounting:
                    //OpenMutualAccounting(dc);
                    break;
                case DocumentType.CurrencyConvertAccounting:
                    //OpenCurrencyConvertAccounting(dc);
                    break;
                case DocumentType.InvoiceClient:
                    //OpenSFClient(dc);
                    break;
                case DocumentType.InvoiceProvider:
                    //OpenSFProvider(dc);
                    break;
                case DocumentType.ProjectsReference:
                    OpenProjectsReferences();
                    break;
                case DocumentType.CashIn:
                    if (vm is CashInWindowViewModel cashInVM)
                        OpenCashIn(cashInVM, parent);
                    break;
                case DocumentType.CashOut:
                    if (vm is CashOutWindowViewModel cashOutVM)
                        OpenCashOut(cashOutVM, parent);
                    break;
                case DocumentType.CurrencyChange:
                    if (vm is CashCurrencyExchangeViewModel cashExch)
                        OpenCurrencyChange(cashExch, parent);
                    break;
                case DocumentType.NomenklTransfer:
                    //OpenCashIn(vm);
                    break;
                case DocumentType.StoreOrderIn:
                    //OpenStoreIn(dc);
                    break;
                case DocumentType.Waybill:
                    if(vm is WaybillWindowViewModel wayBill)
                     OpenWayBill(wayBill);
                    break;
                case DocumentType.Bank:
                    OpenBank(vm);
                    break;
                default:
                    return;
            }
        }

        private static void OpenBank(RSWindowViewModelBase vm)
        {
            var form = new BankOperationsView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new BankOperationsWindowViewModel(form);
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.TD_101.Include(_ => _.SD_101).FirstOrDefault(_ => _.CODE == vm.Code);
                if (d == null) return;
                dtx.CurrentBankAccount = dtx.BankAccountCollection.FirstOrDefault(_ => _.BankDC == d.SD_101.VV_ACC_DC);
                dtx.CurrentPeriods = dtx.Periods.FirstOrDefault(_ =>
                    _.DateStart.Year == d.SD_101.VV_START_DATE.Year && _.PeriodType == PeriodType.Year);
                dtx.CurrentBankOperations = dtx.BankOperationsCollection.FirstOrDefault(_ => _.Code == vm.Code);
            }
            form.DataContext = dtx;
            dtx.Form = form;
            form.Show();
            for (var i = 0; i < dtx.BankOperationsCollection.Count; i++)
            {
                if (!(form.GridDocuments.GetRow(i) is BankOperationsViewModel row) || row.Code != vm.Code) continue;
                form.TableViewDocuments.FocusedRowHandle = i;
                return;
            }
        }

        private static void OpenBank(decimal dc)
        {
            var form = new BankOperationsView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new BankOperationsWindowViewModel(form);
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.TD_101.Include(_ => _.SD_101).FirstOrDefault(_ => _.CODE == dc);
                if (d == null) return;
                dtx.CurrentBankAccount = dtx.BankAccountCollection.FirstOrDefault(_ => _.BankDC == d.SD_101.VV_ACC_DC);
                dtx.CurrentPeriods = dtx.Periods.FirstOrDefault(_ =>
                    _.DateStart.Year == d.SD_101.VV_START_DATE.Year && _.PeriodType == PeriodType.Year);
                dtx.CurrentBankOperations = dtx.BankOperationsCollection.FirstOrDefault(_ => _.Code == dc);
            }
            form.DataContext = dtx;
            dtx.Form = form;
            form.Show();
            for (var i = 0; i < dtx.BankOperationsCollection.Count; i++)
            {
                if (!(form.GridDocuments.GetRow(i) is BankOperationsViewModel row) || row.Code != dc) continue;
                form.TableViewDocuments.FocusedRowHandle = i;
                return;
            }
        }

        private static void OpenCurrencyChange(decimal dc, object parent)
        {
            var ctx = new CashCurrencyExchangeViewModel(dc)
            {
                BookView = parent as CashBookView
            };
            var form = new CashCurrencyExchangeView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.CreateMenu();
            ctx.Form = form;
            form.Show();
            form.DataContext = ctx;
        }

        private static void OpenCurrencyChange(CashCurrencyExchangeViewModel vm, object parent)
        {
            var ctx = vm;
            ctx.BookView = parent as CashBookView;
            var form = new CashCurrencyExchangeView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            ctx.CreateMenu();
            ctx.Form = form;
            form.Show();
            form.DataContext = ctx;
        }

        public static void Open(DocumentType docType, Guid id)
        {
            //if (!IsDocumentOpen(docType)) return;
            switch (docType)
            {
                case DocumentType.NomenklTransfer:
                    OpenNomenklTransfer(id);
                    break;
                default:
                    return;
            }
        }

        private static void OpenNomenklTransfer(Guid id)
        {
            var frm = new NomenklTransferView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new NomenklTransferWindowViewModel(id) {Form = frm};
            frm.DataContext = ctx;
            frm.Show();
        }

        private static void OpenSFClient(decimal docCode)
        {
            var ctx = new ClientWindowViewModel(docCode);
            var view = new InvoiceClientView {Owner = Application.Current.MainWindow};
            ctx.Form = view;
            view.Show();
            view.DataContext = ctx;
        }

        private static void OpenSFProvider(decimal docCode)
        {
            var ctx = new ProviderWindowViewModel(docCode);
            var view = new InvoiceProviderView{Owner = Application.Current.MainWindow};
            ctx.Form = view;
            view.Show();
            view.DataContext = ctx;
        }

        /// <summary>
        ///     Открывает акт взаимозачета
        /// </summary>
        /// <param name="docCode"></param>
        private static void OpenMutualAccounting(decimal docCode)
        {
            var frm = new MutualAccountingView {Owner = Application.Current.MainWindow};
            var ctx = new MutualAcountingWindowViewModel(docCode)
            {
                IsCurrencyConvert = false,
                Form = frm

            };
            if (ctx.Document == null) return;
            ctx.CreateMenu();
            frm.Show();
            frm.DataContext = ctx;
        }

        /// <summary>
        ///     Открывает акт конвертации
        /// </summary>
        /// <param name="docCode"></param>
        private static void OpenCurrencyConvertAccounting(decimal docCode)
        {
            var frm = new MutualAccountingView {Owner = Application.Current.MainWindow};
            var ctx = new MutualAcountingWindowViewModel(docCode)
            {
                IsCurrencyConvert = true,
                Form = frm
            };
            if (ctx.Document == null) return;
            ctx.CreateMenu();
            frm.Show();
            frm.DataContext = ctx;
            var dtx = (MutualAcountingWindowViewModel) frm.DataContext;
            dtx.Document.State = RowStatus.NotEdited;
        }

        private static void OpenProjectsReferences()
        {
            var form = new ProjectReferenceView
            {
                Owner = Application.Current.MainWindow
            };
            form.DataContext = new ProjectReferenceWindowViewModel
            {
                Form = form
            };
            form.Show();
        }

        private static void OpenCashOut(decimal dc, object parent)
        {
            var ctx = new CashOutWindowViewModel(dc)
            {
                BookView = parent as CashBookView
            };
            var form = new CashOutView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            ctx.CreateMenu();
            form.Show();
            form.DataContext = ctx;
        }

        private static void OpenCashOut(CashOutWindowViewModel vm, object parent)
        {
            var ctx = vm;
            ctx.BookView = parent as CashBookView;
            var form = new CashOutView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            ctx.CreateMenu();
            form.Show();
            form.DataContext = ctx;
        }

        private static void OpenCashIn(decimal dc, object parent)
        {
            var ctx = new CashInWindowViewModel(dc)
            {
                BookView = parent as CashBookView
            };
            var form = new CashInView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            ctx.CreateMenu();
            form.Show();
            //form.DataContext = ctx;
        }

        private static void OpenCashIn(CashInWindowViewModel vm, object parent)
        {
            var ctx = vm;
            ctx.BookView = parent as CashBookView;
            var form = new CashInView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            ctx.CreateMenu();
            form.Show();
            form.DataContext = ctx;
        }

        /// <summary>
        ///     Приходный складской ордер
        /// </summary>
        /// <param name="dc"></param>
        private static void OpenStoreIn(decimal dc)
        {
            var form = new OrderInView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new OrderInWindowViewModel( new StandartErrorManager(GlobalOptions.GetEntities(),form.Name),dc) {Form = form};
            form.Show();
            form.DataContext = ctx;
        }

        /// <summary>
        ///     Расходная накладная
        /// </summary>
        /// <param name="dc"></param>
        private static void OpenWayBill(decimal dc)
        {
            var form = new WaybillView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new WaybillWindowViewModel(dc) {Form = form};
            form.Show();
            form.DataContext = ctx;
        }

        private static void OpenWayBill(WaybillWindowViewModel vm)
        {
            if (vm == null)
                return;
            var dc = vm.DocCode;
            OpenWayBill(dc);
        }
    }
}