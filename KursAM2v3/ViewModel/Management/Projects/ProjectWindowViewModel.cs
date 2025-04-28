using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Management;
using KursAM2.ViewModel.Period;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Currency;
using KursDomain.Documents.Management;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.Projects;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ProjectWindowViewModel2 : RSWindowViewModelBase
{
    #region Вспомогательные

    private void SetUserRight()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            IsCanOpenProjectsReference =
                ctx.USER_FORMS_RIGHT.Any(_ => _.USER_NAME == GlobalOptions.UserInfo.NickName && _.FORM_ID == 39);
        }
    }

    #endregion

    #region Fields

    private bool myIsAllProjects;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IsCanOpenProjectsReference { get; private set; }

    #endregion

    #region Constructors

    public ProjectWindowViewModel2()
    {
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        SetUserRight();
        IsAllProjects = false;
        // ReSharper disable once VirtualMemberCallInConstructor
        RefreshData(null);
    }

    public ProjectWindowViewModel2(Window form)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Form = form;
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        SetUserRight();
        IsAllProjects = false;
        // ReSharper disable once VirtualMemberCallInConstructor
        RefreshData(null);
    }

    #endregion

    #region Properties

    public bool IsAllProjects
    {
        get => myIsAllProjects;
        set
        {
            if (myIsAllProjects == value) return;
            myIsAllProjects = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<MultyCurrenciesDatePeriod> Periods { set; get; } =
        new ObservableCollection<MultyCurrenciesDatePeriod>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public ObservableCollection<ProjectResultInfo> Projects { set; get; } =
        new ObservableCollection<ProjectResultInfo>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public ObservableCollection<ProjectDocumentViewModel> Documents { set; get; } =
        new ObservableCollection<ProjectDocumentViewModel>();

    #endregion

    #region Commands

    public ICommand ProjectsReferenceOpenCommand
    {
        get { return new Command(ProjectsReferenceOpen, _ => true); }
    }

    private void ProjectsReferenceOpen(object obj)
    {
        DocumentsOpenManager.Open(DocumentType.ProjectsReference, 0);
    }

    public override void RefreshData(object obj)
    {
        Documents.Clear();
        Projects.Clear();
        Periods = new ObservableCollection<MultyCurrenciesDatePeriod>();
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.ProjectsDocuments.ToList();
                foreach (var d in data)
                {
                    var doc = new ProjectDocumentViewModel
                    {
                        CurrencyDC = d.CurrencyDC ?? 0,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC) as Currency,
                        DocCode = d.DocDC,
                        DocRowId = d.DocRowId,
                        DocInNum = d.DocInNum,
                        DocName = d.DocName,
                        DocNum = d.DocNum,
                        Employee = GlobalOptions.ReferencesCache.GetEmployee(d.EmployeeTN) as Employee,
                        EmployeeTN = d.EmployeeTN,
                        KontragentDC = d.KontragentDC,
                        Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent,
                        NomenklDC = d.NomenklDC,
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as Nomenkl,
                        DocType = (DocumentType)d.DocType,
                        ProjectId = d.ProjectId,
                        DocDate = d.DocDate ?? DateTime.MinValue,
                        Sum = d.Sum,
                        ConfirmedSum = d.ConfirmedSum ?? 0
                    };
                    // ReSharper disable once PossibleNullReferenceException
                    switch (doc.Currency.DocCode)
                    {
                        case CurrencyCode.SEK:
                            doc.ProfitSEK = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossSEK = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerSEK = d.DilerSum;
                            break;
                        case CurrencyCode.USD:
                            doc.ProfitUSD = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossUSD = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerUSD = d.DilerSum;
                            break;
                        case CurrencyCode.EUR:
                            doc.ProfitEUR = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossEUR = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerEUR = d.DilerSum;
                            break;
                        case CurrencyCode.RUB:
                            doc.ProfitRUB = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossRUB = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerRUB = d.DilerSum;
                            break;
                        case CurrencyCode.CHF:
                            doc.ProfitCHF = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossCHF = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerCHF = d.DilerSum;
                            break;
                        case CurrencyCode.GBP:
                            doc.ProfitGBP = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossGBP = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerGBP = d.DilerSum;
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex, "Ошибка в методе RefreshData");
        }

        RaisePropertyChanged(nameof(Projects));
    }

    #endregion
}

public class ProjectWindowViewModel : RSWindowViewModelBase
{
    private readonly ProjectManager manager = new ProjectManager();
    private ProjectDocumentViewModel myCurrentDocument;
    private MultyCurrenciesDatePeriod myCurrentPeriod;
    private ProjectResultInfo myCurrentProject;
    private bool myIsAllPeriods;
    private bool myIsAllProjects;
    private bool myIsEnableTreeList = true;

