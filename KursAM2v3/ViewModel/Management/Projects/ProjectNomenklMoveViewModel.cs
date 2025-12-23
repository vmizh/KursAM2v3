using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core.ViewModel.Base;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using KursAM2.Managers;
using KursAM2.View.KursReferences;
using KursAM2.View.Projects;
using KursAM2.ViewModel.Reference;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.WindowsManager.WindowsManager;
using KursRepositories.Repositories.Projects;

namespace KursAM2.ViewModel.Management.Projects;

public sealed class ProjectNomenklMoveViewModel : RSWindowViewModelBase
{
    #region Constructors

    public ProjectNomenklMoveViewModel()
    {
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.ReferenceRightBar(this);
        IsRecursive = true;
        myContext = GlobalOptions.GetEntities();
        myProjectRepository = new ProjectRepository(myContext);
        RefreshData(null);
    }

    #endregion

    #region Commands

    public ICommand ManualQuantityChangedCommand
    {
        get { return new Command(ManualQuantityChanged, _ => CurrentDocument != null); }
    }

    private void ManualQuantityChanged(object obj)
    {
        if (obj is not CellValueChangedEventArgs param) return;
        switch (param.Column.FieldName)
        {
            case nameof(ProjectNomenklMoveDocumentInfo.ManualClientQuantity):
                if ((decimal)param.Value < 0 || (decimal)param.Value > CurrentDocument.ClientQuantity)
                {
                    NotifyColor = Brushes.Red;
                    CurrentDocument.ManualClientQuantity = CurrentDocument.ClientQuantity;
                    NotifyInfo =
                        $"Кол-во не может быть меньше 0 и больше, чем в счете {CurrentDocument.ClientQuantity}";
                    var notification = KursNotyficationService.CreateCustomNotification(this);
                    notification.ShowAsync();
                    return;
                }

                break;
            case nameof(ProjectNomenklMoveDocumentInfo.ManualProviderQuantity):
                if ((decimal)param.Value < 0 || (decimal)param.Value > CurrentDocument.ProviderQuantity)
                {
                    NotifyColor = Brushes.Red;
                    CurrentDocument.ManualProviderQuantity = CurrentDocument.ProviderQuantity;
                    NotifyInfo =
                        $"Кол-во не может быть меньше 0 и больше, чем в счете {CurrentDocument.ProviderQuantity}";
                    var notification = KursNotyficationService.CreateCustomNotification(this);
                    notification.ShowAsync();
                    return;
                }

                break;
        }

        var p = new ProjectManualParameter
        {
            Quantity = (decimal)param.Value,
            ProjectId = CurrentDocument.ProjectId,
            NomDC = CurrentNomenkl.NomDC,
            DocType = CurrentDocument.DocumentType,
            DocDC = CurrentDocument.DocCode
        };
        myProjectRepository.UpdateManualQuantity(p);

        switch (CurrentDocument.DocumentType)
        {
            case DocumentType.InvoiceClient:
                CurrentDocument.ManualClientSumma = CurrentDocument.ManualClientQuantity * CurrentDocument.ClientSumma /
                                                    CurrentDocument.ClientQuantity;
                CurrentNomenkl.DocQuantityOut = DocumentRows.Sum(_ => _.ManualClientQuantity);
                CurrentNomenkl.DocSummaOut = DocumentRows.Sum(_ => _.ManualClientSumma);
                break;
            case DocumentType.InvoiceProvider:
            case DocumentType.NomenklCurrencyConverterProvider:
                CurrentDocument.ManualProviderSumma = CurrentDocument.ManualProviderQuantity *
                    CurrentDocument.ProviderSumma / CurrentDocument.ProviderQuantity;
                CurrentNomenkl.DocQuantityIn = DocumentRows.Sum(_ => _.ManualProviderQuantity);
                CurrentNomenkl.DocSummaIn = DocumentRows.Sum(_ => _.ManualProviderSumma);
                break;
        }

        if (Form is ProjectNomenklMove frm)
        {
            CurrentNomenkl.HasManualChanged = DocumentRows.Any(_ => _.IsManualChanged);
            frm.gridNomenklRows.RefreshData();
        }
    }

