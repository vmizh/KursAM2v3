using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.Helper;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Systems;
using KursDomain.Menu;

namespace KursAM2.ViewModel.StartLogin
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public sealed class LastDocumentWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private LastDocumentViewModel myCurrentcLastDocument;

        #endregion

        #region Constructors

        public LastDocumentWindowViewModel()
        {
            RightMenuBar = MenuGenerator.DialogRightBar(this);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RefreshData(null);
            LayoutName = "LastDocumentWindowView";
        }

        #endregion

        private class DocumentIdItem
        {
            public decimal? DocCode { set; get; }
            public int? Code { set; get; }
            public Guid? Id { set; get; }
        }

        #region Commands

        public override void RefreshData(object obj)
        {
            List<DocumentIdItem> documents;

            using (var context = GlobalOptions.GetEntities())
            {
                var sql = @"SELECT doc_code AS DocCode, cast(null AS int) AS Code, id AS Id FROM sd_24 
                            UNION ALL
                            SELECT doc_code, null AS Code, null from sd_33
                            UNION ALL
                            SELECT doc_code, null AS Code, null from sd_34
                            UNION ALL
                            SELECT doc_code, code AS Code, null from td_101
                            UNION ALL
                            SELECT doc_code, null AS Code, null from sd_110
                            UNION ALL
                            SELECT doc_code, null AS Code, id from sd_26
                            UNION ALL
                            SELECT doc_code, null AS Code, id from sd_84
                            UNION ALL
                            SELECT null, null AS Code, id from AccruedAmountForClient
                            UNION ALL
                            SELECT null, null AS Code, id from AccruedAmountOfSupplier";
                documents = context.Database.SqlQuery<DocumentIdItem>(sql).ToList();
            }

            using (var ctx = GlobalOptions.KursSystem())
            {
                LastDocuments.Clear();
                var d = DateTime.Today.AddDays(-15);
                foreach (var h in ctx.LastDocument.Where(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                                              && _.DbId == GlobalOptions.DataBaseId
                                                              && _.LastOpen > d)
                             .OrderByDescending(_ => _.LastOpen))
                {
                    var newItem = new LastDocumentViewModel(h);
                    switch ((DocumentType)newItem.Entity.DocType)
                    {
                        case DocumentType.CashIn:
                        case DocumentType.CashOut:
                        case DocumentType.MutualAccounting:
                        case DocumentType.InvoiceProvider:
                        case DocumentType.ActReconciliation:
                        case DocumentType.AktSpisaniya:
                        case DocumentType.CurrencyChange:
                        case DocumentType.DogovorClient:
                        case DocumentType.InventoryList:
                        case DocumentType.CurrencyConvertAccounting:
                        case DocumentType.DogovorOfSupplier:
                        case DocumentType.InvoiceClient:
                        case DocumentType.Naklad:
                        case DocumentType.PayRollVedomost:
                            newItem.IsDeleted = documents.All(_ => _.DocCode != newItem.DocCode);
                            break;

                        case DocumentType.Bank:
                            newItem.IsDeleted =
                                !documents.Any(_ => _.DocCode == newItem.DocCode && _.Code == newItem.Code);
                            break;
                        case DocumentType.AccruedAmountForClient:
                        case DocumentType.AccruedAmountOfSupplier:
                            newItem.IsDeleted = documents.All(_ => _.Id != newItem.Id);
                            break;
                    }

                    LastDocuments.Add(newItem);
                }
            }
        }

        //private bool checkExistsDocument(DocumentType docType, decimal? dc = null, Guid? docId = null)
        //{
        //    if (dc == null && docId == null) return false;
        //    using (var context = GlobalOptions.GetEntities())
        //    {
        //        switch (docType)
        //        {
        //            case DocumentType.MutualAccounting:
        //                return dc != null && context.SD_110.Any(_ => _.DOC_CODE == dc.Value);
        //        }
        //    }

        //    return false;
        //}

        public override void DocumentOpen(object obj)
        {
            if (CurrentLastDocument.Entity.DocDC == null && CurrentLastDocument.Entity.DocId == null) return;
            if (CurrentLastDocument.Entity.DocDC != null || CurrentLastDocument.Entity.DocId != null)
            {
                if (!CurrentLastDocument.IsDeleted)
                    DocumentsOpenManager.Open((DocumentType)CurrentLastDocument.Entity.DocType,
                        // ReSharper disable once PossibleInvalidOperationException
                        CurrentLastDocument.Entity.DocDC ?? 0, CurrentLastDocument.Entity.DocId);
                else
                    DocumentHistoryManager.LoadHistory((DocumentType)CurrentLastDocument.Entity.DocType,
                        CurrentLastDocument.Entity.DocId,
                        CurrentLastDocument.Entity.DocDC);
            }
            //Form.Close();
        }

        public override bool IsDocumentOpenAllow => CurrentLastDocument != null;

        #endregion

        #region Properties

        public ObservableCollection<LastDocumentViewModel> LastDocuments { set; get; }
            = new ObservableCollection<LastDocumentViewModel>();

        public LastDocumentViewModel CurrentLastDocument
        {
            get => myCurrentcLastDocument;
            set
            {
                if (myCurrentcLastDocument == value) return;
                myCurrentcLastDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
