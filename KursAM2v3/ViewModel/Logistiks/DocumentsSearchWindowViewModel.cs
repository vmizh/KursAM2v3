using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursDomain;
using KursDomain.Documents.Bank;

namespace KursAM2.ViewModel.Logistiks
{
    public class DocumentsSearchWindowViewModel : RSViewModelBase
    {
        private readonly Dictionary<decimal, GruzoInfoViewModel> myTempDirRequisite =
            new Dictionary<decimal, GruzoInfoViewModel>();

/*
        private BankAccount myActualBank = null;
*/
        private ObservableCollection<BankAccount> myActualBanks;
        private BankAccount myDefaultBank;
        private ObservableCollection<DocumentSearchViewModel> myDocuments;
        private DateTime myEndDate;
        private DocumentRowSearchViewModel myNomenklRow;
        private DocumentSearchViewModel mySelectDocument;
        private DateTime myStartDate;

        public DocumentsSearchWindowViewModel()
        {
            StartDate = new DateTime(2015, 4, 1);
            EndDate = new DateTime(2015, 4, 30);
            Documents = new ObservableCollection<DocumentSearchViewModel>();
            BankAll = new List<BankAccount>();
            RefreshData();
        }

        private List<BankAccount> BankAll { get; }

        public DocumentRowSearchViewModel NomenklRow
        {
            get => myNomenklRow;
            set
            {
                if (myNomenklRow != null && myNomenklRow.Equals(value)) return;
                myNomenklRow = value;
                RaisePropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => myStartDate;
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => myEndDate;
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public ObservableCollection<DocumentSearchViewModel> Documents
        {
            set => myDocuments = value;
            get => myDocuments;
        }

        public DocumentSearchViewModel SelectDocument
        {
            set
            {
                if (mySelectDocument != null && mySelectDocument.Equals(value)) return;
                GruzoInfoSave();
                mySelectDocument = value;
                RaisePropertyChanged(nameof(ActualBank));
                GetActualBanks();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ActualBanks));
                RaisePropertyChanged(nameof(AccGruzopol));
                RaisePropertyChanged(nameof(AccGruzoOtprav));
                RaisePropertyChanged(nameof(PlatDocText));
            }
            get => mySelectDocument;
        }

        public BankAccount ActualBank
        {
            set
            {
                if (SelectDocument == null) return;
                if (SelectDocument.GruzoInfo == null)
                    SelectDocument.GruzoInfo = new GruzoInfoViewModel();
                SelectDocument.GruzoInfo.BankAccount = value;
                RaisePropertyChanged();
            }
            get
            {
                if (SelectDocument == null || SelectDocument.GruzoInfo == null) return null;
                return SelectDocument.GruzoInfo.BankAccount;
            }
        }

        public ObservableCollection<BankAccount> ActualBanks
        {
            set
            {
                myActualBanks = value;
                RaisePropertyChanged();
            }
            get => myActualBanks;
        }