    public string NotifyInfo { get; set; }

    public ICommand ExcludeFromProfitLossCommand
    {
        get { return new Command(ExcludeFromProfitLoss, _ => CurrentProject?.IsExcludeFromProfitAndLoss == false); }
    }

    private void ExcludeFromProfitLoss(object obj)
    {
        CurrentProject.IsExcludeFromProfitAndLoss = true;
        myProjectRepository.ExcludeFromProfitLoss(CurrentProject.Id);
    }


    public ICommand IncludeInProfitLossCommand
    {
        get { return new Command(IncludeInProfitLoss, _ => CurrentProject?.IsExcludeFromProfitAndLoss == true); }
    }

    private void IncludeInProfitLoss(object obj)
    {
        CurrentProject.IsExcludeFromProfitAndLoss = false;
        myProjectRepository.IncludeInProfitLoss(CurrentProject.Id);
    }


    public ICommand ProjectsReferenceOpenCommand
    {
        get { return new Command(ProjectsReferenceOpen, _ => true); }
    }

    private void ProjectsReferenceOpen(object obj)
    {
        var prjCtx = new ProjectReferenceWindowViewModel();
        var form = new ProjectReferenceView
        {
            Owner = Application.Current.MainWindow,
            DataContext = prjCtx
        };
        prjCtx.Form = form;
        form.Show();
    }

    public ICommand ExcludeFromProjectCommand
    {
        get { return new Command(ExcludeFromProject, _ => CurrentDocument?.IsInclude == true); }
    }

    private void ExcludeFromProject(object obj)
    {
        var projectGuids = new List<Guid>();
        if (IsRecursive)
            projectGuids.AddRange(myProjectRepository.GetAllTreeProjectIds(CurrentProject.Id));
        else projectGuids.Add(CurrentProject.Id);
        myProjectRepository.ExcludeNomenklFromProjects(projectGuids, CurrentDocument.DocumentType,
            CurrentDocument.Id);
        CurrentDocument.IsInclude = false;
        CurrentNomenkl.HasExcluded = true;
        if (!IsShowExcluded)
            DocumentRows.Remove(CurrentDocument);
    }

    public ICommand IncludeIntoProjectCommand
    {
        get { return new Command(IncludeIntoProject, _ => !(CurrentDocument?.IsInclude ?? false)); }
    }

    private void IncludeIntoProject(object obj)
    {
        var projectGuids = new List<Guid>();
        if (IsRecursive)
            projectGuids.AddRange(myProjectRepository.GetAllTreeProjectIds(CurrentProject.Id));
        else projectGuids.Add(CurrentProject.Id);
        myProjectRepository.IncludeNomenklToProject(projectGuids, CurrentDocument.DocumentType,
            CurrentDocument.Id);
        CurrentDocument.IsInclude = true;
        CurrentNomenkl.HasExcluded = DocumentRows.Any(_ => !_.IsInclude);
    }

