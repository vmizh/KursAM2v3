using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.ViewModel.Base;
using Data;
using KursAM2.Managers;
using KursDomain;
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

        [Display(AutoGenerateField = true, Name = "Ном.№")]
        public string NomNomenkl { get; set; }

        [Display(AutoGenerateField = true, Name = "Номенклатура")]
        public string NomName { get; set; }

        [Display(AutoGenerateField = true, Name = "Услуга")]
        public bool IsService { get; set; }

        [Display(AutoGenerateField = true, Name = "Кол-во(прих/док)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocQuantityIn { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма(прих/док)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocSummaIn { get; set; }

        [Display(AutoGenerateField = true, Name = "Кол-во(расх/док)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocQuantityOut { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма(расх/док)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DocSummaOut { get; set; }

        [Display(AutoGenerateField = true, Name = "Кол-во(прих/факт)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactQuantityIn { get; set; }

        [Display(AutoGenerateField = true, Name = "Кол-во(расх/факт)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactQuantityOut { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма(прих/факт)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactSummaIn { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма(расх/факт)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactSummaOut { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма накл.")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NakladSumma { get; set; }

        [Display(AutoGenerateField = true, Name = "Сумма дилер")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal DilerSumma { get; set; }

        [Display(AutoGenerateField = true, Name = "Результат (кол-во)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal QuantityResult => DocQuantityIn - DocQuantityOut;

        [Display(AutoGenerateField = true, Name = "Результат (сумма)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal SummaResult => FactQuantityOut - DilerSumma - FactSummaIn - NakladSumma;

        [Display(AutoGenerateField = true, Name = "Услуги поставщиков")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ServiceProviderSumma { get; set; }

        [Display(AutoGenerateField = true, Name = "Услуги клиентам")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal ServiceClientSumma { get; set; }
    }

    #endregion

    #region Commands

    public override void RefreshData(object obj)
    {
        try
        {
            Projects.Clear();
            foreach (var prj in myProjectRepository.LoadReference().OrderBy(_ => _.Name))
            {
                if (!IsAllProject && prj.IsDeleted) continue;
                var newItem = new Project();
                newItem.LoadFromEntity(prj, GlobalOptions.ReferencesCache);
                Projects.Add(newItem);
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
        DocumentsOpenManager.Open(CurrentDocument.DocumentType, CurrentDocument.DocCode);
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

    #endregion

    #region Properties

    public override string WindowName => "Движение товаров по проектам";
    public override string LayoutName => "ProjectNomenklMoveViewModel";

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

    private void LoadDocuments()
    {
        DocumentRows.Clear();
        if (CurrentNomenkl is null || CurrentProject is null) return;
        var ProjIds = new List<Guid>();
        if (IsRecursive)
            ProjIds.AddRange(myProjectRepository.GetChilds(CurrentProject.Id));
        else
            ProjIds.Add(CurrentProject.Id);

        foreach (var doc in myProjectRepository.GetDocumentsForNomenkl(ProjIds, CurrentNomenkl.NomDC))
            DocumentRows.Add(doc);
    }

    private void LoadNomenkls()
    {
        NomenklRows.Clear();
        DocumentRows.Clear();
        if (CurrentProject is null) return;
        foreach (var n in myProjectRepository.GetNomenklMoveForProject(CurrentProject.Id, IsRecursive))
            NomenklRows.Add(new ProjectNomenklMoveInfo
            {
                NakladSumma = n.NakladSumma ?? 0,
                FactSummaIn = n.IsService == 0 ? n.FactSummaIn ?? 0 : 0,
                FactQuantityOut = n.IsService == 0 ? n.FactQuantityOut ?? 0 : 0,
                DilerSumma = n.DilerSumma,
                DocQuantityIn = n.IsService == 0 ? n.DocQuantityIn ?? 0 : 0,
                DocQuantityOut = n.IsService == 0 ? n.DocQuantityOut ?? 0 : 0,
                DocSummaIn = n.IsService == 0 ? n.DocSummaIn ?? 0 : 0,
                DocSummaOut = n.IsService == 0 ? n.DocSummaOut ?? 0 : 0,
                FactQuantityIn = n.IsService == 0 ? n.FactQuantityIn ?? 0 : 0,
                FactSummaOut = n.IsService == 0 ? n.FactSummaOut ?? 0 : 0,
                IsService = (n.IsService ?? 0) == 1,
                ServiceClientSumma = n.IsService == 1 ? n.DocSummaOut ?? 0 : 0,
                ServiceProviderSumma = n.IsService == 1 ? n.DocSummaIn ?? 0 : 0,
                NomDC = n.NomDC ?? 0,
                NomId = n.NomId ?? Guid.Empty,
                NomName = n.NomName,
                NomNomenkl = n.NomNomenkl
            });
    }

    #endregion
}