    public ProjectWindowViewModel()
    {
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        SetUserRight();
        IsAllProjects = false;
        Projects.Add(new ProjectResultInfo());
        Periods.Add(new MultyCurrenciesDatePeriod());
        // ReSharper disable once VirtualMemberCallInConstructor
        //RefreshData(null);
    }

    public ProjectWindowViewModel(Window form)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Form = form;
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        SetUserRight();
        IsAllProjects = false;
        // ReSharper disable once VirtualMemberCallInConstructor
        RefreshData(null);
    }

    public ICommand ProjectsProjectProfitAndLossCommand
    {
        get { return new Command(ProjectsProjectProfitAndLoss, _ => CurrentProject != null); }
    }

    public ObservableCollection<ProjectDocumentViewModel> SelectedDocs { set; get; } =
        new ObservableCollection<ProjectDocumentViewModel>();

    public ObservableCollection<ProjectPrihodRowViewModel> PrihodDocuments { set; get; } =
        new ObservableCollection<ProjectPrihodRowViewModel>();

    public ObservableCollection<ProjectResultInfo> Projects { set; get; } =
        new ObservableCollection<ProjectResultInfo>();

    public ObservableCollection<MultyCurrenciesDatePeriod> Periods { set; get; } =
        new ObservableCollection<MultyCurrenciesDatePeriod>();

    public ObservableCollection<ProjectDocumentViewModel> AllDocuments { set; get; } =
        new ObservableCollection<ProjectDocumentViewModel>();

    public ObservableCollection<ProjectDocumentViewModel> AllProjectDocuments { set; get; } =
        new ObservableCollection<ProjectDocumentViewModel>();

    public ObservableCollection<ProjectDocumentViewModel> Documents { set; get; } =
        new ObservableCollection<ProjectDocumentViewModel>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public List<DilerRow> DilerDocCollection { set; get; } = new List<DilerRow>();

    public ProjectResultInfo CurrentProject
    {
        get => myCurrentProject;
        set
        {
            if (Equals(myCurrentProject, value)) return;
            myCurrentProject = value;
            Periods.Clear();
            Documents.Clear();
            PrihodDocuments.Clear();
            if (myCurrentProject == null) return;
            LoadDocumentForProject(myCurrentProject.Id);
            if (myCurrentProject == null) return;
            var docs = AllProjectDocuments.Where(_ =>
                _.DocDate >= CurrentProject.DateStart && _.DocDate <= (CurrentProject.DateEnd ?? DateTime.Today)
                                                      && _.ConfirmedSum != 0 && _.Sum != 0).ToList();
            if (!docs.Any()) return;
            var periods = DatePeriod.GenerateIerarhy(
                docs.Min(_ => _.DocDate), docs.Max(_ => _.DocDate),
                PeriodIerarhy.YearMonth);
            foreach (var p in periods.OrderByDescending(_ => _.DateEnd))
            {
                var newper = new MultyCurrenciesDatePeriod(p);
                foreach (var d in docs.Where(_ => _.DocDate >= p.DateStart && _.DocDate <= p.DateEnd))
                    switch (d.Currency.DocCode)
                    {
                        case CurrencyCode.SEK:
                            newper.ProfitSEK += d.ProfitSEK;
                            newper.LossSEK += d.LossSEK;
                            newper.DilerSEK += d.DilerSEK;
                            newper.ResultSEK += d.ProfitSEK - d.LossSEK - d.DilerSEK;
                            break;
                        case CurrencyCode.USD:
                            newper.ProfitUSD += d.ProfitUSD;
                            newper.LossUSD += d.LossUSD;
                            newper.DilerUSD += d.DilerUSD;
                            newper.ResultUSD += d.ProfitUSD - d.LossUSD - d.DilerUSD;
                            break;
                        case CurrencyCode.EUR:
                            newper.ProfitEUR += d.ProfitEUR;
                            newper.LossEUR += d.LossEUR;
                            newper.DilerEUR += d.DilerEUR;
                            newper.ResultEUR += d.ProfitEUR - d.LossEUR - d.DilerEUR;
                            break;
                        case CurrencyCode.RUB:
                            newper.ProfitRUB += d.ProfitRUB;
                            newper.LossRUB += d.LossRUB;
                            newper.DilerRUB += d.DilerRUB;
                            newper.ResultRUB += d.ProfitRUB - d.LossRUB - d.DilerRUB;
                            break;
                        case CurrencyCode.CHF:
                            newper.ProfitCHF += d.ProfitCHF;
                            newper.LossCHF += d.LossCHF;
                            newper.DilerCHF += d.DilerCHF;
                            newper.ResultCHF += d.ProfitCHF - d.LossCHF - d.DilerCHF;
                            break;
                        case CurrencyCode.GBP:
                            newper.ProfitGBP += d.ProfitGBP;
                            newper.LossGBP += d.LossGBP;
                            newper.DilerGBP += d.DilerGBP;
                            newper.ResultGBP += d.ProfitGBP - d.LossGBP - d.DilerGBP;
                            break;
                    }

                if (newper.IsZero()) continue;
                Periods.Add(newper);
            }

            var frm = Form as ProjectsView;
            MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridProjects, Projects);
            MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.treePeriods, Periods);
            MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridDocuments, Documents);
            MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridExtend, PrihodDocuments);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Periods));
            RaisePropertyChanged(nameof(Documents));
            RaisePropertyChanged(nameof(PrihodDocuments));
        }
    }

    public bool IsAllProjects
    {
        get => myIsAllProjects;
        set
        {
            if (myIsAllProjects == value) return;
            myIsAllProjects = value;
            RaisePropertyChanged();
        }
    }

    public ProjectDocumentViewModel CurrentDocument
    {
        get => myCurrentDocument;
        set
        {
            if (Equals(myCurrentDocument, value)) return;
            myCurrentDocument = value;
            if (myCurrentDocument != null)
            {
                loadDocList();
                var frm = Form as ProjectsView;
                MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridExtend, PrihodDocuments);
            }

            RaisePropertyChanged();
            RaisePropertyChanged(nameof(PrihodDocuments));
        }
    }

    public MultyCurrenciesDatePeriod CurrentPeriod
    {
        get => myCurrentPeriod;
        set
        {
            if (Equals(myCurrentPeriod, value)) return;
            PrihodDocuments.Clear();
            myCurrentPeriod = value;
            if (myCurrentPeriod == null) return;
            GetDocumentsInCollection();
            var frm = Form as ProjectsView;
            MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridDocuments, Documents);
            MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridExtend, PrihodDocuments);
            RaisePropertyChanged();
        }
    }

    public bool IsAllPeriods
    {
        get => myIsAllPeriods;
        set
        {
            if (myIsAllPeriods == value) return;
            myIsAllPeriods = value;
            IsEnableTreeList = !myIsAllPeriods;
            RaisePropertyChanged();
        }
    }

    public bool IsEnableTreeList
    {
        get => myIsEnableTreeList;
        set
        {
            if (myIsEnableTreeList == value) return;
            myIsEnableTreeList = value;
            if (myIsAllPeriods)
            {
                Documents.Clear();
                GetDocumentsInCollection();
                RaisePropertyChanged(nameof(Documents));
            }

            RaisePropertyChanged();
        }
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private bool IsCanOpenProjectsReference { set; get; }

    public ICommand ProjectsReferenceOpenCommand
    {
        get { return new Command(ProjectsReferenceOpen, _ => true); }
    }

    public new Command DocumentOpenCommand
    {
        get { return new Command(DocumentOpen, _ => IsDocumentOpenAllow); }
    }

    public override bool IsDocumentOpenAllow => CurrentDocument != null &&
                                                DocumentsOpenManager.IsDocumentOpen(CurrentDocument.DocType);

    private void updatePeriodSum()
    {
        var curP = CurrentPeriod;
        Periods.Clear();
        if (curP == null) return;
        var docs = AllProjectDocuments.Where(_ =>
            _.DocDate >= CurrentProject.DateStart && _.DocDate <= (CurrentProject.DateEnd ?? DateTime.Today)
                                                  && _.ConfirmedSum != 0 && _.Sum != 0).ToList();
        if (!docs.Any()) return;
        var periods = DatePeriod.GenerateIerarhy(
            docs.Min(_ => _.DocDate), docs.Max(_ => _.DocDate),
            PeriodIerarhy.YearMonth);
        foreach (var p in periods.OrderByDescending(_ => _.DateEnd))
        {
            var newper = new MultyCurrenciesDatePeriod(p);
            foreach (var d in docs.Where(_ => _.DocDate >= p.DateStart && _.DocDate <= p.DateEnd))
                switch (d.Currency.DocCode)
                {
                    case CurrencyCode.SEK:
                        newper.ProfitSEK += d.ProfitSEK;
                        newper.LossSEK += d.LossSEK;
                        newper.DilerSEK += d.DilerSEK;
                        newper.ResultSEK += d.ProfitSEK - d.LossSEK - d.DilerSEK;
                        break;
                    case CurrencyCode.USD:
                        newper.ProfitUSD += d.ProfitUSD;
                        newper.LossUSD += d.LossUSD;
                        newper.DilerUSD += d.DilerUSD;
                        newper.ResultUSD += d.ProfitUSD - d.LossUSD - d.DilerUSD;
                        break;
                    case CurrencyCode.EUR:
                        newper.ProfitEUR += d.ProfitEUR;
                        newper.LossEUR += d.LossEUR;
                        newper.DilerEUR += d.DilerEUR;
                        newper.ResultEUR += d.ProfitEUR - d.LossEUR - d.DilerEUR;
                        break;
                    case CurrencyCode.RUB:
                        newper.ProfitRUB += d.ProfitRUB;
                        newper.LossRUB += d.LossRUB;
                        newper.DilerRUB += d.DilerRUB;
                        newper.ResultRUB += d.ProfitRUB - d.LossRUB - d.DilerRUB;
                        break;
                    case CurrencyCode.CHF:
                        newper.ProfitCHF += d.ProfitCHF;
                        newper.LossCHF += d.LossCHF;
                        newper.DilerCHF += d.DilerCHF;
                        newper.ResultCHF += d.ProfitCHF - d.LossCHF - d.DilerCHF;
                        break;
                    case CurrencyCode.GBP:
                        newper.ProfitGBP += d.ProfitGBP;
                        newper.LossGBP += d.LossGBP;
                        newper.DilerGBP += d.DilerGBP;
                        newper.ResultGBP += d.ProfitGBP - d.LossGBP - d.DilerGBP;
                        break;
                }

            if (newper.IsZero()) continue;
            Periods.Add(newper);
            CurrentPeriod = Periods.FirstOrDefault(_ =>
                _.PeriodType == curP.PeriodType && _.DateStart == curP.DateStart && _.DateEnd == curP.DateEnd);
            RaisePropertyChanged(nameof(Periods));
        }
    }

    private void ProjectsProjectProfitAndLoss(object obj)
    {
        //var ctx2 = new ProjectProfitAndLossesWindowViewModel();
        //if (CurrentProject != null)
        //    ctx2.CurrentProject = CurrentProject;
        //var form = new ProjectProfitAndLossView
        //{
        //    Owner = Application.Current.MainWindow,
        //    DataContext = ctx2
        //};
        //ctx2.Form = form;
        //form.Show();
    }

    private void loadDocList()
    {
        PrihodDocuments.Clear();
        if (CurrentDocument.DocType != DocumentType.InvoiceClient &&
            CurrentDocument.DocType != DocumentType.InvoiceProvider &&
            CurrentDocument.DocType != DocumentType.MutualAccounting) return;
        foreach (var d in AllDocuments.Where(_ => _.DocCode == CurrentDocument.DocCode))
            PrihodDocuments.Add(new ProjectPrihodRowViewModel
            {
                DocCode = CurrentDocument.DocCode,
                NomenklName = d.Nomenkl?.Name,
                NomenklNumber = d.Nomenkl?.NomenklNumber,
                Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK + d.ResultUSD,
                LossCHF = d.CurrencyDC == CurrencyCode.CHF
                    ? d.ResultCHF
                    : 0,
                LossEUR = d.CurrencyDC == CurrencyCode.EUR
                    ? d.ResultEUR
                    : 0,
                LossGBP = d.CurrencyDC == CurrencyCode.GBP
                    ? d.ResultGBP
                    : 0,
                LossRUB = d.CurrencyDC == CurrencyCode.RUB
                    ? d.ResultRUB
                    : 0,
                LossSEK = d.CurrencyDC == CurrencyCode.SEK
                    ? d.ResultSEK
                    : 0,
                LossUSD = d.CurrencyDC == CurrencyCode.USD
                    ? d.ResultUSD
                    : 0
            });
    }

    private void LoadDocumentForProject(Guid projectId)
    {
        AllProjectDocuments.Clear();
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.Database
                    .SqlQuery<ProjectDocument>(
                        $"SELECT * FROM ProjectsDocuments where ProjectId = '{CustomFormat.GuidToSqlString(projectId)}'")
                    .ToList();
                foreach (var d in data)
                {
                    // ReSharper disable once RedundantAssignment
                    decimal anotherConfirmedSum = 0;
                    switch (d.DocType)
                    {
                        case DocumentType.Bank:
                        case DocumentType.CashIn:
                        case DocumentType.CashOut:
                            var ss = ctx.ProjectsDocs
                                .Where(_ => _.ProjectId != CurrentProject.Id && _.DocDC == d.DocDC &&
                                            _.DocRowId == d.DocRowId).AsNoTracking();
                            if (ss.Any())
                                anotherConfirmedSum = (decimal)ss.Sum(s => s.FactCRSSumma);
                            else
                                anotherConfirmedSum = 0;
                            break;
                        default:
                            anotherConfirmedSum = 0;
                            break;
                    }

                    var doc = new ProjectDocumentViewModel
                    {
                        Id = d.Id,
                        CurrencyDC = d.CurrencyDC,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC) as Currency,
                        DocCode = d.DocDC,
                        DocRowId = d.DocRowId,
                        DocInNum = d.DocInNum,
                        DocName = d.DocName,
                        DocNum = d.DocNum,
                        Employee = GlobalOptions.ReferencesCache.GetEmployee(d.EmployeeTN) as Employee,
                        EmployeeTN = d.EmployeeTN,
                        KontragentDC = d.KontragentDC,
                        Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent,
                        NomenklDC = d.NomenklDC,
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as Nomenkl,
                        DocType = d.DocType,
                        ProjectId = d.ProjectId,
                        DocDate = d.DocDate,
                        WarehouseDocDC = d.WarehouseDocDC,
                        WarehouseCode = d.WarehouseCode,
                        Sum = d.Sum,
                        ConfirmedSum = d.ConfirmedSum ?? 0,
                        ProfitOrLossType = d.TypeProfitAndLossCalc == 0
                            ? TypeProfitAndLossCalc.IsProfit
                            : TypeProfitAndLossCalc.IsLoss,
                        AnotherDocConfirmedSum = anotherConfirmedSum
                    };
                    switch (doc.Currency.DocCode)
                    {
                        case CurrencyCode.SEK:
                            doc.ProfitSEK = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossSEK = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerSEK = d.DilerSum ?? 0;
                            break;
                        case CurrencyCode.USD:
                            doc.ProfitUSD = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossUSD = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerUSD = d.DilerSum ?? 0;
                            break;
                        case CurrencyCode.EUR:
                            doc.ProfitEUR = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossEUR = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerEUR = d.DilerSum ?? 0;
                            break;
                        case CurrencyCode.RUB:
                            doc.ProfitRUB = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossRUB = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerRUB = d.DilerSum ?? 0;
                            break;
                        case CurrencyCode.CHF:
                            doc.ProfitCHF = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossCHF = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerCHF = d.DilerSum ?? 0;
                            break;
                        case CurrencyCode.GBP:
                            doc.ProfitGBP = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossGBP = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerGBP = d.DilerSum ?? 0;
                            break;
                    }

                    AllProjectDocuments.Add(doc);
                }

                calcCurrentProjectSum(CurrentProject);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex, "Ошибка в методе RefreshData");
        }
    }

    private void calcCurrentProjectSum(ProjectResultInfo p)
    {
        MultyCurrencyHelper.SetCurrencyValueToZero(p);
        foreach (var d in AllProjectDocuments)
            switch (d.Currency.DocCode)
            {
                case CurrencyCode.SEK:
                    CurrentProject.ProfitSEK += d.ProfitSEK;
                    CurrentProject.LossSEK += d.LossSEK;
                    CurrentProject.DilerSEK += d.DilerSEK;
                    break;
                case CurrencyCode.USD:
                    CurrentProject.ProfitUSD += d.ProfitUSD;
                    CurrentProject.LossUSD += d.LossUSD;
                    CurrentProject.DilerUSD += d.DilerUSD;
                    break;
                case CurrencyCode.EUR:
                    CurrentProject.ProfitEUR += d.ProfitEUR;
                    CurrentProject.LossEUR += d.LossEUR;
                    CurrentProject.DilerEUR += d.DilerEUR;
                    break;
                case CurrencyCode.RUB:
                    CurrentProject.ProfitRUB += d.ProfitRUB;
                    CurrentProject.LossRUB += d.LossRUB;
                    CurrentProject.DilerRUB += d.DilerRUB;
                    break;
                case CurrencyCode.CHF:
                    CurrentProject.ProfitCHF += d.ProfitCHF;
                    CurrentProject.LossCHF += d.LossCHF;
                    CurrentProject.DilerCHF += d.DilerCHF;
                    break;
                case CurrencyCode.GBP:
                    CurrentProject.ProfitGBP += d.ProfitGBP;
                    CurrentProject.LossGBP += d.LossGBP;
                    CurrentProject.DilerGBP += d.DilerGBP;
                    break;
            }
    }

    public override void RefreshData(object obj)
    {
        Periods.Clear();
        Projects.Clear();
        PrihodDocuments.Clear();
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.Database.SqlQuery<ProjectDocument>("SELECT * FROM ProjectsDocuments").ToList();
                foreach (var d in data)
                {
                    // ReSharper disable once RedundantAssignment
                    decimal anotherConfirmedSum = 0;
                    switch (d.DocType)
                    {
                        case DocumentType.Bank:
                        case DocumentType.CashIn:
                        case DocumentType.CashOut:
                            anotherConfirmedSum = (decimal)ctx.ProjectsDocs
                                .Where(_ => _.DocDC == d.DocDC &&
                                            _.DocRowId == d.DocRowId)
                                .Sum(s => s.FactCRSSumma);
                            break;
                        default:
                            anotherConfirmedSum = 0;
                            break;
                    }

                    var doc = new ProjectDocumentViewModel
                    {
                        Id = d.Id,
                        CurrencyDC = d.CurrencyDC,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC) as Currency,
                        DocCode = d.DocDC,
                        DocRowId = d.DocRowId,
                        DocInNum = d.DocInNum,
                        DocName = d.DocName,
                        DocNum = d.DocNum,
                        Employee = GlobalOptions.ReferencesCache.GetEmployee(d.EmployeeTN) as Employee,
                        EmployeeTN = d.EmployeeTN,
                        KontragentDC = d.KontragentDC,
                        Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent,
                        NomenklDC = d.NomenklDC,
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as Nomenkl,
                        DocType = d.DocType,
                        ProjectId = d.ProjectId,
                        DocDate = d.DocDate,
                        WarehouseDocDC = d.WarehouseDocDC,
                        WarehouseCode = d.WarehouseCode,
                        Sum = d.Sum,
                        ConfirmedSum = d.ConfirmedSum ?? 0,
                        ProfitOrLossType = d.TypeProfitAndLossCalc == 0
                            ? TypeProfitAndLossCalc.IsProfit
                            : TypeProfitAndLossCalc.IsLoss,
                        AnotherDocConfirmedSum = anotherConfirmedSum
                    };
                    switch (doc.Currency.DocCode)
                    {
                        case CurrencyCode.SEK:
                            doc.ProfitSEK = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossSEK = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerSEK = (decimal)d.DilerSum;
                            break;
                        case CurrencyCode.USD:
                            doc.ProfitUSD = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossUSD = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerUSD = (decimal)d.DilerSum;
                            break;
                        case CurrencyCode.EUR:
                            doc.ProfitEUR = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossEUR = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerEUR = (decimal)d.DilerSum;
                            break;
                        case CurrencyCode.RUB:
                            doc.ProfitRUB = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossRUB = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerRUB = (decimal)d.DilerSum;
                            break;
                        case CurrencyCode.CHF:
                            doc.ProfitCHF = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossCHF = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerCHF = (decimal)d.DilerSum;
                            break;
                        case CurrencyCode.GBP:
                            doc.ProfitGBP = d.TypeProfitAndLossCalc == 0 ? d.ConfirmedSum ?? 0 : 0;
                            doc.LossGBP = d.TypeProfitAndLossCalc == 1 ? d.ConfirmedSum ?? 0 : 0;
                            doc.DilerGBP = (decimal)d.DilerSum;
                            break;
                    }

                    AllDocuments.Add(doc);
                }
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex, "Ошибка в методе RefreshData");
        }

        foreach (var p in manager.LoadProjects())
        {
            foreach (var d in AllDocuments.Where(_ => _.ProjectId == p.Id))
                switch (d.Currency.DocCode)
                {
                    case CurrencyCode.SEK:
                        p.ProfitSEK += d.ProfitSEK;
                        p.LossSEK += d.LossSEK;
                        p.DilerSEK += d.DilerSEK;
                        break;
                    case CurrencyCode.USD:
                        p.ProfitUSD += d.ProfitUSD;
                        p.LossUSD += d.LossUSD;
                        p.DilerUSD += d.DilerUSD;
                        break;
                    case CurrencyCode.EUR:
                        p.ProfitEUR += d.ProfitEUR;
                        p.LossEUR += d.LossEUR;
                        p.DilerEUR += d.DilerEUR;
                        break;
                    case CurrencyCode.RUB:
                        p.ProfitRUB += d.ProfitRUB;
                        p.LossRUB += d.LossRUB;
                        p.DilerRUB += d.DilerRUB;
                        break;
                    case CurrencyCode.CHF:
                        p.ProfitCHF += d.ProfitCHF;
                        p.LossCHF += d.LossCHF;
                        p.DilerCHF += d.DilerCHF;
                        break;
                    case CurrencyCode.GBP:
                        p.ProfitGBP += d.ProfitGBP;
                        p.LossGBP += d.LossGBP;
                        p.DilerGBP += d.DilerGBP;
                        break;
                }

            Projects.Add(p);
        }

        RaisePropertyChanged(nameof(Projects));
        CurrentPeriod = null;
        CurrentDocument = null;
        Documents.Clear();
        var frm = Form as ProjectsView;
        MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridProjects, Projects);
        MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.treePeriods, Periods);
        MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridDocuments, Documents);
        MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridExtend, PrihodDocuments);
        RaisePropertyChanged(nameof(Periods));
        RaisePropertyChanged(nameof(Documents));
        RaisePropertyChanged(nameof(PrihodDocuments));
    }

    protected override void DocumentOpen(object obj)
    {
        DocumentsOpenManager.Open(CurrentDocument.DocType, CurrentDocument.DocCode);
    }

    private void SetUserRight()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            IsCanOpenProjectsReference =
                ctx.USER_FORMS_RIGHT.Any(_ => _.USER_NAME == GlobalOptions.UserInfo.NickName && _.FORM_ID == 39);
        }
    }

    private void ProjectsReferenceOpen(object obj)
    {
        DocumentsOpenManager.Open(DocumentType.ProjectsReference, 0);
    }

    #region Command

    public ICommand AddDocumentLinkCommand
    {
        get { return new Command(AddDocumentLink, _ => CurrentProject != null); }
    }

    private void AddDocumentLink(object obj)
    {
        var ctx = new ProjectDocumentSelect
        {
            DateStart = CurrentProject.DateStart,
            DateEnd = CurrentProject.DateEnd ?? DateTime.Today,
            CurrentProject = CurrentProject,
            ProjectName = CurrentProject.Name
        };
        var dlg = new SelectDialogView { DataContext = ctx };
        ctx.Form = dlg;
        dlg.ShowDialog();
        // ReSharper disable once RedundantJumpStatement
        if (!ctx.DialogResult) return;
        if (ctx.CurrentDocument == null) return;
        ctx.CurrentDocument.ProjectId = CurrentProject.Id;
        foreach (var doc in ctx.DocumentList.Where(_ => _.IsSelected))
        {
            doc.ProjectId = CurrentProject.Id;
            manager.SaveDocument(doc, CurrentProject.Id);
            AllDocuments.Add(doc);
            AllProjectDocuments.Add(doc);
            Documents.Add(doc);
        }

        updatePeriodSum();
        calcCurrentProjectSum(CurrentProject);
        RaisePropertyChanged(nameof(Periods));
        RaisePropertyChanged(nameof(Documents));
        CurrentPeriod = Periods.FirstOrDefault(_ =>
            _.DateStart <= ctx.CurrentDocument.DocDate && _.DateEnd >= ctx.CurrentDocument.DocDate);
    }

    private void GetDocumentsInCollection()
    {
        if (CurrentProject == null) return;
        Documents.Clear();
        if (IsAllPeriods == false)
        {
            if (CurrentPeriod == null) return;
            foreach (var d in AllProjectDocuments.Where(_ =>
                         _.ProjectId == CurrentProject.Id && _.DocDate >= CurrentPeriod.DateStart
                                                          && _.DocDate <= CurrentPeriod.DateEnd)
                    )
            {
                var doc = Documents.FirstOrDefault(_ => _.DocCode == d.DocCode && _.DocRowId == d.DocRowId);
                if (doc == null)
                {
                    var newdoc = new ProjectDocumentViewModel
                    {
                        Id = d.Id,
                        CurrencyDC = d.Currency.DocCode,
                        Currency = d.Currency,
                        DocCode = d.DocCode,
                        DocRowId = d.DocRowId,
                        DocInNum = d.DocInNum,
                        DocName = d.DocName,
                        DocNum = d.DocNum,
                        Employee = GlobalOptions.ReferencesCache.GetEmployee(d.EmployeeTN) as Employee,
                        EmployeeTN = d.EmployeeTN,
                        KontragentDC = d.KontragentDC,
                        Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent,
                        NomenklDC = d.NomenklDC,
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as Nomenkl,
                        DocType = d.DocType,
                        ProjectId = d.ProjectId,
                        DocDate = d.DocDate,
                        WarehouseDocDC = d.WarehouseDocDC,
                        WarehouseCode = d.WarehouseCode,
                        Sum = d.Sum,
                        ConfirmedSum = d.ConfirmedSum,
                        ProfitOrLossType = d.ProfitOrLossType
                    };
                    switch (newdoc.Currency.DocCode)
                    {
                        case CurrencyCode.SEK:
                            newdoc.ProfitSEK += d.ProfitSEK;
                            newdoc.LossSEK += d.LossSEK;
                            newdoc.DilerSEK += d.DilerSEK;
                            break;
                        case CurrencyCode.USD:
                            newdoc.ProfitUSD += d.ProfitUSD;
                            newdoc.LossUSD += d.LossUSD;
                            newdoc.DilerUSD += d.DilerUSD;
                            break;
                        case CurrencyCode.EUR:
                            newdoc.ProfitEUR += d.ProfitEUR;
                            newdoc.LossEUR += d.LossEUR;
                            newdoc.DilerEUR += d.DilerEUR;
                            break;
                        case CurrencyCode.RUB:
                            newdoc.ProfitRUB += d.ProfitRUB;
                            newdoc.LossRUB += d.LossRUB;
                            newdoc.DilerRUB += d.DilerRUB;
                            break;
                        case CurrencyCode.CHF:
                            newdoc.ProfitCHF += d.ProfitCHF;
                            newdoc.LossCHF += d.LossCHF;
                            newdoc.DilerCHF += d.DilerCHF;
                            break;
                        case CurrencyCode.GBP:
                            newdoc.ProfitGBP += d.ProfitGBP;
                            newdoc.LossGBP += d.LossGBP;
                            newdoc.DilerGBP += d.DilerGBP;
                            break;
                    }

                    Documents.Add(newdoc);
                }
                else
                {
                    switch (doc.Currency.DocCode)
                    {
                        case CurrencyCode.SEK:
                            doc.ProfitSEK += d.ProfitSEK;
                            doc.LossSEK += d.LossSEK;
                            doc.DilerSEK += d.DilerSEK;
                            break;
                        case CurrencyCode.USD:
                            doc.ProfitUSD += d.ProfitUSD;
                            doc.LossUSD += d.LossUSD;
                            doc.DilerUSD += d.DilerUSD;
                            break;
                        case CurrencyCode.EUR:
                            doc.ProfitEUR += d.ProfitEUR;
                            doc.LossEUR += d.LossEUR;
                            doc.DilerEUR += d.DilerEUR;
                            break;
                        case CurrencyCode.RUB:
                            doc.ProfitRUB += d.ProfitRUB;
                            doc.LossRUB += d.LossRUB;
                            doc.DilerRUB += d.DilerRUB;
                            break;
                        case CurrencyCode.CHF:
                            doc.ProfitCHF += d.ProfitCHF;
                            doc.LossCHF += d.LossCHF;
                            doc.DilerCHF += d.DilerCHF;
                            break;
                        case CurrencyCode.GBP:
                            doc.ProfitGBP += d.ProfitGBP;
                            doc.LossGBP += d.LossGBP;
                            doc.DilerGBP += d.DilerGBP;
                            break;
                    }
                }
            }
        }

        if (IsAllPeriods)
        {
            if (CurrentProject == null) return;
            if (Periods.Count == 0) return;
            foreach (var d in AllProjectDocuments.Where(_ => _.ProjectId == CurrentProject.Id))
            {
                foreach (var c in DilerDocCollection.Where(_ => _.DocDC == d.DocCode))
                    switch (c.Crs.DocCode)
                    {
                        case CurrencyCode.SEK:
                            d.DilerSEK += c.Summa;
                            break;
                        case CurrencyCode.USD:
                            d.DilerUSD += c.Summa;
                            break;
                        case CurrencyCode.EUR:
                            d.DilerEUR += c.Summa;
                            break;
                        case CurrencyCode.RUB:
                            d.DilerRUB += c.Summa;
                            break;
                        case CurrencyCode.CHF:
                            d.DilerCHF += c.Summa;
                            break;
                        case CurrencyCode.GBP:
                            d.DilerGBP += c.Summa;
                            break;
                    }

                Documents.Add(d);
            }

            RaisePropertyChanged(nameof(Documents));
        }
    }

    public ICommand RemoveDocumentLinkCommand
    {
        get { return new Command(RemoveDocumentLink, _ => CurrentDocument != null); }
    }

    private void RemoveDocumentLink(object obj)
    {
        var removeDocs = new List<ProjectDocumentViewModel>(SelectedDocs);
        if (SelectedDocs != null && SelectedDocs.Count > 0)
            foreach (var doc in SelectedDocs)
                manager.DeleteDocument(doc);
        foreach (var rd in removeDocs)
        {
            Documents.Remove(rd);
            var d = AllProjectDocuments.FirstOrDefault(_ => _.DocCode == rd.DocCode && _.DocRowId == rd.DocRowId);
            if (d != null)
            {
                AllDocuments.Remove(d);
                AllProjectDocuments.Remove(d);
            }
        }

        updatePeriodSum();
        GetDocumentsInCollection();
        var frm = Form as ProjectsView;
        MultyCurrencyHelper.VisibilityCurrencyWithDilerColumns(frm?.gridProjects, Projects);
        MultyCurrencyHelper.VisibilityCurrencyColumns(frm?.treePeriods, Periods);
        MultyCurrencyHelper.VisibilityCurrencyColumns(frm?.treePeriods, Periods);
        calcCurrentProjectSum(CurrentProject);
        RaisePropertyChanged(nameof(Documents));
        RaisePropertyChanged(nameof(Periods));
    }

    #endregion
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class DilerRow
{
    public decimal DocDC { set; get; }
    public int Code { set; get; }
    public DateTime Date { set; get; }
    public decimal Summa { set; get; }
    public Currency Crs { set; get; }
    public Guid ProjectId { set; get; }
}