    public override void RefreshData(object obj)
    {
        try
        {
            Projects.Clear();
            var counts = myProjectRepository.GetCountDocumentsForProjects();
            foreach (var prj in myProjectRepository.LoadReference().OrderBy(_ => _.Name))
            {
                if (!IsAllProject && prj.IsDeleted) continue;
                if (prj.ParentId is null)
                {
                    var newItem = new Project();
                    newItem.LoadFromEntity(prj, GlobalOptions.ReferencesCache);
                    Projects.Add(newItem);
                    continue;
                }

                if (!IsAllProject)
                {
                    var old = counts.FirstOrDefault(_ => _.Item1 == prj.Id);
                    if (old is null) continue;
                    if (old.Item2 == 0) continue;
                    var newItem = new Project();
                    newItem.LoadFromEntity(prj, GlobalOptions.ReferencesCache);
                    Projects.Add(newItem);
                }
                else
                {
                    var newItem = new Project();
                    newItem.LoadFromEntity(prj, GlobalOptions.ReferencesCache);
                    Projects.Add(newItem);
                }
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public override bool IsDocumentOpenAllow => CurrentDocument is not null;

    protected override void DocumentOpen(object obj)
    {
        DocumentsOpenManager.Open(
            CurrentDocument.DocumentType == DocumentType.NomenklCurrencyConverterProvider
                ? DocumentType.InvoiceProvider
                : CurrentDocument.DocumentType, CurrentDocument.DocCode);
    }

    #endregion

    #region Fields

    private readonly IProjectRepository myProjectRepository;
    private readonly ALFAMEDIAEntities myContext;
    private bool myIsAllProject;
    private bool myIsRecursive;
    private Project myCurrentProject;
    private ObservableCollection<Project> myProjects = [];
    private ObservableCollection<ProjectNomenklMoveInfo> myNomenklRows = [];
    private ProjectNomenklMoveInfo myCurrentNomenkl;
    private ProjectNomenklMoveDocumentInfo myCurrentDocument;
    private bool myIsShowExcluded;

    private readonly HashSet<string> manualColNames =
    [
        "ManualProviderSumma", "ManualClientSumma",
        "ManualProviderQuantity", "ManualClientQuantity"
    ];

    private readonly HashSet<string> colSumNames =
    [
        "NomName", "DocQuantityIn", "DocSummaIn", "DocQuantityOut", "DocSummaOut",
        "DocQuantityResult", "DocSummaResult", "FactQuantityResult", "FactSummaResult", "FactQuantityIn",
        "FactQuantityOut",
        "FactSummaIn", "FactSummaOut", "NakladSumma", "DilerSumma",
        "ResultSummaIn", "ResultQuantityIn", "ResultQuantityOut", "ResultSummaOut", "Result", "ResultOstatok",
        "ResultOstatokSumma", "ExpectedIncomeSumma", "ExpectedIncomeProfit"
    ];

    private readonly HashSet<string> colDocSumNames =
    [
        "DocumentType", "ProviderSumma", "ClientSumma", "ProviderShipped", "ClientShipped", "ProviderQuantity",
        "ClientQuantity", "ProviderShippedQuantity", "ClientShippedQuantity",
        "ManualProviderSumma", "ManualClientSumma", "ManualProviderShipped",
        "ManualClientShipped", "ManualProviderQuantity", "ManualClientQuantity", "ManualProviderShippedQuantity",
        "ManualClientShippedQuantity"
    ];

    #endregion

    #region Properties

    public override string WindowName => "Движение товаров по проектам";
    public override string LayoutName => "ProjectNomenklMoveViewModel";

    public bool IsShowExcluded
    {
        set
        {
            if (value == myIsShowExcluded) return;
            myIsShowExcluded = value;
            LoadNomenkls();
            RaisePropertyChanged();
        }
        get => myIsShowExcluded;
    }

    public ObservableCollection<Project> Projects
    {
        set
        {
            if (Equals(value, myProjects)) return;
            myProjects = value;
            RaisePropertyChanged();
        }
        get => myProjects;
    }

    public ObservableCollection<ProjectNomenklMoveInfo> NomenklRows
    {
        set
        {
            if (Equals(value, myNomenklRows)) return;
            myNomenklRows = value;
            RaisePropertyChanged();
        }
        get => myNomenklRows;
    }

    public ObservableCollection<ProjectNomenklMoveDocumentInfo> DocumentRows { set; get; } = [];

    public ProjectNomenklMoveDocumentInfo CurrentDocument
    {
        get => myCurrentDocument;
        set
        {
            if (Equals(value, myCurrentDocument)) return;
            myCurrentDocument = value;
            RaisePropertyChanged();
        }
    }

    public ProjectNomenklMoveInfo CurrentNomenkl
    {
        get => myCurrentNomenkl;
        set
        {
            if (Equals(value, myCurrentNomenkl)) return;
            myCurrentNomenkl = value;
            LoadDocuments();
            RaisePropertyChanged();
        }
    }

    public Project CurrentProject
    {
        set
        {
            if (Equals(value?.Id, myCurrentProject?.Id)) return;
            myCurrentProject = value;
            LoadNomenkls();
            RaisePropertyChanged();
        }
        get => myCurrentProject;
    }

    public bool IsAllProject
    {
        get => myIsAllProject;
        set
        {
            if (value == myIsAllProject) return;
            myIsAllProject = value;
            NomenklRows.Clear();
            DocumentRows.Clear();
            RefreshData(null);
            RaisePropertyChanged();
        }
    }

    public bool IsRecursive
    {
        get => myIsRecursive;
        set
        {
            if (value == myIsRecursive) return;
            myIsRecursive = value;
            LoadNomenkls();
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void UpdateVisualObjects()
    {
        if (Form is not ProjectNomenklMove frm) return;

        frm.tableViewPtojects.ShowAutoFilterRow = true;

        frm.gridNomenklRows.TotalSummary.Clear();
        frm.gridDocumentsRows.TotalSummary.Clear();
        foreach (var col in frm.gridDocumentsRows.Columns)
        {
            if (!colDocSumNames.Contains(col.FieldName)) continue;
            var sum = new GridSummaryItem
            {
                FieldName = col.FieldName,
                DisplayFormat = "n2",
                ShowInColumn = col.FieldName,
                SummaryType = col.FieldName == "DocumentType" ? SummaryItemType.Count : SummaryItemType.Sum
            };
            frm.gridDocumentsRows.TotalSummary.Add(sum);
        }

        foreach (var col in frm.gridNomenklRows.Columns)
        {
            if (col.FieldName == "HasExcluded")
                col.Visible = false;
            if (!colSumNames.Contains(col.FieldName)) continue;
            var sum = new GridSummaryItem
            {
                FieldName = col.FieldName,
                DisplayFormat = "n2",
                ShowInColumn = col.FieldName,
                SummaryType = col.FieldName == "NomName" ? SummaryItemType.Count : SummaryItemType.Sum
            };
            frm.gridNomenklRows.TotalSummary.Add(sum);
        }

        foreach (var col in frm.gridDocumentsRows.Columns)
            if (col.FieldName == "IsInclude")
                col.Visible = false;
        frm.tableViewDocuemts.FormatConditions.Clear();
        frm.tableViewPtojects.FormatConditions.Clear();
        var showRowForExcluded = new FormatCondition
        {
            //Expression = "[SummaFact] < [Summa]",
            FieldName = "HasExcluded",
            ApplyToRow = true,
            Format = new Format
            {
                Foreground = Brushes.Blue
            },
            ValueRule = ConditionRule.Equal,
            Value1 = true
        };
        var showRowForExcluded2 = new FormatCondition
        {
            FieldName = "IsInclude",
            ApplyToRow = true,
            Format = new Format
            {
                Foreground = Brushes.Blue
            },
            ValueRule = ConditionRule.Equal,
            Value1 = false
        };
        var notProfitLossCondition = new FormatCondition
        {
            FieldName = "IsExcludeFromProfitAndLoss",
            ApplyToRow = true,
            Format = new Format
            {
                Foreground = Brushes.DarkOrange
            },
            ValueRule = ConditionRule.Equal,
            Value1 = true
        };

        frm.tableViewDocuemts.FormatConditions.Add(showRowForExcluded);
        frm.tableViewDocumentRows.FormatConditions.Add(showRowForExcluded2);
        frm.tableViewPtojects.FormatConditions.Add(notProfitLossCondition);

        var manualChangedCondition = new FormatCondition
        {
            FieldName = "IsManualChanged",
            ApplyToRow = true,
            Format = new Format
            {
                Foreground = Brushes.Green
            },
            ValueRule = ConditionRule.Equal,
            Value1 = true
        };

        frm.tableViewDocumentRows.FormatConditions.Add(manualChangedCondition);
    }

    private void LoadDocuments()
    {
        DocumentRows.Clear();
        if (CurrentNomenkl is null || CurrentProject is null) return;
        var ProjIds = new List<Guid>();
        if (IsRecursive)
            ProjIds.AddRange(myProjectRepository.GetChilds(CurrentProject.Id));
        else
            ProjIds.Add(CurrentProject.Id);

        foreach (var doc in myProjectRepository.GetDocumentsForNomenkl(ProjIds, CurrentNomenkl.NomDC, IsShowExcluded))
            DocumentRows.Add(doc);
        if (Form is ProjectNomenklMove frm)
        {
        }
    }

    private void LoadNomenkls()
    {
        NomenklRows.Clear();
        DocumentRows.Clear();
        if (CurrentProject is null) return;
        foreach (var n in myProjectRepository.GetNomenklMoveForProject(CurrentProject.Id, IsRecursive, IsShowExcluded))
        {
            var newItem = new ProjectNomenklMoveInfo
            {
                NakladSumma = n.NakladSumma ?? 0,
                FactSummaIn = n.IsService == 0 ? n.FactSummaIn ?? 0 : 0,
                FactQuantityOut = n.IsService == 0 ? n.FactQuantityOut ?? 0 : 0,
                DilerSumma = n.DilerSumma ?? 0,
                DocQuantityIn = n.IsService == 0 ? n.DocQuantityIn ?? 0 : 0,
                DocQuantityOut = n.IsService == 0 ? n.DocQuantityOut ?? 0 : 0,
                DocSummaIn = n.IsService == 0 ? n.DocSummaIn ?? 0 : 0,
                DocSummaOut = n.IsService == 0 ? n.DocSummaOut ?? 0 : 0,
                FactQuantityIn = n.IsService == 0 ? n.FactQuantityIn ?? 0 : 0,
                FactSummaOut = n.IsService == 0 ? n.FactSummaOut ?? 0 : 0,
                IsService = (n.IsService ?? 0) == 1,
                ServiceClientSumma = n.IsService == 1 ? n.DocSummaIn ?? 0 : 0,
                ServiceProviderSumma = n.IsService == 1 ? n.DocSummaOut ?? 0 : 0,
                NomDC = n.NomDC ?? 0,
                NomId = n.NomId ?? Guid.Empty,
                NomName = n.NomName,
                NomNomenkl = n.NomNomenkl,
                HasExcluded = n.HasExcluded ?? false,
                HasManualChanged = n.IsManualChanged
            };
            newItem.ResultPriceIn =
                Math.Round(
                    newItem.FactSummaIn == 0 ? 0 : (newItem.FactSummaIn + newItem.NakladSumma) / newItem.FactQuantityIn,
                    2);
            newItem.ResultSummaIn = newItem.ResultPriceIn * newItem.FactQuantityOut + newItem.ServiceProviderSumma;
            newItem.ResultQuantityOut = newItem.FactQuantityOut;
            newItem.ResultSummaOut = (decimal)(n.FactSummaOut - n.DilerSumma + (newItem.IsService ? n.DocSummaOut : 0));
            newItem.ResultPriceOut =
                Math.Round(newItem.ResultQuantityOut == 0 ? 0 : newItem.ResultSummaOut / newItem.ResultQuantityOut, 2);
            newItem.Result = newItem.ResultSummaOut - newItem.ResultSummaIn;
            newItem.ResultOstatok = newItem.FactQuantityIn - newItem.FactQuantityOut;
            newItem.ResultOstatokSumma = newItem.ResultPriceIn * newItem.ResultOstatok;
            newItem.ExpectedIncomeSumma = newItem.ResultPriceOut * newItem.ResultOstatok;
            newItem.ExpectedIncomeProfit = newItem.ResultPriceOut * newItem.ResultOstatok -
                                           newItem.ResultPriceIn * newItem.ResultOstatok;
            NomenklRows.Add(newItem);
        }
    }

    #endregion
}
