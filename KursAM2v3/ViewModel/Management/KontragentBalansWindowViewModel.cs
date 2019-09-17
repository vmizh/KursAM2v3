using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.ViewModel.Management
{
    [DataContract]
    public class KontragentBalansWindowViewModel : RSWindowViewModelBase
    {
        private KonragentBalansRowViewModel myCurrentDocument;
        private KontragentPeriod myCurrentPeriod;
        private Kontragent myKontragent;
        private decimal myLastBalansSumma;
        private DateTime myLastOperationDate;

        public KontragentBalansWindowViewModel()
        {
            Operations = new ObservableCollection<KonragentBalansRowViewModel>();
            Periods = new ObservableCollection<KontragentPeriod>();
            Documents = new ObservableCollection<KonragentBalansRowViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
        }

        public KontragentBalansWindowViewModel(decimal doccode) : this()
        {
            StartKontragent = Kontragents.Single(_ => _.DOC_CODE == doccode);
        }

        public ObservableCollection<KonragentBalansRowViewModel> SelectedDocs { set; get; } =
            new ObservableCollection<KonragentBalansRowViewModel>();

        public Kontragent StartKontragent { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<KonragentBalansRowViewModel> Documents { set; get; }

        public KontragentPeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (myCurrentPeriod != null && myCurrentPeriod.Equals(value)) return;
                myCurrentPeriod = value;
                if (myCurrentPeriod != null)
                    LoadDocumentsForPeriod();
                else
                    Documents.Clear();
                RaisePropertyChanged();
            }
        }

        public KonragentBalansRowViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsDocumentOpenAllow));
            }
        }

        public List<Kontragent> Kontragents => KontragentManager.GetAllKontragentSortedByUsed();

        [DataMember]
        public Kontragent Kontragent
        {
            get => myKontragent != null ? MainReferences.GetKontragent(myKontragent.DOC_CODE) : null;
            set
            {
                if (myKontragent != null && myKontragent.Equals(value)) return;
                myKontragent = value;
                if (myKontragent != null)
                {
                    KontragentManager.UpdateSelectCount(myKontragent.DOC_CODE);
                    LoadOperations(myKontragent.DOC_CODE);
                    RaisePropertyChanged(nameof(Kontragents));
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Periods));
                RaisePropertyChanged(nameof(CrsName));
                RaisePropertyChanged(nameof(LastBalansSumma));
                RaisePropertyChanged(nameof(LastOperationDate));
            }
        }

        public string CrsName => Kontragent != null && Kontragent.BalansCurrency != null
            ? Kontragent.BalansCurrency.Name
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

        [DataMember] public ObservableCollection<KonragentBalansRowViewModel> Operations { set; get; }

        [DataMember] public ObservableCollection<KontragentPeriod> Periods { set; get; }

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

        private void LoadPeriod(IEnumerable<KonragentBalansRowViewModel> rows)
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

        public ObservableCollection<KonragentBalansRowViewModel> Load(decimal dc)
        {
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    if (!MainReferences.AllKontragents.Keys.Contains(dc))
                        throw new Exception($"Контрагент {dc} не найден. RecalcKontragentBalans.CalcBalans");
                    RecalcKontragentBalans.CalcBalans(dc,
                        MainReferences.GetKontragent(dc).START_BALANS ?? new DateTime(2000, 1, 1));
                    var data =
                        ent.KONTR_BALANS_OPER_ARC.AsNoTracking()
                            .Where(_ => _.KONTR_DC == dc)
                            .OrderBy(_ => _.DOC_DATE)
                            .ToList();
                    double sum = 0;
                    Operations.Clear();
                    var sn = data.FirstOrDefault(_ => _.DOC_NAME == " На начало учета");
                    if (sn != null)
                    {
                        if ((decimal) sn.CRS_KONTR_IN == 0)
                            sum = sn.CRS_KONTR_OUT;
                        else
                            sum = -sn.CRS_KONTR_IN;
                        Operations.Add(KonragentBalansRowViewModel.DbToViewModel(sn, (decimal) Math.Round(sum, 2)));
                        data.Remove(sn);
                    }

                    foreach (var d in data)
                    {
                        sum += d.CRS_KONTR_OUT - d.CRS_KONTR_IN;
                        Operations.Add(KonragentBalansRowViewModel.DbToViewModel(d, (decimal) Math.Round(sum, 2)));
                    }

                    LastBalansSumma = (decimal) Math.Round(sum, 2);
                    LastOperationDate = Operations.Count > 0 ? Operations.Max(_ => _.DocDate) : DateTime.MinValue;
                    var prjcts = ent.ProjectsDocs.Include(_ => _.Projects).ToList();
                    foreach (var o in Operations)
                        switch (o.DocName)
                        {
                            case "Акт в/з ":
                            case "Акт конвертации ":
                                var prj1 = prjcts.FirstOrDefault(_ => _.DocDC == o.DocDC && _.DocRowId == o.DocRowCode);
                                if (prj1 != null)
                                    o.Project = MainReferences.Projects[prj1.Projects.Id];
                                break;
                            default:
                                var prj = prjcts.FirstOrDefault(_ => _.DocDC == o.DocDC);
                                if (prj != null)
                                    o.Project = MainReferences.Projects[prj.Projects.Id];
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
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (myKontragent == null) return;
            CurrentPeriod = null;
            LoadOperations(myKontragent.DOC_CODE);
        }

        #region Command

        public ICommand DocumentLinkToProjectCommand
        {
            get { return new Command(DocumentLinkToProject, _ => SelectedDocs != null && SelectedDocs.Count > 0); }
        }

        public override void ResetLayout(object form)
        {
            var curKontr = Kontragent;
            base.ResetLayout(form);
            Kontragent = curKontr;
        }

        private void setPrihod(decimal dc)
        {
        }

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
            get { return new Command(DocumentOpen, param => IsDocumentOpenAllow); }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null &&
                                                    DocumentsOpenManager.IsDocumentOpen(CurrentDocument.DocTypeCode);

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument.DocTypeCode == DocumentType.Bank)
            {
                DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, CurrentDocument.DocRowCode);
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

        #endregion
    }

    public class KontragentPeriod : DatePeriod
    {
        [DataMember] public decimal EndPeriodSumma { set; get; }
    }
}