using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.Managers.Base;
using KursAM2.View.Dogovors;
using KursAM2.View.Finance;
using KursAM2.View.Finance.AccruedAmount;
using KursAM2.View.Finance.Cash;
using KursAM2.View.Finance.DistributeNaklad;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.KursReferences;
using KursAM2.View.Logistiks;
using KursAM2.View.Logistiks.AktSpisaniya;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.View.Personal;
using KursAM2.View.StockHolder;
using KursAM2.ViewModel.Dogovora;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Finance.AccruedAmount;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Finance.DistributeNaklad;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Logistiks.AktSpisaniya;
using KursAM2.ViewModel.Logistiks.Warehouse;
using KursAM2.ViewModel.Personal;
using KursAM2.ViewModel.Reference;
using KursAM2.ViewModel.StockHolder;
using KursDomain;
using KursDomain.Documents.Bank;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Repository;

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
                case DocumentType.DogovorClient:
                case DocumentType.AktSpisaniya:
                case DocumentType.AccruedAmountForClient:
                case DocumentType.AccruedAmountOfSupplier:
                case DocumentType.DogovorOfSupplier:
                case DocumentType.StockHolderAccrual:
                case DocumentType.Naklad:
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

        public static void SaveLastOpenInfo(DocumentType docType, Guid? docId, decimal? docDC,
            string creator, string lastChanger,
            string desc)
        {
            try
            {
                using (var ctx = GlobalOptions.KursSystem())
                {
                    if (docDC != null)
                        ctx.Database.ExecuteSqlCommand(
                            $"DELETE FROM LastDocument WHERE DocDC = {CustomFormat.DecimalToSqlDecimal(docDC)} " +
                            $"AND UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                            $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' ");
                    if (docId != null && docId != Guid.Empty)
                        ctx.Database.ExecuteSqlCommand(
                            $"DELETE FROM LastDocument WHERE DocId = '{CustomFormat.GuidToSqlString(docId.Value)}' " +
                            $"AND UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                            $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' ");

                    ctx.Database.ExecuteSqlCommand(
                        $"DELETE FROM LastDocument WHERE UserId = '{CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}' " +
                        $"AND DbId = '{CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}' " +
                        $"AND LastOpen < '{CustomFormat.DateToString(DateTime.Today.AddDays(-60))}'");

                    var newItem = new LastDocument
                    {
                        Id = Guid.NewGuid(),
                        UserId = GlobalOptions.UserInfo.KursId,
                        DbId = GlobalOptions.DataBaseId,
                        DocId = docId,
                        Creator = creator ?? "не указан",
                        DocDC = docDC,
                        DocType = (int)docType,
                        LastChanger = lastChanger,
                        LastOpen = DateTime.Now,
                        Description = desc
                    };
                    ctx.LastDocument.Add(newItem);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public static void DeleteFromLastDocument(Guid? id, decimal? dc)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                if (id != null && id != Guid.Empty)
                {
                    var old = ctx.LastDocument.FirstOrDefault(_ => _.DocId == id);
                    if (old != null)
                    {
                        ctx.LastDocument.Remove(old);
                        ctx.SaveChanges();
                    }
                }

                if (dc == null) return;
                {
                    var old = ctx.LastDocument
                        .FirstOrDefault(_ => _.DocDC == dc && _.DbId == GlobalOptions.DataBaseId);
                    if (old != null)
                    {
                        ctx.LastDocument.Remove(old);
                        ctx.SaveChanges();
                    }
                }
            }
        }

        public static void Open(DocumentType docType, decimal dc, Guid? id = null,
            object parent = null, UnitOfWork<ALFAMEDIAEntities> ctx = null)
        {
            if (!IsDocumentOpen(docType)) return;
            switch (docType)
            {
                case DocumentType.Naklad:
                    var nakl = OpenNakladDistribute(id);
                    SaveLastOpenInfo(docType, nakl.Id, null, nakl.Creator,
                        "", nakl.Description);
                    break;

                case DocumentType.StockHolderAccrual:
                    var accr = OpenStockHolderAccrual(id);
                    SaveLastOpenInfo(docType, accr.Document.Id, null, accr.Document.Creator,
                        "", accr.Document.Description);
                    break;
                case DocumentType.DogovorClient:
                    var dog = OpenDogovorClient(id);
                    SaveLastOpenInfo(docType, dog.Document.Id, null, dog.Document.Creator,
                        "", dog.Document.Description);
                    break;
                case DocumentType.MutualAccounting:
                    var mut = OpenMutualAccounting(dc);
                    mut.Document.myState = RowStatus.NotEdited;
                    SaveLastOpenInfo(docType, mut.Document.Id, mut.Document.DocCode, mut.Document.CREATOR,
                        "", mut.Document.Description);
                    break;
                case DocumentType.CurrencyConvertAccounting:
                    var conv = OpenCurrencyConvertAccounting(dc);
                    SaveLastOpenInfo(docType, conv.Document.Id, conv.Document.DocCode, conv.Document.CREATOR,
                        "", conv.Document.Description);
                    break;
                case DocumentType.InvoiceClient:
                    var invcl = OpenSFClient(dc);
                    SaveLastOpenInfo(docType, invcl.Document.Id, invcl.Document.DocCode, invcl.Document.CREATOR,
                        "", invcl.Document.Description);
                    break;
                case DocumentType.InvoiceProvider:
                    var sfp = OpenSFProvider(dc);
                    SaveLastOpenInfo(docType, sfp.Document.Id, sfp.Document.DocCode, sfp.Document.CREATOR,
                        "", sfp.Document.Description);
                    break;
                case DocumentType.ProjectsReference:
                    OpenProjectsReferences();
                    break;
                case DocumentType.CashIn:
                    var cashIn = OpenCashIn(dc, parent);
                    SaveLastOpenInfo(docType, cashIn.Document.Id, cashIn.Document.DocCode, cashIn.Document.CREATOR,
                        "", cashIn.Document.Description);
                    break;
                case DocumentType.CashOut:
                    var cashOut = OpenCashOut(dc, (Window)parent);
                    SaveLastOpenInfo(docType, cashOut.Document.Id, cashOut.Document.DocCode, cashOut.Document.CREATOR,
                        "", cashOut.Document.Description);
                    break;
                case DocumentType.CurrencyChange:
                    var cc = OpenCurrencyChange(dc, parent);
                    SaveLastOpenInfo(docType, cc.Document.Id, cc.Document.DocCode, cc.Document.CREATOR,
                        "", cc.Document.Description);
                    break;
                case DocumentType.NomenklTransfer:
                    // ReSharper disable once PossibleInvalidOperationException
                    var ont = OpenNomenklTransfer(id.Value);
                    SaveLastOpenInfo(docType, ont.Document.Id, ont.Document.DocCode, ont.Document.Creator,
                        "", ont.Document.Description);
                    break;
                case DocumentType.StoreOrderIn:
                    var osi = OpenStoreIn(dc);
                    SaveLastOpenInfo(docType, osi.Document.Id, osi.Document.DocCode, osi.Document.CREATOR,
                        "", osi.Document.Description);
                    break;
                case DocumentType.StoreOrderOut:
                    var oso = OpenStoreOut(dc);
                    SaveLastOpenInfo(docType, oso.Document.Id, oso.Document.DocCode, oso.Document.CREATOR,
                        "", oso.Document.Description);
                    break;
                case DocumentType.Bank:
                    OpenBank(dc, parent);
                    break;
                case DocumentType.Waybill:
                    var owb = OpenWayBill(dc);
                    SaveLastOpenInfo(docType, owb.Document.Id, owb.Document.DocCode, owb.Document.CREATOR,
                        "", owb.Document.Description);
                    break;
                case DocumentType.PayRollVedomost:
                    var op = OpenPayroll(id);
                    SaveLastOpenInfo(docType, op.Id, op.DocCode, op.Creator,
                        "", op.Description);
                    break;
                case DocumentType.AktSpisaniya:
                    var akt = OpenAktSpisaniyaNomenkl(id);
                    SaveLastOpenInfo(docType, akt.Document.Id, null, akt.Document.DocCreator,
                        "", akt.Description);
                    break;
                case DocumentType.AccruedAmountForClient:
                    var aat = OpenAccruedAmountForClient(id, parent);
                    SaveLastOpenInfo(docType, aat.Document.Id, null, aat.Document.Creator,
                        "", aat.Description);
                    break;
                case DocumentType.AccruedAmountOfSupplier:
                    var aas = OpenAccruedAmountOfSupplier(id, parent);
                    SaveLastOpenInfo(docType, aas.Document.Id, null, aas.Document.Creator,
                        "", aas.Description);
                    break;
                case DocumentType.DogovorOfSupplier:
                    var dos = OpenDogovorOfSupplier(id, parent);
                    SaveLastOpenInfo(docType, dos.Document.Id, null, dos.Document.Creator,
                        "", dos.Description);
                    break;
                default:
                    WindowManager.ShowFunctionNotReleased();
                    return;
            }
        }

        private static DistributeNakladViewModel OpenNakladDistribute(Guid? id)
        {
            var dsForm = new DistributedNakladView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new DistributeNakladViewModel(null, new DocumentOpenType
            {
                Id = id,
                OpenType = DocumentCreateTypeEnum.Open
            })
            {
                Form = dsForm
            };
            dsForm.DataContext = dtx;

            foreach (var t in dtx.Tovars) t.State = RowStatus.NotEdited;
            dsForm.Show();
            dtx.State = RowStatus.NotEdited;
            return dtx;
        }

        private static DogovorOfSupplierWindowViewModel OpenDogovorOfSupplier(Guid? id, object parent)
        {
            var ctx = new DogovorOfSupplierWindowViewModel(id) { ParentFormViewModel = (RSWindowViewModelBase)parent };
            var view = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = view;
            view.Show();
            return ctx;
        }

        private static PayRollVedomostWindowViewModel OpenPayroll(Guid? id)
        {
            var ctx = new PayRollVedomostWindowViewModel(id.ToString());
            var form = new PayRollVedomost();
            form.Show();
            form.DataContext = ctx;
            return ctx;
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
                    if (vm is CashCurrencyExchangeWindowViewModel cashExch)
                        OpenCurrencyChange(cashExch, parent);
                    break;
                case DocumentType.NomenklTransfer:
                    //OpenCashIn(vm);
                    break;
                case DocumentType.StoreOrderIn:
                    //OpenStoreIn(dc);
                    break;
                case DocumentType.Waybill:
                    if (vm is WaybillWindowViewModel2 wayBill)
                        OpenWayBill(wayBill);
                    break;
                case DocumentType.Bank:
                    OpenBank(vm);
                    break;
                default:
                    return;
            }
        }

        private static void OpenBank(RSWindowViewModelBase vm, object parent = null)
        {
            var form = new BankOperationsView2
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new BankOperationsWindowViewModel2(form)
            {
                Parent = parent
            };
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.TD_101.Include(_ => _.SD_101).FirstOrDefault(_ => _.CODE == vm.Code);
                if (d == null) return;
                dtx.CurrentBankAccount = dtx.BankAccountCollection.FirstOrDefault(_ => _.DocCode == d.SD_101.VV_ACC_DC);
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

        private static void OpenBank(decimal dc, object parent = null)
        {
            var form = new BankOperationsView2
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new BankOperationsWindowViewModel2(form)
            {
                Parent = parent
            };
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx.TD_101.Include(_ => _.SD_101).FirstOrDefault(_ => _.CODE == dc);
                if (d == null) return;
                dtx.CurrentBankAccount = dtx.BankAccountCollection.FirstOrDefault(_ => _.DocCode == d.SD_101.VV_ACC_DC);
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
                form.GridDocuments.SelectedItem = form.GridDocuments.GetRow(i);
                form.TableViewDocuments.Focus();
                return;
            }
        }

        private static CashCurrencyExchangeWindowViewModel OpenCurrencyChange(decimal dc, object parent)
        {
            var ctx = new CashCurrencyExchangeWindowViewModel(dc)
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
            return ctx;
        }

        private static void OpenCurrencyChange(CashCurrencyExchangeWindowViewModel vm, object parent)
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

        private static NomenklTransferWindowViewModel OpenNomenklTransfer(Guid id)
        {
            var frm = new NomenklTransferView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new NomenklTransferWindowViewModel(id) { Form = frm };
            frm.DataContext = ctx;
            frm.Show();
            return ctx;
        }

        public static ClientWindowViewModel OpenSFClient(decimal docCode, bool isLoadPay = true)
        {
            var ctx = new ClientWindowViewModel(docCode);
            var view = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = view;
            view.Show();
            return ctx;
        }

        private static AccruedAmountForClientWindowViewModel OpenAccruedAmountForClient(Guid? id, object parent)
        {
            var ctx = new AccruedAmountForClientWindowViewModel(id);
            if (parent != null)
                switch (parent)
                {
                    case RSWindowSearchViewModelBase p:
                        ctx.ParentFormViewModel = p;
                        break;
                    case RSWindowViewModelBase p1:
                        ctx.ParentFormViewModel = p1;
                        break;
                }

            var view = new AccruedAmountForClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = view;
            view.Show();
            return ctx;
        }

        //OpenAccruedAmountOfSupplier
        private static AccruedAmountOfSupplierWindowViewModel OpenAccruedAmountOfSupplier(Guid? id, object parent)
        {
            var ctx = new AccruedAmountOfSupplierWindowViewModel(id);
            if (parent != null)
                switch (parent)
                {
                    case RSWindowSearchViewModelBase p:
                        ctx.ParentFormViewModel = p;
                        break;
                    case RSWindowViewModelBase p1:
                        ctx.ParentFormViewModel = p1;
                        break;
                }

            var view = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = view;
            view.Show();
            return ctx;
        }

        private static ProviderWindowViewModel OpenSFProvider(decimal docCode)
        {
            var ctx = new ProviderWindowViewModel(docCode);
            var view = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = view;
            var state = ctx.Document.State;
            view.Show();
            ctx.Document.myState = state;
            ctx.Document.RaisePropertyChanged("State");
            return ctx;
        }

        private static StockHolderAccrualWindowViewModel OpenStockHolderAccrual(Guid? id)
        {
            var ctx = new StockHolderAccrualWindowViewModel();
            var form = new StockHolderAccrualsView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            form.Show();
            ctx.RefreshData(id);

            return ctx;
        }

        private static DogovorClientWindowViewModel OpenDogovorClient(Guid? id)
        {
            if (id == null) return null;
            var form = new DogovorClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new DogovorClientWindowViewModel(id.Value)
            {
                Form = form
            };
            form.DataContext = ctx;
            form.Show();
            return ctx;
        }

        /// <summary>
        ///     Открывает акт взаимозачета
        /// </summary>
        /// <param name="docCode"></param>
        private static MutualAcountingWindowViewModel OpenMutualAccounting(decimal docCode)
        {
            var frm = new MutualAccountingView { Owner = Application.Current.MainWindow };
            var ctx = new MutualAcountingWindowViewModel(docCode)
            {
                IsCurrencyConvert = false,
                Form = frm
            };
            if (ctx.Document == null) return null;
            ctx.CreateMenu();
            frm.Show();
            frm.DataContext = ctx;
            return ctx;
        }

        /// <summary>
        ///     Открывает акт конвертации
        /// </summary>
        /// <param name="docCode"></param>
        private static MutualAcountingWindowViewModel OpenCurrencyConvertAccounting(decimal docCode)
        {
            var frm = new MutualAccountingView { Owner = Application.Current.MainWindow };
            var ctx = new MutualAcountingWindowViewModel(docCode)
            {
                IsCurrencyConvert = true,
                Form = frm
            };
            if (ctx.Document == null) return null;
            ctx.CreateMenu();
            frm.Show();
            frm.DataContext = ctx;
            var dtx = (MutualAcountingWindowViewModel)frm.DataContext;
            dtx.Document.State = RowStatus.NotEdited;
            return ctx;
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

        private static CashOutWindowViewModel OpenCashOut(decimal dc, Window parent)
        {
            var ctx = new CashOutWindowViewModel(dc)
            {
                BookView = parent
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
            return ctx;
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
        }

        private static CashInWindowViewModel OpenCashIn(decimal dc, object parent)
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
            return ctx;
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
        }

        /// <summary>
        ///     Приходный складской ордер
        /// </summary>
        /// <param name="dc"></param>
        private static OrderInWindowViewModel OpenStoreIn(decimal dc)
        {
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(), null), dc);
            var form = new OrderInView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            form.Show();
            return ctx;
        }
        /// <summary>
        ///     Расходный складской ордер
        /// </summary>
        /// <param name="dc"></param>
        private static OrderOutWindowViewModel OpenStoreOut(decimal dc)
        {
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(), null), dc);
            var form = new OrderOutView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = form;
            form.Show();
            return ctx;
        }

        /// <summary>
        ///     Расходная накладная
        /// </summary>
        /// <param name="dc"></param>
        private static WaybillWindowViewModel2 OpenWayBill(decimal dc)
        {
            var form = new WayBillView2
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new WaybillWindowViewModel2(dc) { Form = form };
            form.DataContext = ctx;
            form.Show();
            return ctx;
        }

        private static void OpenWayBill(WaybillWindowViewModel2 vm)
        {
            if (vm == null)
                return;
            var dc = vm.DocCode;
            OpenWayBill(dc);
        }

        private static AktSpisaniyaNomenklTitleWIndowViewModel OpenAktSpisaniyaNomenkl(Guid? id)
        {
            if (id == null) return null;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(id.Value)
            {
                State = RowStatus.NotEdited
            };
            var view = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = view;
            view.Show();
            return ctx;
        }
    }
}