        private void GruzoInfoSave()
        {
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    using (var tran = ent.Database.BeginTransaction())
                    {
                        try
                        {
                            if (SelectDocument != null && SelectDocument.GruzoInfo != null)
                            {
                                var g =
                                    ent.GROZO_REQUISITE.SingleOrDefault(
                                        _ => _.Id == SelectDocument.GruzoInfo.Id);
                                if (g == null)
                                {
                                    ent.GROZO_REQUISITE.Add(new GROZO_REQUISITE
                                    {
                                        Id = SelectDocument.GruzoInfo.Id,
                                        ACCOUNT_GRUZOOTPRAV = SelectDocument.GruzoInfo.AccGruzoOtprav,
                                        ACCOUNT_GRUZOPOL = SelectDocument.GruzoInfo.AccGruzopol,
                                        NAKL_GRUOOTPRAV_OKPO = SelectDocument.GruzoInfo.NaklGruzoOtpravOKPO,
                                        NAKL_GRUZOOTPRAV = SelectDocument.GruzoInfo.NaklGruzoOtprav,
                                        NAKL_GRUZOPOL = SelectDocument.GruzoInfo.NaklGruzopol,
                                        NAKL_GRUZOPOL_OKPO = SelectDocument.GruzoInfo.NaklGruzopolOKPO,
                                        BANK_DC =
                                            SelectDocument.GruzoInfo.BankAccount == null
                                                ? null
                                                : SelectDocument.GruzoInfo.BankAccount.KontrBankCode,
                                        PALATEL_TEXT = SelectDocument.GruzoInfo.PlatelText,
                                        PLATEL_OKPO = SelectDocument.GruzoInfo.PlatelOKPO,
                                        PLAT_DOC_TEXT = SelectDocument.GruzoInfo.PlatDocText,
                                        POSTAV_OKPO = SelectDocument.GruzoInfo.PostavOKPO,
                                        POSTAV_TEXT = SelectDocument.GruzoInfo.PostavText
                                    });
                                }
                                else
                                {
                                    g.ACCOUNT_GRUZOOTPRAV = SelectDocument.GruzoInfo.AccGruzoOtprav;
                                    g.ACCOUNT_GRUZOPOL = SelectDocument.GruzoInfo.AccGruzopol;
                                    g.NAKL_GRUOOTPRAV_OKPO = SelectDocument.GruzoInfo.NaklGruzoOtpravOKPO;
                                    g.NAKL_GRUZOOTPRAV = SelectDocument.GruzoInfo.NaklGruzoOtprav;
                                    g.NAKL_GRUZOPOL = SelectDocument.GruzoInfo.NaklGruzopol;
                                    g.NAKL_GRUZOPOL_OKPO = SelectDocument.GruzoInfo.NaklGruzopolOKPO;
                                    g.BANK_DC =
                                        SelectDocument.GruzoInfo.BankAccount == null
                                            ? null
                                            : SelectDocument.GruzoInfo.BankAccount.KontrBankCode;
                                    g.PALATEL_TEXT = SelectDocument.GruzoInfo.PlatelText;
                                    g.PLATEL_OKPO = SelectDocument.GruzoInfo.PlatelOKPO;
                                    g.PLAT_DOC_TEXT = SelectDocument.GruzoInfo.PlatDocText;
                                    g.POSTAV_OKPO = SelectDocument.GruzoInfo.PostavOKPO;
                                    g.POSTAV_TEXT = SelectDocument.GruzoInfo.PostavText;
                                }

                                ent.SaveChanges();
                            }

                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void GetActualBanks()
        {
            if (SelectDocument == null) return;
            ActualBanks = new ObservableCollection<BankAccount>();
            foreach (var b in BankAll.Where(_ => _.DocCode == SelectDocument.KontragentOuterDC))
                ActualBanks.Add(b);
            if (ActualBanks.All(_ => _.KontrBankCode != myDefaultBank.KontrBankCode))
                ActualBanks.Add(myDefaultBank);
        }

        public void RefreshData()
        {
            LoadBanks();
            using (var ent = GlobalOptions.GetEntities())
            {
                var dataSFOut = (from sd84 in ent.SD_84
                        .Include(_ => _.GROZO_REQUISITE)
                        .Include(_ => _.TD_84)
                        .Include("TD_84.SD_83")
                    from client in ent.SD_43
                    from receiver in ent.SD_43
                    from sd301 in ent.SD_301
                    where sd84.SF_DATE >= StartDate && sd84.SF_DATE <= EndDate
                                                    && receiver.DOC_CODE == sd84.SF_RECEIVER_KONTR_DC
                                                    && client.DOC_CODE == sd84.SF_CLIENT_DC
                                                    && sd301.DOC_CODE == sd84.SF_CRS_DC
                    select new
                    {
                        DocumentType = "Клиент",
                        DocumentName = "Cчет-фактура",
                        DocNum = sd84.SF_IN_NUM + (string.IsNullOrEmpty(sd84.SF_OUT_NUM) ? "" : "/" + sd84.SF_OUT_NUM),
                        DocDate = sd84.SF_DATE,
                        KontragentOuter = receiver.NAME,
                        KontragentOuterDC = receiver.DOC_CODE,
                        KontragentInner = client.NAME,
                        KontragentInnerDC = client.DOC_CODE,
                        Currency = sd301.CRS_SHORTNAME,
                        DocCode = sd84.DOC_CODE,
                        Summa = sd84.SF_CRS_SUMMA_K_OPLATE,
                        Note = sd84.SF_NOTE,
                        Rows = sd84.TD_84,
                        GruzoInfo = sd84.GRUZO_INFO_ID == null
                            ? null
                            : new GruzoInfoViewModel
                            {
                                Id = sd84.GROZO_REQUISITE.Id,
                                AccGruzoOtprav = sd84.GROZO_REQUISITE.ACCOUNT_GRUZOOTPRAV,
                                AccGruzopol = sd84.GROZO_REQUISITE.ACCOUNT_GRUZOPOL,
                                BankAccount = sd84.GROZO_REQUISITE.BANK_DC == null
                                    ? null
                                    : new BankAccount
                                    {
                                        DocCode = (decimal)sd84.GROZO_REQUISITE.BANK_DC
                                    },
                                NaklGruzoOtprav = sd84.GROZO_REQUISITE.NAKL_GRUZOOTPRAV,
                                NaklGruzoOtpravOKPO = sd84.GROZO_REQUISITE.NAKL_GRUOOTPRAV_OKPO,
                                NaklGruzopol = sd84.GROZO_REQUISITE.NAKL_GRUZOPOL,
                                NaklGruzopolOKPO = sd84.GROZO_REQUISITE.NAKL_GRUZOPOL_OKPO,
                                PlatDocText = sd84.GROZO_REQUISITE.PLAT_DOC_TEXT,
                                PlatelOKPO = sd84.GROZO_REQUISITE.PLATEL_OKPO,
                                PlatelText = sd84.GROZO_REQUISITE.PALATEL_TEXT,
                                PostavOKPO = sd84.GROZO_REQUISITE.POSTAV_OKPO,
                                PostavText = sd84.GROZO_REQUISITE.POSTAV_TEXT
                            }
                    }).ToList();
                myTempDirRequisite.Clear();
                foreach (var d in dataSFOut.Where(d => d.GruzoInfo == null))
                    GenerateDefaultGruzoInfo(d.DocCode);

                //var dataSFIn = from sd26 in ent.SD_26
                //    from post in ent.SD_43
                //    from sd301 in ent.SD_301
                //    where sd26.SF_POSTAV_DATE >= StartDate && sd26.SF_POSTAV_DATE <= EndDate
                //          && post.DOC_CODE == sd26.SF_POST_DC
                //          && sd301.DOC_CODE == sd26.SF_CRS_DC
                //    select new
                //    {
                //        DocumentType = "Поставщик",
                //        DocumentName = "Cчет-фактура",
                //        DocNum =
                //        sd26.SF_IN_NUM + (string.IsNullOrEmpty(sd26.SF_POSTAV_NUM) ? "" : "/" + sd26.SF_POSTAV_NUM),
                //        DocDate = sd26.SF_POSTAV_DATE,
                //        KontragentOuter = post.NAME,
                //        KontragentInner = GlobalOptions.SystemProfile.OwnerKontragentViewModel.Name,
                //        Currency = sd301.CRS_SHORTNAME,
                //        DocCode = sd26.DOC_CODE,
                //        Summa = sd26.SF_CRS_SUMMA,
                //        Note = sd26.SF_NOTES
                //    };
                // ReSharper disable AccessToDisposedClosure
                var dataNakladOut = from sd24 in ent.SD_24
                    from client in ent.SD_43
                    from sd84 in ent.SD_84
                    from sd301 in ent.SD_301
                    where sd24.DD_DATE >= StartDate && sd24.DD_DATE <= EndDate
                                                    && client.DOC_CODE == sd24.DD_KONTR_POL_DC
                                                    && sd84.DOC_CODE == sd24.DD_SFACT_DC
                                                    && sd24.DD_TYPE_DC == 2010000012
                                                    && sd301.DOC_CODE == sd84.SF_CRS_DC
                    select new
                    {
                        DocumentType = "Клиент",
                        DocumentName = "Расходная накладная",
                        DocNum = sd24.DD_IN_NUM + (string.IsNullOrEmpty(sd24.DD_EXT_NUM) ? "" : "/" + sd24.DD_EXT_NUM),
                        DocDate = sd24.DD_DATE,
                        KontragentOuter = GlobalOptions.SystemProfile.OwnerKontragent.Name,
                        KontragentOuterDC = GlobalOptions.SystemProfile.OwnerKontragent.DocCode,
                        KontragentInner = client.NAME,
                        KontragentInnerDC = client.DOC_CODE,
                        Currency = sd301.CRS_SHORTNAME,
                        DocCode = sd24.DOC_CODE,
                        Summa = (from td24 in ent.TD_24
                            from td84 in ent.TD_84
                            where td24.DOC_CODE == sd24.DOC_CODE
                                  && td84.DOC_CODE == td24.DDT_SFACT_DC && td84.CODE == td24.DDT_SFACT_ROW_CODE
                            select new
                            {
                                Summa = td24.DDT_KOL_RASHOD * td84.SFT_ED_CENA
                            }).Sum(_ => _.Summa),
                        Note = sd24.DD_NOTES
                    };
                // ReSharper restore AccessToDisposedClosure
                Documents.Clear();
                foreach (var d in dataSFOut)
                {
                    var newDoc = new DocumentSearchViewModel
                    {
                        DocumentType = d.DocumentType,
                        DocumentName = d.DocumentName,
                        DocNum = d.DocNum,
                        DocDate = d.DocDate,
                        KontragentOuter = d.KontragentOuter,
                        KontragentInner = d.KontragentInner,
                        KontragentInnerDC = d.KontragentInnerDC,
                        KontragentOuterDC = d.KontragentOuterDC,
                        Currency = d.Currency,
                        DocCode = d.DocCode,
                        Summa = (decimal)d.Summa,
                        Note = d.Note,
                        GruzoInfo =
                            d.GruzoInfo ?? myTempDirRequisite.FirstOrDefault(_ => _.Key == d.DocCode).Value,
                        Rows = new ObservableCollection<DocumentRowSearchViewModel>()
                    };
                    foreach (var ddd in d.Rows)
                        newDoc.Rows.Add(new DocumentRowSearchViewModel
                        {
                            Name = ddd.SD_83.NOM_NAME,
                            NomName = ddd.SD_83.NOM_NOMENKL,
                            IsUsluga = ddd.SD_83.NOM_0MATER_1USLUGA == 1,
                            DocCode = ddd.SD_83.DOC_CODE,
                            Note = ddd.SFT_TEXT,
                            Quantity = (decimal)ddd.SFT_KOL,
                            // ReSharper disable once PossibleInvalidOperationException
                            Price = (decimal)ddd.SFT_ED_CENA,
                            Summa = (decimal)(ddd.SFT_KOL * (double)ddd.SFT_ED_CENA)
                        });
                    Documents.Add(newDoc);
                }

                foreach (var d in dataNakladOut)
                    Documents.Add(new DocumentSearchViewModel
                    {
                        DocumentType = d.DocumentType,
                        DocumentName = d.DocumentName,
                        DocNum = d.DocNum,
                        DocDate = d.DocDate,
                        KontragentOuter = d.KontragentOuter,
                        KontragentInner = d.KontragentInner,
                        KontragentInnerDC = d.KontragentInnerDC,
                        KontragentOuterDC = d.KontragentOuterDC,
                        Currency = d.Currency,
                        DocCode = d.DocCode,
                        Summa = (decimal)d.Summa,
                        Note = d.Note
                    });
            }
        }

        private void LoadBanks()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var banks = from td43 in ent.TD_43
                    // ReSharper disable once AccessToDisposedClosure
                    from sd44 in ent.SD_44
                    where td43.BANK_DC == sd44.DOC_CODE
                          && (td43.DELETED ?? 0) == 0 && (td43.DISABLED ?? 0) == 0
                    select new
                    {
                        DocCode = td43.DOC_CODE,
                        BankDC = sd44.DOC_CODE,
                        Code = td43.CODE,
                        BankName = sd44.BANK_NAME,
                        CorrAccount = sd44.CORRESP_ACC,
                        BIK = sd44.POST_CODE,
                        Account = td43.RASCH_ACC
                    };
                var defBankCode = 0;
                var dd =
                    ent.PROFILE.SingleOrDefault(_ => _.SECTION == "ПО_УМОЛЧАНИЮ" && _.ITEM == "БАНК");
                if (dd != null)
                    defBankCode = Convert.ToInt32(dd.ITEM_VALUE);
                BankAll.Clear();
                foreach (var b in banks)
                    BankAll.Add(new BankAccount
                    {
                        DocCode = b.DocCode,
                        KontrBankCode = b.Code,
                        Account = b.Account,
                        BankDC = b.BankDC
                    });
                myDefaultBank = BankAll.SingleOrDefault(_ => _.KontrBankCode == defBankCode);
            }
        }

        private void GenerateDefaultGruzoInfo(decimal dc)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                using (var tran = ent.Database.BeginTransaction())
                {
                    try
                    {
                        var newId = Guid.NewGuid();
                        var schet = ent.SD_84
                            .Include(_ => _.SD_432)
                            .Single(_ => _.DOC_CODE == dc);
                        var bank = ent.TD_43
                            .Include(_ => _.SD_44)
                            .FirstOrDefault(
                                _ => _.DOC_CODE == schet.SF_RECEIVER_KONTR_DC && schet.SF_RECEIVER_RS_CODE == _.CODE);
                        var newReq = new GROZO_REQUISITE
                        {
                            Id = newId,
                            PLAT_DOC_TEXT = schet.SF_PAYDOC_TEXT,
                            BANK_DC = bank != null ? bank.CODE : null,
                            POSTAV_OKPO = schet.SD_432.OKPO,
                            ACCOUNT_GRUZOOTPRAV = schet.SD_432.NAME_FULL + " " + schet.SD_432.ADDRESS,
                            ACCOUNT_GRUZOPOL = schet.SD_43.NAME_FULL + " " + schet.SD_43.ADDRESS,
                            NAKL_GRUOOTPRAV_OKPO = schet.SD_432.OKPO,
                            NAKL_GRUZOOTPRAV =
                                schet.SD_432.NAME_FULL +
                                (string.IsNullOrEmpty(schet.SD_432.TEL) ? null : " т. " + schet.SD_432.TEL)
                                + (string.IsNullOrEmpty(schet.SD_432.FAX) ? null : " факс " + schet.SD_432.FAX) +
                                (bank != null
                                    ? string.Format("{0} к/с {1} р/с {2}", bank.SD_44.BANK_NAME, bank.SD_44.CORRESP_ACC,
                                        bank.RASCH_ACC)
                                    : null),
                            NAKL_GRUZOPOL_OKPO = schet.SD_43.OKPO,
                            NAKL_GRUZOPOL = schet.SD_432.NAME_FULL +
                                            (string.IsNullOrEmpty(schet.SD_432.TEL) ? null : " т. " + schet.SD_432.TEL)
                                            +
                                            (string.IsNullOrEmpty(schet.SD_432.FAX)
                                                ? null
                                                : " факс " + schet.SD_432.FAX),
                            PALATEL_TEXT = schet.SD_43.NAME_FULL +
                                           (string.IsNullOrEmpty(schet.SD_43.TEL) ? null : " т. " + schet.SD_43.TEL)
                                           +
                                           (string.IsNullOrEmpty(schet.SD_43.FAX) ? null : " факс " + schet.SD_43.FAX),
                            PLATEL_OKPO = schet.SD_43.OKPO,
                            POSTAV_TEXT = schet.SD_432.NAME_FULL +
                                          (string.IsNullOrEmpty(schet.SD_432.TEL) ? null : " т. " + schet.SD_432.TEL)
                                          +
                                          (string.IsNullOrEmpty(schet.SD_432.FAX) ? null : " факс " + schet.SD_432.FAX)
                        };
                        ent.GROZO_REQUISITE.Add(newReq);
                        schet.GRUZO_INFO_ID = newId;
                        ent.SaveChanges();
                        tran.Commit();
                        myTempDirRequisite.Add(dc, new GruzoInfoViewModel
                        {
                            Id = newReq.Id,
                            AccGruzoOtprav = newReq.ACCOUNT_GRUZOOTPRAV,
                            AccGruzopol = newReq.ACCOUNT_GRUZOPOL,
                            BankAccount = newReq.BANK_DC == null
                                ? null
                                : new BankAccount
                                {
                                    BankDC = (decimal)newReq.BANK_DC
                                },
                            NaklGruzoOtprav = newReq.NAKL_GRUZOOTPRAV,
                            NaklGruzoOtpravOKPO = newReq.NAKL_GRUOOTPRAV_OKPO,
                            NaklGruzopol = newReq.NAKL_GRUZOPOL,
                            NaklGruzopolOKPO = newReq.NAKL_GRUZOPOL_OKPO,
                            PlatDocText = newReq.PLAT_DOC_TEXT,
                            PlatelOKPO = newReq.PLATEL_OKPO,
                            PlatelText = newReq.PALATEL_TEXT,
                            PostavOKPO = newReq.POSTAV_OKPO,
                            PostavText = newReq.POSTAV_TEXT
                        });
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(null, ex);
                        tran.Rollback();
                    }
                }
            }
        }

