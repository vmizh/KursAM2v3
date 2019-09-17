using System;
using System.Windows;
using Core;
using KursAM2.Managers.Base;
using KursAM2.View.Finance;
using KursAM2.View.Finance.Cash;
using KursAM2.View.KursReferences;
using KursAM2.View.Logistiks;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Reference;

namespace KursAM2.Managers
{
    public class DocumentsOpenManager
    {
        public static string GetDocTypeName(DocumentType dt)
        {
            return dt.GetDisplayName();
        }
        public static bool IsDocumentOpen(DocumentType docTypeCode)
        {
            switch (docTypeCode)
            {
                case DocumentType.MutualAccounting:
                case DocumentType.CurrencyConvertAccounting:
                case DocumentType.SFClient:
                case DocumentType.ProjectsReference:
                case DocumentType.CashIn:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Возвращает тип документа по коду в таблице SD_201
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

        public static void Open(DocumentType docType, decimal dc)
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
                case DocumentType.SFClient:
                    OpenSFClient(dc);
                    break;
                case DocumentType.ProjectsReference:
                    OpenProjectsReferences();
                    break;
                case DocumentType.CashIn:
                    OperCashIn(dc);
                    break;
                case DocumentType.NomenklTransfer:
                    OperCashIn(dc);
                    break;
                default:
                    return;
            }
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
            frm.Show();
            frm.DataContext = new NomenklTransferWindowViewModel(id);
        }

        private static void OpenSFClient(decimal docCode)
        {
            var ctx = new SFClientWindowViewModel();
            ctx.RefreshData(docCode);
            var view = new SFClientView { Owner = Application.Current.MainWindow };
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
        }

        private static void OpenProjectsReferences()
        {
            var form = new ProjectReferenceView
            {
                Owner = Application.Current.MainWindow,
                DataContext = new ProjectReferenceWindowViewModel()
            };
            form.Show();
        }

        private static void OperCashIn(decimal dc)
        {
            var form = new CashInView
            {
                Owner = Application.Current.MainWindow,
            };
            var ctx = new CashInWindowViewModel(dc);
            ctx.CreateMenu();
            form.Show();
            form.DataContext = ctx;
        }
    }
}