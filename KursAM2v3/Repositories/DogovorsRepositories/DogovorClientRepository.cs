using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.Dogovora;
using Core.Finance;
using Data;
using Data.Repository;

namespace KursAM2.Repositories.DogovorsRepositories
{
    public interface IDogovorClientRepository
    {
        DogovorClient Dogovor { set; get; }

        DogovorClient GetByGuidId(Guid id);
        DogovorClient GetFullCopy(DogovorClient doc);
        DogovorClient GetRequisiteCopy(DogovorClient doc);
        List<DogovorClient> GetAllByDates(DateTime dateStart, DateTime dateEnd);
        DogovorClient CreateNew();
        List<DogovorClientFactViewModel> GetOtgruzkaInfo();

        List<LinkDocumentInfo> GetLinkDocuments();
        List<InvoicePaymentDocument> GetPayments();
    }

    public class DogovorClientRepository : GenericKursDBRepository<DogovorClient>, IDogovorClientRepository
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;

        public DogovorClientRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>) unitOfWork;
        }

        public DogovorClientRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public DogovorClient Dogovor { get; set; }

        public DogovorClient GetByGuidId(Guid id)
        {
            return Context.DogovorClient
                .Include(_ => _.SD_102)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_43)
                .Include(_ => _.DogovorClientRow)
                .FirstOrDefault(_ => _.Id == id);
        }

        public DogovorClient GetFullCopy(DogovorClient doc)
        {
            throw new NotImplementedException();
        }

        public DogovorClient GetRequisiteCopy(DogovorClient doc)
        {
            throw new NotImplementedException();
        }

        public List<DogovorClient> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.DogovorClient
                .Include(_ => _.SD_102)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_43)
                .Include(_ => _.DogovorClientRow)
                .Where(_ => _.DogDate >= dateStart && _.DogDate <= dateEnd).ToList();
        }

        public DogovorClient CreateNew()
        {
            var item = new DogovorClient
            {
                Id = Guid.NewGuid(),
                Creator = GlobalOptions.UserInfo.NickName,
                DogDate = DateTime.Today
            };
            Context.DogovorClient.Add(item);
            return item;
        }

        public List<DogovorClientFactViewModel> GetOtgruzkaInfo()
        {
            if (Dogovor == null) return null;
            var ret = new List<DogovorClientFactViewModel>();
            var data = Context.SD_84
                .Include(_ => _.TD_84)
                .Include("TD_84.TD_24")
                .Include("TD_84.TD_24.SD_24")
                .Where(_ => _.DogovorClientId == Dogovor.Id);
            foreach (var s in data)
            foreach (var t in s.TD_84)
            foreach (var r in t.TD_24)
            {
                var newFact = new DogovorClientFactViewModel
                {
                    SFactDC = s.DOC_CODE,
                    Nomenkl = MainReferences.GetNomenkl(t.SFT_NEMENKL_DC),
                    Price = (t.SFT_SUMMA_K_OPLATE ?? 0) / (decimal) t.SFT_KOL,
                    SFactNote = t.SFT_TEXT,
                    WayBillDC = r.DOC_CODE,
                    WayBillNote = r.SD_24.DD_NOTES,
                    Quantity = r.DDT_KOL_RASHOD,
                    Summa = (t.SFT_SUMMA_K_OPLATE ?? 0) * r.DDT_KOL_RASHOD / (decimal) t.SFT_KOL
                };
                ret.Add(newFact);
            }

            return ret;
        }

        public List<LinkDocumentInfo> GetLinkDocuments()
        {
            if (Dogovor == null) return null;
            var ret = new List<LinkDocumentInfo>();
            var sflist = Context.SD_84.Where(_ => _.DogovorClientId == Dogovor.Id).ToList();
            foreach (var sf in sflist)
            {
                ret.Add(new LinkDocumentInfo
                {
                    DocumentType = DocumentType.InvoiceClient,
                    DocCode = sf.DOC_CODE,
                    DocDate = sf.SF_DATE,
                    DocNumber = $"{sf.SF_IN_NUM}/{sf.SF_OUT_NUM}",
                    Summa = sf.SF_CRS_SUMMA_K_OPLATE ?? 0,
                    DocInfo = sf.SF_NOTE
                });
                foreach (var c in Context.SD_33.Where(_ => _.SFACT_DC == sf.DOC_CODE).ToList())
                    ret.Add(new LinkDocumentInfo
                    {
                        DocCode = c.DOC_CODE,
                        Code = 0,
                        DocumentType = DocumentType.CashIn,
                        DocDate = c.DATE_ORD ?? DateTime.MinValue,
                        DocNumber = $"{c.NUM_ORD}",
                        Summa = c.CRS_SUMMA ?? 0,
                        Note = c.NOTES_ORD
                    });
                foreach (var c in Context.TD_101.Include(_ => _.SD_101).Where(_ => _.VVT_SFACT_CLIENT_DC == sf.DOC_CODE)
                    .ToList())
                    ret.Add(new LinkDocumentInfo
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.Bank,
                        DocDate = c.SD_101.VV_START_DATE,
                        DocNumber = "",
                        Summa = c.VVT_VAL_PRIHOD ?? 0,
                        Note = c.VVT_DOC_NUM
                    });
                foreach (var c in Context.TD_110.Include(_ => _.SD_110).Where(_ => _.VZT_SFACT_DC == sf.DOC_CODE)
                    .ToList())
                    ret.Add(new LinkDocumentInfo
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.MutualAccounting,
                        DocDate = c.SD_110.VZ_DATE,
                        DocNumber = $"{c.SD_110.VZ_NUM}",
                        Summa = c.VZT_CRS_SUMMA ?? 0,
                        Note = c.VZT_DOC_NOTES
                    });
            }

            return ret;
        }

        public List<InvoicePaymentDocument> GetPayments()
        {
            var ret = new List<InvoicePaymentDocument>();
            var sflist = Context.SD_84.Where(_ => _.DogovorClientId == Dogovor.Id).ToList();
            foreach (var sf in sflist)
            {
                foreach (var c in Context.SD_33.Where(_ => _.SFACT_DC == sf.DOC_CODE).ToList())
                    ret.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = 0,
                        DocumentType = DocumentType.CashIn,
                        // ReSharper disable once PossibleInvalidOperationException
                        DocumentName =
                            $"{c.NUM_ORD} от {c.DATE_ORD.Value.ToShortDateString()} на {c.SUMM_ORD} " +
                            // ReSharper disable once PossibleInvalidOperationException
                            $"{MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) c.SUMM_ORD,
                        Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
                        Note = c.NOTES_ORD
                    });

                foreach (var c in Context.TD_101.Include(_ => _.SD_101).Where(_ => _.VVT_SFACT_CLIENT_DC == sf.DOC_CODE)
                    .ToList())
                    ret.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.Bank,
                        DocumentName =
                            // ReSharper disable once PossibleInvalidOperationException
                            $"{c.SD_101.VV_START_DATE.ToShortDateString()} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                        Summa = (decimal) c.VVT_VAL_PRIHOD,
                        Currency = MainReferences.Currencies[c.VVT_CRS_DC],
                        Note = c.VVT_DOC_NUM
                    });

                foreach (var c in Context.TD_110.Include(_ => _.SD_110).Where(_ => _.VZT_SFACT_DC == sf.DOC_CODE)
                    .ToList())
                    ret.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.MutualAccounting,
                        DocumentName =
                            // ReSharper disable once PossibleInvalidOperationException
                            $"Взаимозачет №{c.SD_110.VZ_NUM} от {c.SD_110.VZ_DATE.ToShortDateString()} на {c.VZT_CRS_SUMMA}",
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) c.VZT_CRS_SUMMA,
                        Currency = MainReferences.Currencies[c.SD_110.CurrencyFromDC],
                        Note = c.VZT_DOC_NOTES
                    });
            }

            return ret;
        }
    }
}