        public void PrintSchet()
        {
            GruzoInfoSave();
            if (SelectDocument == null) return;
            //var fileAccrep = string.Format("{0}\\Reports\\{1}.repx", Environment.CurrentDirectory,
            //    "AccountForPay");
            //var reportAcc = new XtraReport();
            //reportAcc.LoadLayout(fileAccrep);
            //reportAcc.DataSource = new BindingSource
            {
                //DataSource = SchetFacturaClientViewModel.Load(SelectDocument.DocCode)
            }
            //PrintHelper.ShowPrintPreview(Application.Current.MainWindow, reportAcc);
        }

        public void PrintSF()
        {
            GruzoInfoSave();
            if (SelectDocument == null) return;
            //var frileSFReport = string.Format("{0}\\Reports\\{1}.repx", Environment.CurrentDirectory,
            //    "Invoice2012Report");
            //var reportAcc = new XtraReport();
            //reportAcc.LoadLayout(frileSFReport);
            //reportAcc.DataSource = new BindingSource
            {
                // DataSource = SchetFacturaClientViewModel.Load(SelectDocument.DocCode)
            }
            //PrintHelper.ShowPrintPreview(Application.Current.MainWindow, reportAcc);
        }

        public void PrintReports()
        {
            GruzoInfoSave();
            if (SelectDocument == null) return;
            //var fileAccrep = string.Format("{0}\\Reports\\{1}.repx", Environment.CurrentDirectory,
            //    "AccountForPay");
            //var frileSFReport = string.Format("{0}\\Reports\\{1}.repx", Environment.CurrentDirectory,
            //    "Invoice2012Report");
            //var reportAcc = new XtraReport();
            //var reportSF = new XtraReport();

            //reportSF.LoadLayout(frileSFReport);
            //reportAcc.LoadLayout(fileAccrep);
            //reportAcc.DataSource = new BindingSource
            //{
            //   // DataSource = SchetFacturaClientViewModel.Load(SelectDocument.DocCode)
            //};
            //reportSF.DataSource = new BindingSource
            {
                //DataSource = SchetFacturaClientViewModel.Load(SelectDocument.DocCode)
            }

            //PrintHelper.ShowPrintPreview(Application.Current.MainWindow, reportAcc);
            //PrintHelper.ShowPrintPreview(Application.Current.MainWindow, reportSF);

            //reportAcc.ShowPreview();
            //repView.Show();
        }

