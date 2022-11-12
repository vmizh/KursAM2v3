using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Management.Controls;
using KursAM2.ViewModel.Management.Projects;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Currency;
using KursDomain.Documents.Management;
using KursDomain.References;

namespace KursAM2.View.DialogUserControl
{
    public class ProjectDocumentSelect : RSWindowViewModelBase, IDataUserControl
    {
        private bool myAktConvertDocumentType;
        private bool myAktVzaimozachetDocumentType;
        private bool myAllDocumentType;
        private bool myBankDocumentType;
        private bool myCashDocumentType;
        private bool myClientSkladDocumentType;
        private bool myClientUslugiDocumentType;
        private ProjectDocumentSelectViewModel myCurrentDocument;

        private ProjectResultInfo myCurrentProject;
        private ProjectSelectDialogUI myDataUserControl;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private bool myProviderSkladDocumentType;
        private bool myProviderUslugiDocumentType;

        public ProjectDocumentSelect()
        {
            LayoutControl = myDataUserControl = new ProjectSelectDialogUI();
            AllDocumentType = true;
            CurrentProject = new ProjectResultInfo();
            WindowName = "Связь документов с проектом";
        }

        public ProjectDocumentSelect(ProjectResultInfo project) : this()
        {
            CurrentProject = project;
        }

        public string ProjectName { set; get; }

        public ProjectResultInfo CurrentProject
        {
            get => myCurrentProject;
            set
            {
                if (Equals(myCurrentProject, value)) return;
                myCurrentProject = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ProjectDocumentSelectViewModel> SelectedDocs { set; get; } =
            new ObservableCollection<ProjectDocumentSelectViewModel>();

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                if (myDateEnd < DateStart)
                {
                    myDateStart = myDateEnd;
                    RaisePropertyChanged(nameof(DateStart));
                }

                RaisePropertyChanged();
            }
        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                if (myDateStart > DateEnd)
                {
                    myDateEnd = myDateStart;
                    RaisePropertyChanged(nameof(DateEnd));
                }

                RaisePropertyChanged();
            }
        }

