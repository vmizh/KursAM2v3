using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Management;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Management
{
    [DataContract]
    public class KontragentBalansWindowViewModel : RSWindowViewModelBase
    {
        private KontragentBalansRowViewModel myCurrentDocument;
        private KontragentPeriod myCurrentPeriod;
        private Kontragent myKontragent;
        private decimal myLastBalansSumma;
        private DateTime myLastOperationDate;

        public KontragentBalansWindowViewModel()
        {
            Operations = new ObservableCollection<KontragentBalansRowViewModel>();
            Periods = new ObservableCollection<KontragentPeriod>();
            Documents = new ObservableCollection<KontragentBalansRowViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        }

        public KontragentBalansWindowViewModel(decimal doccode) : this()
        {
            StartKontragent = Kontragents.Single(_ => _.DocCode == doccode);
        }

        public override string LayoutName => "KontragentBalansView";
        public override string WindowName => "Лицевой счет контрагента";

        public ObservableCollection<KontragentBalansRowViewModel> SelectedDocs { set; get; } =
            new ObservableCollection<KontragentBalansRowViewModel>();

        public Kontragent StartKontragent { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<KontragentBalansRowViewModel> Documents { set; get; }

        public override void AddSearchList(object obj)
        {
            var form = new KontragentBalansView
            {
                Owner = Application.Current.MainWindow
            };
            var ctxk = new KontragentBalansWindowViewModel
            {
                Form = form
            };
            form.DataContext = ctxk;
            form.Show();


        }


        public KontragentPeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (Equals(myCurrentPeriod, value)) return;
                myCurrentPeriod = value;
                if (myCurrentPeriod != null)
                    LoadDocumentsForPeriod();
                else
                    Documents.Clear();
                RaisePropertyChanged();
            }
        }

        public KontragentBalansRowViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsDocumentOpenAllow));
            }
        }

        public List<Kontragent> Kontragents => KontragentManager.GetAllKontragentSortedByUsed();

        [DataMember]
        public Kontragent Kontragent
        {
            get => myKontragent != null
                ? GlobalOptions.ReferencesCache.GetKontragent(myKontragent.DocCode) as Kontragent
                : null;
            set
            {
                if (Equals(myKontragent, value)) return;
                myKontragent = value;
                if (myKontragent != null)
                {
                    KontragentManager.UpdateSelectCount(myKontragent.DocCode);
                    LoadOperations(myKontragent.DocCode);
                    RaisePropertyChanged(nameof(Kontragents));
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Periods));
                RaisePropertyChanged(nameof(CrsName));
                RaisePropertyChanged(nameof(LastBalansSumma));
                RaisePropertyChanged(nameof(LastOperationDate));
            }
        }

        public string CrsName => Kontragent != null && Kontragent.Currency != null
            ? ((IName)Kontragent.Currency).Name
            : null;

        [DataMember]
        public decimal LastBalansSumma
        {
            get => myLastBalansSumma;
            set
            {
                if (myLastBalansSumma == value) return;
                myLastBalansSumma = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public DateTime LastOperationDate
        {
            get => myLastOperationDate;
            set
            {
                if (myLastOperationDate == value) return;
                myLastOperationDate = value;
                RaisePropertyChanged();
            }
        }

        [DataMember] public ObservableCollection<KontragentBalansRowViewModel> Operations { set; get; }

        [DataMember]
        public ObservableCollection<KontragentPeriod> Periods { set; get; } =
            // ReSharper disable once MemberInitializerValueIgnored
            new ObservableCollection<KontragentPeriod>();

        public Brush BalansBrush
            => LastBalansSumma < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);

        private void LoadDocumentsForPeriod()
        {
            Documents.Clear();
            if (CurrentPeriod == null)
            {
                Documents.Clear();
                RaisePropertyChanged(nameof(Documents));
                return;
            }

            foreach (
                var item in
                Operations.Where(_ => _.DocDate >= CurrentPeriod.DateStart && _.DocDate <= CurrentPeriod.DateEnd))
                Documents.Add(item);
            RaisePropertyChanged(nameof(Documents));
        }

        private void LoadAllDocuments()
        {
            Documents.Clear();
            foreach (var item in Operations)
                Documents.Add(item);
            RaisePropertyChanged(nameof(Documents));
        }

        private void LoadPeriod(IEnumerable<KontragentBalansRowViewModel> rows)
        {
            if (rows == null) return;
            var periods = DatePeriod.GenerateIerarhy(rows.Select(_ => _.DocDate).Distinct(), PeriodIerarhy.YearMonth);
            Periods.Clear();
            var localperiods = periods.Select(d => new KontragentPeriod
                {
                    DateStart = d.DateStart,
                    DateEnd = d.DateEnd,
                    Name = d.Name,
                    Id = d.Id,
                    ParentId = d.ParentId,
                    PeriodType = d.PeriodType,
                    EndPeriodSumma = Operations.Last(_ => _.DocDate >= d.DateStart && _.DocDate <= d.DateEnd).Nakopit
                })
                .ToList();
            Periods = new ObservableCollection<KontragentPeriod>(localperiods);
            RaisePropertyChanged(nameof(Periods));
        }

        public void LoadOperations(decimal doccode)
        {
            Load(doccode);
            LoadPeriod(Operations);
        }

        public ObservableCollection<KontragentBalansRowViewModel> Load(decimal dc)
        {
            try
            {
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
                using (var ent = GlobalOptions.GetEntities())
                {
                    if (GlobalOptions.ReferencesCache.GetKontragent(dc) == null)
                        throw new Exception($"Контрагент {dc} не найден. RecalcKontragentBalans.CalcBalans");
                    RecalcKontragentBalans.CalcBalans(dc,
                        GlobalOptions.ReferencesCache.GetKontragent(dc).StartBalans);
                    var sql = "SELECT DOC_NAME as DocName, " +
                              "CASE WHEN DOC_TYPE_CODE = 101 THEN cast(td_101.CODE as varchar) ELSE DOC_NUM END AS DocNum, " +
                              "DOC_DATE as DocDate, " +
                              "cast(CRS_KONTR_IN as numeric(18,4)) as CrsKontrIn, " +
                              "cast(CRS_KONTR_OUT as numeric(18,4)) as CrsKontrOut, " +
                              "DOC_DC as DocDC, " +
                              "DOC_ROW_CODE as DocRowCode, " +
                              "DOC_TYPE_CODE as DocTypeCode, " +
                              "cast(CRS_OPER_IN as numeric(18,4)) as CrsOperIn, " +
                              "cast(CRS_OPER_OUT as numeric(18,4)) as CrsOperOut, " +
                              "OPER_CRS_DC as CrsOperDC, " +
                              "cast(OPER_CRS_RATE as numeric(18,4)) as CrsOperRate, " +
                              "cast(UCH_CRS_RATE as numeric(18,4)) as CrsUchRate, " +
                              "KONTR_DC as DocCode, " +
                              "DOC_EXT_NUM as DocExtNum, " +
                              "ISNULL(sd_24.DD_NOTES, ISNULL(Sd_26.SF_NOTES, ISNULL(SD_33.NOTES_ORD, " +
                              "ISNULL(SD_34.NOTES_ORD, ISNULL(sd_84.SF_NOTE, ISNULL(td_101.VVT_DOC_NUM, " +
                              "ISNULL(td_110.VZT_DOC_NOTES, ISNULL(sd_430.ASV_NOTES,'')))))))) AS Notes " +
                              "FROM dbo.KONTR_BALANS_OPER_ARC kboa " +
                              "LEFT OUTER JOIN sd_24 ON DOC_DC = sd_24.DOC_CODE " +
                              "LEFT OUTER JOIN sd_26 ON DOC_DC = sd_26.DOC_CODE " +
                              "LEFT OUTER JOIN SD_33 ON DOC_DC = sd_33.DOC_CODE " +
                              "LEFT OUTER JOIN SD_34 ON DOC_DC = sd_34.DOC_CODE " +
                              "LEFT OUTER JOIN SD_84 ON DOC_DC = sd_84.DOC_CODE " +
                              "LEFT OUTER JOIN td_101 ON DOC_ROW_CODE = td_101.code " +
                              "LEFT OUTER JOIN td_110 ON DOC_DC = td_110.DOC_CODE AND kboa.DOC_ROW_CODE = td_110.code " +
                              "LEFT OUTER JOIN sd_430 ON doc_dc = sd_430.DOC_CODE " +
                              $"WHERE KONTR_DC = {CustomFormat.DecimalToSqlDecimal(dc)} " +
                              "ORDER BY kboa.DOC_DATE;";
                    var data1 = ent.Database.SqlQuery<KontragentBalansRowViewModel>(sql).ToList();
                    decimal sum = 0;
                    Operations.Clear();
                    var sn = data1.FirstOrDefault(_ => _.DocName == " На начало учета");
                    if (sn != null)
                    {
                        if (sn.CrsKontrIn > 0)
                            sum = -sn.CrsKontrIn;
                        else
                            sum = sn.CrsKontrOut;
                        //Operations.Add(KontragentBalansRowViewModel.DbToViewModel(sn, (decimal) Math.Round(sum, 2)));
                        Operations.Add(sn);
                        data1.Remove(sn);
                    }

                    foreach (var d in data1)
                    {
                        sum += d.CrsKontrOut - d.CrsKontrIn;
                        d.Nakopit = sum;
                        Operations.Add(d);
                    }

                    LastBalansSumma = Math.Round(sum, 2);
                    LastOperationDate = Operations.Count > 0 ? Operations.Max(_ => _.DocDate) : DateTime.MinValue;
                    var prjcts = ent.ProjectsDocs.Include(_ => _.Projects).ToList();
                    foreach (var o in Operations)
                        switch (o.DocName)
                        {
                            case "Акт в/з ":
                            case "Акт конвертации ":
                                var prj1 = prjcts.FirstOrDefault(_ => _.DocDC == o.DocDC && _.DocRowId == o.DocRowCode);
                                if (prj1 != null)
                                    o.Project = GlobalOptions.ReferencesCache.GetProject(prj1.Projects.Id) as Project;
                                break;
                            default:
                                var prj = prjcts.FirstOrDefault(_ => _.DocDC == o.DocDC);
                                if (prj != null)
                                    o.Project = GlobalOptions.ReferencesCache.GetProject(prj.Projects.Id) as Project;
                                break;
                        }
                }

                return Operations;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }
            finally
            {
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (myKontragent == null) return;
            CurrentPeriod = null;
            LoadOperations(myKontragent.DocCode);
        }

        #region Command

        public ICommand DocumentLinkToProjectCommand
        {
            get { return new Command(DocumentLinkToProject, _ => SelectedDocs != null && SelectedDocs.Count > 0); }
        }

        //public override void ResetLayout(object form)
        //{
        //    var curKontr = Kontragent;
        //    base.ResetLayout(form);
        //    Kontragent = curKontr;
        //}

        private void DocumentLinkToProject(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var prj = StandartDialogs.SelectProject();
                    if (prj == null) return;
                    foreach (var d in SelectedDocs)
                    {
                        switch (d.DocName)
                        {
                            case "Акт конвертации ":
                                ctx.ProjectsDocs.Add(new ProjectsDocs
                                {
                                    Id = Guid.NewGuid(),
                                    ProjectId = prj.Id,
                                    DocDC = d.DocDC,
                                    DocTypeName = "Акт конвертации",
                                    DocInfo = d.DocName,
                                    DocRowId = d.DocRowCode
                                });
                                d.Project = prj;
                                break;
                            case "Акт в/з ":
                                ctx.ProjectsDocs.Add(new ProjectsDocs
                                {
                                    Id = Guid.NewGuid(),
                                    ProjectId = prj.Id,
                                    DocDC = d.DocDC,
                                    DocTypeName = "Акт взаимозачета",
                                    DocInfo = d.DocName,
                                    DocRowId = d.DocRowCode
                                });
                                d.Project = prj;
                                break;
                            case "Банковская выписка":
                                ctx.ProjectsDocs.Add(new ProjectsDocs
                                {
                                    Id = Guid.NewGuid(),
                                    ProjectId = prj.Id,
                                    DocDC = d.DocDC,
                                    DocTypeName = "Банковская выписка",
                                    DocInfo = d.DocName,
                                    DocRowId = d.DocRowCode
                                });
                                d.Project = prj;
                                break;
                        }

                        if (d.DocName.Contains("С/Ф") &&
                            !d.DocName.ToUpper().Contains("УСЛУГИ")
                            && d.DocName.ToUpper().Contains("КЛИЕНТУ"))
                        {
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = prj.Id,
                                DocDC = d.DocDC,
                                DocTypeName = "Отгрузка товара",
                                DocInfo = d.DocName
                            });
                            d.Project = prj;
                        }

                        if (d.DocName.ToUpper().Contains("С/Ф") &&
                            d.DocName.ToUpper().Contains("ПОСТАВЩИКА")
                            && !d.DocName.ToUpper().Contains("УСЛУГИ"))
                        {
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = prj.Id,
                                DocDC = d.DocDC,
                                DocTypeName = "Услуги поставщиков",
                                DocInfo = d.DocName
                            });
                            d.Project = prj;
                        }

                        if (d.DocName.ToUpper().Contains("С/Ф") &&
                            d.DocName.ToUpper().Contains("ПОСТАВЩИКА")
                            && d.DocName.ToUpper().Contains("ТМЦ"))
                        {
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = prj.Id,
                                DocDC = d.DocDC,
                                DocTypeName = "Приход товара",
                                DocInfo = d.DocName
                            });
                            d.Project = prj;
                        }

                        if (d.DocName.ToUpper().Contains("УСЛУГИ")
                            && d.DocName.ToUpper().Contains("КЛИЕНТУ"))
                        {
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = prj.Id,
                                DocDC = d.DocDC,
                                DocTypeName = "Услуги клиенту",
                                DocInfo = d.DocName,
                                DocRowId = d.DocRowCode
                            });
                            d.Project = prj;
                        }

                        if (d.DocName.ToUpper().Contains("ПРИХОДНЫЙ")
                            && d.DocName.ToUpper().Contains("ОРДЕР"))
                        {
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = prj.Id,
                                DocDC = CurrentDocument.DocDC,
                                DocTypeName = "Кассовые документы",
                                DocInfo = CurrentDocument.DocName
                            });
                            d.Project = prj;
                        }

                        if (d.DocName.ToUpper().Contains("РАСХОДНЫЙ")
                            && d.DocName.ToUpper().Contains("ОРДЕР"))
                        {
                            ctx.ProjectsDocs.Add(new ProjectsDocs
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = prj.Id,
                                DocDC = d.DocDC,
                                DocTypeName = "Кассовые документы",
                                DocInfo = d.DocName
                            });
                            d.Project = prj;
                        }
                    }

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public ICommand DocumentUnLinkProjectCommand
        {
            get { return new Command(DocumentUnLinkProject, _ => SelectedDocs != null && SelectedDocs.Count > 0); }
        }

        private void DocumentUnLinkProject(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var d in SelectedDocs.Where(_ => _.Project != null))
                    {
                        var p = ctx.ProjectsDocs.FirstOrDefault(_ =>
                            _.ProjectId == d.Project.Id && _.DocDC == d.DocDC);
                        if (p != null) ctx.ProjectsDocs.Remove(p);
                        d.Project = null;
                    }

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public new Command DocumentOpenCommand
        {
            get { return new Command(DocumentOpen, _ => IsDocumentOpenAllow); }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null &&
                                                    DocumentsOpenManager.IsDocumentOpen(CurrentDocument.DocTypeCode);

        public override void DocumentOpen(object obj)
        {
            var WinManager = new WindowManager();
            if (CurrentDocument.DocTypeCode == DocumentType.Bank)
            {
                DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, CurrentDocument.DocRowCode);
                return;
            }

            if (CurrentDocument.DocTypeCode == DocumentType.AccruedAmountForClient ||
                CurrentDocument.DocTypeCode == DocumentType.AccruedAmountOfSupplier)
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var s = CurrentDocument.DocNum.Split('/');
                    var num = Convert.ToInt32(s[0]);
                    if (CurrentDocument.DocTypeCode == DocumentType.AccruedAmountForClient)
                    {
                        var doc = ctx.AccruedAmountForClient.FirstOrDefault(_ => _.DocInNum == num);
                        if (doc != null)
                            DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, 0, doc.Id);
                        else
                            WinManager.ShowWinUIMessageBox("Документ не найден.", "Сообщение");
                    }

                    if (CurrentDocument.DocTypeCode == DocumentType.AccruedAmountOfSupplier)
                    {
                        var doc = ctx.AccruedAmountOfSupplier.FirstOrDefault(_ => _.DocInNum == num);
                        if (doc != null)
                            DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, 0, doc.Id);
                        else
                            WinManager.ShowWinUIMessageBox("Документ не найден.", "Сообщение");
                    }
                }

                return;
            }

            DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, CurrentDocument.DocDC);
        }

        public ICommand SetAllPeriodsCommand
        {
            get { return new Command(SetAllPeriods, _ => true); }
        }

        private void SetAllPeriods(object obj)
        {
            IsPeriodsEnabled = !IsPeriodsEnabled;
            PeriodModeName = IsPeriodsEnabled ? "Показать по всем периодам" : "Показать по выбранному периоду";
            if (IsPeriodsEnabled)
                LoadDocumentsForPeriod();
            else
                LoadAllDocuments();
        }

        public ICommand PeriodTreeClickCommand
        {
            get { return new Command(PeriodTreeClick, _ => true); }
        }

        private void PeriodTreeClick(object obj)
        {
            IsPeriodsEnabled = true;
            LoadDocumentsForPeriod();
            PeriodModeName = "Показать по всем периодам";
        }

        private bool myIsPeriodsEnabled = true;

        public bool IsPeriodsEnabled
        {
            get => myIsPeriodsEnabled;
            set
            {
                if (myIsPeriodsEnabled == value) return;
                myIsPeriodsEnabled = value;
                RaisePropertyChanged();
            }
        }

        private string myPeriodModeName = "Показать по всем периодам";

        public string PeriodModeName
        {
            get => myPeriodModeName;
            set
            {
                if (myPeriodModeName == value) return;
                myPeriodModeName = value;
                RaisePropertyChanged();
            }
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            var frm = Form as KontragentBalansView;
            if (frm == null) return;
            frm.treeListPeriodView.ShowTotalSummary = false;
            var nakSummaries = new List<GridSummaryItem>();
            foreach (var s in frm.KontrOperGrid.TotalSummary)
            {
                if (s.FieldName == "Nakopit" || s.FieldName == "CrsOperRate")
                    nakSummaries.Add(s);
            }
            
            foreach (var c in nakSummaries) frm.KontrOperGrid.TotalSummary.Remove(c);
        }

        #endregion
    }

    [MetadataType(typeof(DataAnnotationKontragentPeriod))]
    public class KontragentPeriod : DatePeriod
    {
        [DataMember] public decimal EndPeriodSumma { set; get; }
    }

    public class DataAnnotationKontragentPeriod : DataAnnotationForFluentApiBase, IMetadataProvider<KontragentPeriod>
    {
        void IMetadataProvider<KontragentPeriod>.BuildMetadata(MetadataBuilder<KontragentPeriod> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.PeriodType).NotAutoGenerated();
            //builder.Property(_ => _.DateEnd).NotAutoGenerated().DisplayName("Конец");
            //builder.Property(_ => _.DateStart).NotAutoGenerated().DisplayName("Начало");
            builder.Property(_ => _.EndPeriodSumma).AutoGenerated().LocatedAt(1).ReadOnly().DisplayName("Сумма на конец").DisplayFormatString("n2");
            builder.Property(_ => _.Name).AutoGenerated().LocatedAt(0).ReadOnly().DisplayName("Период");
               
        }
    }
}