        #region

        public string AccGruzoOtprav
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.AccGruzoOtprav;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.AccGruzoOtprav == value) return;
                SelectDocument.GruzoInfo.AccGruzoOtprav = value;
                RaisePropertyChanged();
            }
        }

        public string AccGruzopol
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.AccGruzopol;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.AccGruzopol == value) return;
                SelectDocument.GruzoInfo.AccGruzopol = value;
                RaisePropertyChanged();
            }
        }

        public string PlatDocText
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.PlatDocText;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.PlatDocText == value) return;
                SelectDocument.GruzoInfo.PlatDocText = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzoOtprav
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.NaklGruzoOtprav;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.NaklGruzoOtprav == value) return;
                SelectDocument.GruzoInfo.NaklGruzoOtprav = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzopol
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.NaklGruzopol;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.NaklGruzopol == value) return;
                SelectDocument.GruzoInfo.NaklGruzopol = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzoOtpravOKPO
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.NaklGruzoOtpravOKPO;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.NaklGruzoOtpravOKPO == value) return;
                SelectDocument.GruzoInfo.NaklGruzoOtpravOKPO = value;
                RaisePropertyChanged();
            }
        }

        public string NaklGruzopolOKPO
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.NaklGruzopolOKPO;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.NaklGruzopolOKPO == value) return;
                SelectDocument.GruzoInfo.NaklGruzopolOKPO = value;
                RaisePropertyChanged();
            }
        }

        public string PostavText
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.PostavText;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.PostavText == value) return;
                SelectDocument.GruzoInfo.PostavText = value;
                RaisePropertyChanged();
            }
        }

        public string PostavOKPO
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.PostavOKPO;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.PostavOKPO == value) return;
                SelectDocument.GruzoInfo.PostavOKPO = value;
                RaisePropertyChanged();
            }
        }

        public string PlatelText
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.PlatelText;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.PlatelText == value) return;
                SelectDocument.GruzoInfo.PlatelText = value;
                RaisePropertyChanged();
            }
        }

        public string PlatelOKPO
        {
            get
            {
                if (SelectDocument == null) return null;
                return SelectDocument.GruzoInfo == null ? null : SelectDocument.GruzoInfo.PlatelOKPO;
            }
            set
            {
                if (SelectDocument.GruzoInfo == null) return;
                if (SelectDocument.GruzoInfo.PlatelOKPO == value) return;
                SelectDocument.GruzoInfo.PlatelOKPO = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
