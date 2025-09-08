using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Core.ViewModel.Base;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using KursAM2.Managers;
using KursAM2.View.Projects;
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

    #region Вспомогательные классы

    public class ProjectNomenklMoveInfo
    {
        [Display(AutoGenerateField = false)] public Guid NomId { get; set; }

        [Display(AutoGenerateField = false)] public decimal NomDC { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Основные", Name = "Ном.№")]
        public string NomNomenkl { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Основные", Name = "Номенклатура")]
        public string NomName { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Основные", Name = "Услуга")]
        public bool IsService { get; set; }


        [Display(AutoGenerateField = true, Name = "Есть исключенные")]
        public bool HasExcluded { set; get; }

        #region Документы

        [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Кол-во(приход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocQuantityIn { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Сумма(приход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocSummaIn { get; set; }


        [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Кол-во(расход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocQuantityOut { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Сумма(расход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocSummaOut { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Результат (кол-во")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocQuantityResult => DocQuantityIn - DocQuantityOut;

        [Display(AutoGenerateField = true, GroupName = "Документы", Name = "Результат (сумма)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocSummaResult =>  DocSummaOut - DocSummaIn;

        #endregion

        #region Фактически

        [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Кол-во(приход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactQuantityIn { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Кол-во(расход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactQuantityOut { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Сумма(приход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactSummaIn { get; set; }

        [Display(AutoGenerateField = true,GroupName = "Фактические", Name = "Сумма(расход)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactSummaOut { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Результат (кол-во)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactQuantityResult => FactQuantityIn - FactQuantityOut;

        [Display(AutoGenerateField = true, GroupName = "Фактические", Name = "Результат (сумма)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactSummaResult => FactSummaOut - FactSummaIn;

        #endregion

        #region Услуги и прочее

        [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Сумма дилер")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DilerSumma { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Сумма накл.")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NakladSumma { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Услуги поставщиков")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ServiceProviderSumma { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Услуги и затраты", Name = "Услуги клиентам")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ServiceClientSumma { get; set; }

        #endregion

        #region Сводные результаты
        
        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Цена закупки")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultPriceIn { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Сумма закупки")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultSummaIn { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Цена продажи")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultPriceOut { get; set; }
        
        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Продано (кол-во)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultQuantityOut { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Сумма продажи")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultSummaOut { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Результат")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Result { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Остаток не реализованных")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultOstatok { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Остаток (сумма)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ResultOstatokSumma { get; set; }

        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Предполагаемый доход")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ExpectedIncomeSumma { get; set; }
        [Display(AutoGenerateField = true, GroupName = "Сводные результаты", Name = "Предполагаемая маржа")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ExpectedIncomeProfit { get; set; }

        #endregion
    }

    #endregion

    #region Commands

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
        get { return new Command(IncludeIntoProject, _ => (CurrentDocument?.IsInclude ?? false) == false); }
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
        HashSet<string> colSumNames =
        [
            "NomName", "DocQuantityIn", "DocSummaIn", "DocQuantityOut", "DocSummaOut",
            "DocQuantityResult", "DocSummaResult", "FactQuantityResult", "FactSummaResult", "FactQuantityIn",
            "FactQuantityOut",
            "FactSummaIn", "FactSummaOut", "NakladSumma", "DilerSumma",
            "ResultSummaIn","ResultQuantityIn","ResultQuantityOut","ResultSummaOut","Result","ResultOstatok",
            "ResultOstatokSumma","ExpectedIncomeSumma","ExpectedIncomeProfit"
        ];
        HashSet<string> colDocSumNames =
        [
            "DocumentType", "ProviderSumma", "ClientSumma", "ProviderShipped", "ClientShipped", "ProviderQuantity",
            "ClientQuantity", "ProviderShippedQuantity", "ClientShippedQuantity"
        ];
        if (Form is not ProjectNomenklMove frm) return;
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

        frm.tableViewDocuemts.FormatConditions.Add(showRowForExcluded);
        frm.tableViewDocumentRows.FormatConditions.Add(showRowForExcluded2);
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
    }


    private void LoadNomenkls()
    {
        NomenklRows.Clear();
        DocumentRows.Clear();
        if (CurrentProject is null) return;
        //var excl = myProjectRepository.GetDocumentsRowExclude(IsRecursive
        //    ? myProjectRepository.GetChilds(CurrentProject.Id)
        //    : [CurrentProject.Id]).ToList();

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

            };
            newItem.ResultPriceIn = Math.Round(newItem.FactSummaIn == 0 ? 0 : (newItem.FactSummaIn + newItem.NakladSumma) / newItem.FactQuantityIn,2);
            newItem.ResultSummaIn = newItem.ResultPriceIn * newItem.FactQuantityOut + newItem.ServiceProviderSumma;
            newItem.ResultQuantityOut = newItem.FactQuantityOut;
            newItem.ResultSummaOut = (decimal)(n.FactSummaOut - n.DilerSumma + (newItem.IsService ? n.DocSummaOut : 0));
            newItem.ResultPriceOut = Math.Round(newItem.ResultQuantityOut == 0 ? 0 : newItem.ResultSummaOut / newItem.ResultQuantityOut,2);
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