        public bool AktConvertDocumentType
        {
            get => myAktConvertDocumentType;
            set
            {
                if (myAktConvertDocumentType == value) return;
                myAktConvertDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool AktVzaimozachetDocumentType
        {
            get => myAktVzaimozachetDocumentType;
            set
            {
                if (myAktVzaimozachetDocumentType == value) return;
                myAktVzaimozachetDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool ProviderUslugiDocumentType
        {
            get => myProviderUslugiDocumentType;
            set
            {
                if (myProviderUslugiDocumentType == value) return;
                myProviderUslugiDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool ClientUslugiDocumentType
        {
            get => myClientUslugiDocumentType;
            set
            {
                if (myClientUslugiDocumentType == value) return;
                myClientUslugiDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool ProviderSkladDocumentType
        {
            get => myProviderSkladDocumentType;
            set
            {
                if (myProviderSkladDocumentType == value) return;
                myProviderSkladDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool CashDocumentType
        {
            get => myCashDocumentType;
            set
            {
                if (myCashDocumentType == value) return;
                myCashDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool BankDocumentType
        {
            get => myBankDocumentType;
            set
            {
                if (myBankDocumentType == value) return;
                myBankDocumentType = value;
                RaisePropertyChanged();
            }
        }

        public bool ClientSkladDocumentType
        {
            get => myClientSkladDocumentType;
            set
            {
                if (myClientSkladDocumentType == value) return;
                myClientSkladDocumentType = value;
                myAllDocumentType = false;
                RaisePropertyChanged(nameof(AllDocumentType));
                RaisePropertyChanged();
            }
        }

        public bool AllDocumentType
        {
            get => myAllDocumentType;
            set
            {
                if (myAllDocumentType == value) return;
                myAllDocumentType = value;
                myAktConvertDocumentType = myAllDocumentType;
                myAktVzaimozachetDocumentType = myAllDocumentType;
                myProviderUslugiDocumentType = myAllDocumentType;
                myClientUslugiDocumentType = myAllDocumentType;
                myProviderSkladDocumentType = myAllDocumentType;
                myCashDocumentType = myAllDocumentType;
                myClientSkladDocumentType = myAllDocumentType;
                myBankDocumentType = myAllDocumentType;
                RaisePropertyChanged(nameof(AktConvertDocumentType));
                RaisePropertyChanged(nameof(AktVzaimozachetDocumentType));
                RaisePropertyChanged(nameof(ProviderUslugiDocumentType));
                RaisePropertyChanged(nameof(ClientUslugiDocumentType));
                RaisePropertyChanged(nameof(ProviderSkladDocumentType));
                RaisePropertyChanged(nameof(CashDocumentType));
                RaisePropertyChanged(nameof(ClientSkladDocumentType));
                RaisePropertyChanged(nameof(BankDocumentType));
                RaisePropertyChanged();
            }
        }

        public ProjectSelectDialogUI DataUserControl
        {
            get => myDataUserControl;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myDataUserControl == value) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ProjectDocumentSelectViewModel> DocumentList { set; get; } =
            new ObservableCollection<ProjectDocumentSelectViewModel>();

        public ProjectDocumentSelectViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (Equals(myCurrentDocument, value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public DependencyObject LayoutControl { get; }

        #region Command

        public ICommand LoadDocumentCommand
        {
            get { return new Command(LoadDocument, _ => isCanDocumentLoad()); }
        }

        public ICommand RefrashDocumentCommand
        {
            get { return new Command(RefrashDocument, _ => true); }
        }

        private void RefrashDocument(object obj)
        {
            DocumentList.Clear();
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddMonths(-1);
            AllDocumentType = true;
            RaisePropertyChanged(nameof(DocumentList));
        }

        private bool isCanDocumentLoad()
        {
            if (AktConvertDocumentType ||
                AktVzaimozachetDocumentType ||
                ProviderUslugiDocumentType ||
                ClientUslugiDocumentType ||
                ProviderSkladDocumentType ||
                CashDocumentType ||
                BankDocumentType ||
                ClientSkladDocumentType)
                return true;
            return false;
        }

        private void LoadDocument(object obj)
        {
            DocumentList.Clear();
            if (CashDocumentType)
                LoadCash();
            if (BankDocumentType)
                LoadBank();
            if (ProviderSkladDocumentType)
                LoadProviderStore();
            if (ClientSkladDocumentType)
                LoadClientStore();
            if (ProviderUslugiDocumentType)
                LoadProviderUslugi();
            if (ClientUslugiDocumentType)
                LoadClientUslugi();
            if (AktVzaimozachetDocumentType)
                LoadAktVzaimozachet();
            if (AktConvertDocumentType)
                LoadAktVzaimozachetConvert();
            if (DataUserControl?.gridDocument != null)
                MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(DataUserControl.gridDocument, DocumentList);
        }

        private void LoadAktVzaimozachet()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_110
                            .Include(_ => _.SD_110)
                            .Include(_ => _.SD_110.SD_111)
                            .AsNoTracking()
                            .Where(_ => _.SD_110.VZ_DATE >= DateStart && _.SD_110.VZ_DATE <= DateEnd &&
                                        _.SD_110.SD_111.IsCurrencyConvert == false)
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE,
                                code = d.CODE
                            } equals
                            new
                            {
                                dc = p.DocDC,
                                code = p.DocRowId ?? 0
                            }
                            into pd
                        where pd.All(_ => _.DocDC != d.DOC_CODE && _.DocRowId != d.CODE)
                        select d).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency,
                            Name = "Акт взаимозачета",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.VZT_KONTR_DC) as Kontragent,
                            DocDate = d.SD_110.VZ_DATE,
                            DocName = "Акт взаимозачета",
                            DocNum = d.VZT_DOC_NUM,
                            DocType = DocumentType.MutualAccounting,
                            Employee = null,
                            DocRowId = d.CODE,
                            IsSelected = false,
                            Note = null,
                            Sum = d.VZT_CRS_SUMMA,
                            ConfirmedSum = (decimal)d.VZT_CRS_SUMMA,
                            AnotherDocConfirmedSum = 0
                        };
                        MultyCurrencyHelper.SetCurrencyValue((decimal)d.VZT_CRS_SUMMA,
                            GlobalOptions.ReferencesCache.GetCurrency(d.VZT_CRS_DC) as Currency, newdoc,
                            d.VZT_1MYDOLZH_0NAMDOLZH == 1
                                ? TypeProfitAndLossCalc.IsLoss
                                : TypeProfitAndLossCalc.IsProfit);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadAktVzaimozachet");
            }
        }

        private void LoadAktVzaimozachetConvert()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from td110 in ctx.TD_110
                        join sd110 in ctx.SD_110 on td110.DOC_CODE equals sd110.DOC_CODE
                        join sd111 in ctx.SD_111 on sd110.VZ_TYPE_DC equals sd111.DOC_CODE
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = td110.DOC_CODE,
                                code = td110.CODE
                            } equals
                            new
                            {
                                dc = p.DocDC,
                                code = p.DocRowId ?? 0
                            }
                            into pd
                        where pd.All(_ => _.DocDC != td110.DOC_CODE && _.DocRowId != td110.CODE)
                              && sd111.IsCurrencyConvert
                        select new
                        {
                            DocCode = sd110.DOC_CODE,
                            Code = td110.CODE,
                            CrsDc = td110.VZT_CRS_DC,
                            KontrDc = td110.VZT_KONTR_DC,
                            Date = td110.VZT_DOC_DATE,
                            InNum = sd110.VZ_NUM,
                            MYDOLZH_0NAMDOLZH = td110.VZT_1MYDOLZH_0NAMDOLZH,
                            Summa = td110.VZT_CRS_SUMMA
                        }).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CrsDc) as Currency,
                            Name = "Акт конвертации",
                            DocCode = d.DocCode,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDc) as Kontragent,
                            DocDate = d.Date,
                            DocName = "Акт конвертации",
                            DocNum = d.InNum.ToString(),
                            DocRowId = d.Code,
                            DocType = DocumentType.MutualAccounting,
                            Employee = null,
                            IsSelected = false,
                            Note = null,
                            Sum = d.Summa,
                            ConfirmedSum = (decimal)d.Summa,
                            AnotherDocConfirmedSum = 0
                        };
                        if (d.MYDOLZH_0NAMDOLZH == 1)
                            MultyCurrencyHelper.SetCurrencyValue(d.Summa,
                                GlobalOptions.ReferencesCache.GetCurrency(d.CrsDc) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsLoss);
                        else
                            MultyCurrencyHelper.SetCurrencyValue(d.Summa,
                                GlobalOptions.ReferencesCache.GetCurrency(d.CrsDc) as Currency, newdoc,
                                TypeProfitAndLossCalc.IsProfit);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadAktVzaimozachet");
            }
        }

        private void LoadBank()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_101
                            .Include(_ => _.SD_101)
                            .AsNoTracking()
                            .Where(_ => _.SD_101.VV_STOP_DATE >= DateStart && _.SD_101.VV_STOP_DATE <= DateEnd)
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE,
                                code = d.CODE
                            } equals
                            new
                            {
                                dc = p.DocDC,
                                code = p.DocRowId ?? 0
                            }
                            into pd
                        where pd.All(_ => _.DocDC != d.DOC_CODE && _.DocRowId != d.CODE)
                        select d).ToList();
                    var dataRasp = (from d in ctx.TD_101
                            .Include(_ => _.SD_101)
                            .AsNoTracking()
                            .Where(_ => _.SD_101.VV_STOP_DATE >= DateStart && _.SD_101.VV_STOP_DATE <= DateEnd)
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE,
                                code = d.CODE
                            } equals
                            new
                            {
                                dc = p.DocDC,
                                code = p.DocRowId ?? 0
                            }
                            into pd
                        where pd.All(_ => _.DocDC == d.DOC_CODE && _.DocRowId == d.CODE &&
                                          _.ProjectId != CurrentProject.Id) &&
                              pd.Sum(_ => Math.Abs(_.FactCRSSumma ?? 0)) < (decimal)d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD
                        select new
                        {
                            Row = d,
                            ConfirmedSum = pd.Sum(_ => _.FactCRSSumma)
                        }).ToList();
                    foreach (var d1 in dataRasp)
                    {
                        var d = d1.Row;
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency,
                            Name = "Банковская выписка",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.VVT_KONTRAGENT) as Kontragent,
                            DocDate = d.SD_101.VV_STOP_DATE,
                            DocName = "Банковская выписка" + "(" + (d.VVT_VAL_PRIHOD > 0 ? "поступление)" : "выплата)"),
                            DocNum = d.VVT_DOC_NUM,
                            DocType = DocumentType.Bank,
                            DocRowId = d.CODE,
                            Employee = null,
                            IsSelected = false,
                            Note = null,
                            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.VVT_SHPZ_DC.Value) as SDRSchet,
                            Sum = d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD,
                            ConfirmedSum = (decimal)(d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD - d1.ConfirmedSum),
                            AnotherDocConfirmedSum = (decimal)d1.ConfirmedSum,
                            ProfitOrLossType = d.VVT_VAL_PRIHOD > 0
                                ? TypeProfitAndLossCalc.IsProfit
                                : TypeProfitAndLossCalc.IsLoss
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency, newdoc,
                            newdoc.ProfitOrLossType);

                        DocumentList.Add(newdoc);
                    }

                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency,
                            Name = "Банковская выписка",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.VVT_KONTRAGENT) as Kontragent,
                            DocDate = d.SD_101.VV_STOP_DATE,
                            DocName = "Банковская выписка" + "(" + (d.VVT_VAL_PRIHOD > 0 ? "поступление)" : "выплата)"),
                            DocNum = d.VVT_DOC_NUM,
                            DocType = DocumentType.Bank,
                            DocRowId = d.CODE,
                            Employee = null,
                            IsSelected = false,
                            Note = null,
                            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.VVT_SHPZ_DC.Value) as SDRSchet,
                            Sum = d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD,
                            ConfirmedSum = (decimal)(d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD),
                            AnotherDocConfirmedSum = 0,
                            ProfitOrLossType = d.VVT_VAL_PRIHOD > 0
                                ? TypeProfitAndLossCalc.IsProfit
                                : TypeProfitAndLossCalc.IsLoss
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.VVT_VAL_PRIHOD + d.VVT_VAL_RASHOD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency, newdoc,
                            newdoc.ProfitOrLossType);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadBank");
            }
        }

        private void LoadClientUslugi()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from td84 in ctx.TD_84
                            .Include(_ => _.SD_84)
                            .Include(_ => _.SD_83)
                            .AsNoTracking()
                            .Where(_ => _.SD_84.SF_DATE >= DateStart && _.SD_84.SF_DATE <= DateEnd
                                                                     && _.SD_83.NOM_0MATER_1USLUGA == 1)
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = td84.DOC_CODE,
                                code = td84.CODE
                            } equals
                            new
                            {
                                dc = p.DocDC,
                                code = p.DocRowId ?? 0
                            }
                            into pd
                        where pd.All(_ => _.DocDC != td84.DOC_CODE && _.DocRowId != td84.CODE)
                        select td84).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.SD_84.SF_CRS_DC) as Currency,
                            Name = "Услуги клиентам",
                            DocCode = d.DOC_CODE,
                            Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(d.SD_84.SF_CLIENT_DC) as Kontragent,
                            DocDate = d.SD_84.SF_DATE,
                            DocName = "Услуги клиентам",
                            DocNum = Convert.ToString(d.SD_84.SF_IN_NUM) + "/" + d.SD_84.SF_OUT_NUM,
                            DocType = DocumentType.InvoiceProvider,
                            Employee = null,
                            IsSelected = false,
                            Note = d.SD_84.SF_NOTE,
                            Sum = d.SFT_SUMMA_K_OPLATE,
                            ConfirmedSum = (decimal)d.SFT_SUMMA_K_OPLATE,
                            AnotherDocConfirmedSum = 0
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.SFT_SUMMA_K_OPLATE,
                            GlobalOptions.ReferencesCache.GetCurrency(d.SD_84.SF_CRS_DC) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsLoss);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadClientUslugi");
            }
        }

        private void LoadProviderUslugi()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from td26 in ctx.TD_26
                            .Include(_ => _.SD_26)
                            .Include(_ => _.SD_83)
                            .AsNoTracking()
                            .Where(_ => _.SD_26.SF_POSTAV_DATE >= DateStart && _.SD_26.SF_POSTAV_DATE <= DateEnd
                                                                            && _.SD_83.NOM_0MATER_1USLUGA == 1)
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = td26.DOC_CODE,
                                code = td26.CODE
                            } equals
                            new
                            {
                                dc = p.DocDC,
                                code = p.DocRowId ?? 0
                            }
                            into pd
                        where pd.All(_ => _.DocDC != td26.DOC_CODE && _.DocRowId != td26.CODE)
                        select td26).ToList();
                    foreach (var d in data)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.SD_26.SF_CRS_DC.Value) as Currency,
                            Name = "Услуги поставщиков",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.SD_26.SF_POST_DC) as Kontragent,
                            DocDate = d.SD_26.SF_POSTAV_DATE,
                            DocName = "Услуги поставщиков",
                            DocNum = Convert.ToString(d.SD_26.SF_IN_NUM) + "/" + d.SD_26.SF_POSTAV_NUM,
                            DocType = DocumentType.InvoiceProvider,
                            Employee = null,
                            IsSelected = false,
                            Note = d.SD_26.SF_NOTES,
                            Sum = d.SFT_SUMMA_K_OPLATE,
                            ConfirmedSum = (decimal)d.SFT_SUMMA_K_OPLATE,
                            AnotherDocConfirmedSum = 0
                        };
                        MultyCurrencyHelper.SetCurrencyValue(newdoc.Sum,
                            GlobalOptions.ReferencesCache.GetCurrency(d.SD_26.SF_CRS_DC.Value) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsLoss);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadProviderUslugi");
            }
        }

        private void LoadClientStore()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from sd84 in ctx.SD_84
                        from td84 in ctx.TD_84
                        where td84.DOC_CODE == sd84.DOC_CODE &&
                              sd84.SF_DATE >= DateStart && sd84.SF_DATE <= DateEnd
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = sd84.DOC_CODE
                            } equals
                            new
                            {
                                dc = p.DocDC
                            }
                            into pd
                        where pd.All(_ => _.DocDC != sd84.DOC_CODE)
                        select sd84).ToList();
                    foreach (var item in data)
                    {
                        if (DocumentList.Any(_ => _.DocCode == item.DOC_CODE)) continue;
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(item.SF_CRS_DC) as Currency,
                            Name = "Отгрузка товара",
                            // ReSharper disable once PossibleInvalidOperationException
                            DocCode = item.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(item.SF_CLIENT_DC) as Kontragent,
                            DocDate = item.SF_DATE,
                            DocName = "С/фактура клиенту",
                            DocNum = item.SF_IN_NUM + "/" + item.SF_OUT_NUM,
                            DocType = DocumentType.InvoiceClient,
                            Employee = null,
                            IsSelected = false,
                            Note = item.SF_NOTE,
                            AnotherDocConfirmedSum = 0
                        };
                        var summ = item.TD_84.Sum(s => (decimal)(s.SFT_KOL + (double)s.SFT_ED_CENA));
                        newdoc.Sum = summ;
                        newdoc.ConfirmedSum = summ;

                        MultyCurrencyHelper.SetCurrencyValue(summ,
                            GlobalOptions.ReferencesCache.GetCurrency(item.SF_CRS_DC) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsProfit);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadClientStore");
            }
        }

        private void LoadCash()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var cashInRasp = (from d in ctx.SD_33.Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd
                            && (_.KONTRAGENT_DC != null ||
                                _.TABELNUMBER != null))
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE
                            } equals
                            new
                            {
                                dc = p.DocDC
                            }
                            into pd
                        where pd.All(_ => _.DocDC == d.DOC_CODE && _.ProjectId != CurrentProject.Id)
                              && pd.Sum(_ => Math.Abs(_.FactCRSSumma ?? 0)) < d.SUMM_ORD
                        select new
                        {
                            Row = d,
                            ConfirmedSum = pd.Sum(_ => _.FactCRSSumma)
                        }).ToList();
                    var cashIn = (from d in ctx.SD_33.Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd
                            && (_.KONTRAGENT_DC != null ||
                                _.TABELNUMBER != null))
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE
                            } equals
                            new
                            {
                                dc = p.DocDC
                            }
                            into pd
                        where pd.All(_ => _.DocDC != d.DOC_CODE)
                        select d).ToList();

                    var cashOutRasp = (from d in ctx.SD_34.Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd
                            && (_.KONTRAGENT_DC != null ||
                                _.TABELNUMBER != null))
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE
                            } equals
                            new
                            {
                                dc = p.DocDC
                            }
                            into pd
                        where pd.All(_ => _.DocDC == d.DOC_CODE && _.ProjectId != CurrentProject.Id)
                              && pd.Sum(_ => Math.Abs(_.FactCRSSumma ?? 0)) < d.SUMM_ORD
                        select new
                        {
                            Row = d,
                            ConfirmedSum = pd.Sum(_ => _.FactCRSSumma)
                        }).ToList();

                    var cashOut = (from d in ctx.SD_34.Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd
                            && (_.KONTRAGENT_DC != null ||
                                _.TABELNUMBER != null))
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE
                            } equals
                            new
                            {
                                dc = p.DocDC
                            }
                            into pd
                        where pd.All(_ => _.DocDC != d.DOC_CODE)
                        select d).ToList();
                    foreach (var d1 in cashInRasp)
                    {
                        var d = d1.Row;
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency,
                            Name = "Приходный кассовый ордер",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC) as Kontragent,
                            DocDate = d.DATE_ORD.Value,
                            DocName = "Приходный кассовый ордер",
                            DocNum = Convert.ToString(d.NUM_ORD),
                            DocType = DocumentType.CashIn,
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER) as Employee,
                            IsSelected = false,
                            Note = d.NOTES_ORD,
                            Sum = d.SUMM_ORD,
                            ConfirmedSum = (decimal)d.SUMM_ORD - (decimal)d1.ConfirmedSum,
                            AnotherDocConfirmedSum = (decimal)d1.ConfirmedSum
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.SUMM_ORD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsProfit);
                        DocumentList.Add(newdoc);
                    }

                    foreach (var d in cashIn)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency,
                            Name = "Приходный кассовый ордер",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC) as Kontragent,
                            DocDate = d.DATE_ORD.Value,
                            DocName = "Приходный кассовый ордер",
                            DocNum = Convert.ToString(d.NUM_ORD),
                            DocType = DocumentType.CashIn,
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER) as Employee,
                            IsSelected = false,
                            Note = d.NOTES_ORD,
                            Sum = d.SUMM_ORD,
                            ConfirmedSum = (decimal)d.SUMM_ORD,
                            AnotherDocConfirmedSum = 0
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.SUMM_ORD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsProfit);
                        DocumentList.Add(newdoc);
                    }

                    foreach (var d1 in cashOutRasp)
                    {
                        var d = d1.Row;
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency,
                            Name = "Расходный кассовый ордер",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC) as Kontragent,
                            DocDate = d.DATE_ORD.Value,
                            DocName = "Расходный кассовый ордер",
                            DocNum = Convert.ToString(d.NUM_ORD),
                            DocType = DocumentType.CashOut,
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER) as Employee,
                            IsSelected = false,
                            Note = d.NOTES_ORD,
                            Sum = d.SUMM_ORD,
                            ConfirmedSum = (decimal)d.SUMM_ORD - (decimal)d1.ConfirmedSum,
                            AnotherDocConfirmedSum = (decimal)d1.ConfirmedSum
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.SUMM_ORD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsLoss);
                        DocumentList.Add(newdoc);
                    }

                    foreach (var d in cashOut)
                    {
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency,
                            Name = "Расходный кассовый ордер",
                            DocCode = d.DOC_CODE,
                            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC) as Kontragent,
                            DocDate = d.DATE_ORD.Value,
                            DocName = "Расходный кассовый ордер",
                            DocNum = Convert.ToString(d.NUM_ORD),
                            DocType = DocumentType.CashOut,
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER) as Employee,
                            IsSelected = false,
                            Note = d.NOTES_ORD,
                            Sum = d.SUMM_ORD,
                            ConfirmedSum = (decimal)d.SUMM_ORD,
                            AnotherDocConfirmedSum = 0
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.SUMM_ORD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsLoss);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadCash");
            }
        }

        private void LoadProviderStore()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = (from d in ctx.TD_24
                            .Include(_ => _.SD_24)
                            .Include(_ => _.TD_26)
                            .Include(_ => _.TD_26.SD_26)
                            .Where(_ => _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd
                                                                     && _.DDT_SPOST_DC != null)
                        join p in ctx.ProjectsDocs on
                            new
                            {
                                dc = d.DOC_CODE
                            } equals
                            new
                            {
                                dc = p.DocDC
                            }
                            into pd
                        where pd.All(_ => _.DocDC != d.TD_26.SD_26.DOC_CODE)
                        select d).ToList();
                    foreach (var d in data)
                    {
                        if (DocumentList.Any(_ => _.DocCode == d.TD_26.SD_26.DOC_CODE)) continue;
                        var newdoc = new ProjectDocumentSelectViewModel
                        {
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(d.TD_26.SD_26.SF_CRS_DC) as Currency,
                            Name = "Приход товара",
                            DocCode = d.TD_26.SD_26.DOC_CODE,
                            Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_OTPR_DC) as Kontragent,
                            DocDate = d.SD_24.DD_DATE,
                            DocName = "Приход товара",
                            DocNum = Convert.ToString(d.SD_24.DD_IN_NUM) + "/" + d.SD_24.DD_EXT_NUM + "С/ф №" +
                                     d.TD_26.SD_26.SF_IN_NUM + "/" + d.TD_26.SD_26.SF_POSTAV_NUM,
                            DocType = DocumentType.InvoiceProvider,
                            Employee = null,
                            IsSelected = false,
                            Note = d.TD_26.SD_26.SF_NOTES,
                            Sum = d.DDT_FACT_CRS_CENA * d.DDT_KOL_PRIHOD,
                            ConfirmedSum = (decimal)d.DDT_FACT_CRS_CENA * d.DDT_KOL_PRIHOD,
                            AnotherDocConfirmedSum = 0
                        };
                        MultyCurrencyHelper.SetCurrencyValue(d.DDT_FACT_CRS_CENA * d.DDT_KOL_PRIHOD,
                            GlobalOptions.ReferencesCache.GetCurrency(d.TD_26.SD_26.SF_CRS_DC) as Currency, newdoc,
                            TypeProfitAndLossCalc.IsLoss);
                        DocumentList.Add(newdoc);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex, "Ошибка в методе LoadProviderStore");
            }
        }

        #endregion
    }
}
