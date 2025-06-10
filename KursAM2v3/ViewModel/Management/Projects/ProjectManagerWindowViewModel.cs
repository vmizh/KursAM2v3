using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using KursAM2.Managers;
using KursAM2.View.KursReferences;
using KursAM2.View.Projects;
using KursAM2.ViewModel.Reference;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursRepositories.Repositories.Projects;

namespace KursAM2.ViewModel.Management.Projects;

public sealed class ProjectManagerWindowViewModel : RSWindowViewModelBase
{
    #region Constructors

    public ProjectManagerWindowViewModel()
    {
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.ReferenceRightBar(this);

        myContext = GlobalOptions.GetEntities();
        myProjectRepository = new ProjectRepository(myContext);
        RefreshData(null);
    }

    #endregion

    #region Commands

    public override void UpdateVisualObjects()
    {
        var colNameNotVisible = new List<string>([nameof(ProjectDocumentInfo.DocInfo)]);
        base.UpdateVisualObjects();
        if (Form is not ProjectManager frm) return;
        var sumNames = new List<string>();
        foreach (var col in frm.gridDocuments.Columns)
        {
            switch (col.FieldName)
            {
                case nameof(ProjectDocumentInfo.ProjectNote):
                    col.ReadOnly = false;
                    break;
                default:
                    col.ReadOnly = true;
                    break;
            }

            if (colNameNotVisible.Contains(col.FieldName)) col.Visible = false;
        }

        foreach (var s in frm.gridDocuments.TotalSummary) sumNames.Add(s.FieldName);

        if (!sumNames.Contains("DocumentType"))
            frm.gridDocuments.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "DocumentType",
                ShowInColumn = "DocumentType",
                SummaryType = SummaryItemType.Count
            });

        foreach (var col in frm.gridInfoRows.Columns) col.ReadOnly = true;
    }

    public override void SaveData(object data)
    {
        foreach (var doc in Documents.Where(_ => _.State == RowStatus.Edited))
        {
            if (string.IsNullOrWhiteSpace(doc.ProjectNote)) continue;
            myProjectRepository.UpdateDocumentInfo(doc.Id, doc.ProjectNote);
            doc.myState = RowStatus.NotEdited;
        }
    }

    public override void RefreshData(object obj)
    {
        try
        {
            Projects.Clear();
            foreach (var prj in myProjectRepository.LoadReference().OrderBy(_ => _.Name))
            {
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

    public ICommand AddDocumentCommand
    {
        get { return new Command(AddDocument, _ => CurrentProject is not null); }
    }

    private void AddDocument(object obj)
    {
        var ctx = new ProjectDocSelectDialog(CurrentProject, new ProjectRepository(GlobalOptions.GetEntities()));
        var service = this.GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, $"Выбор документов для проекта: {CurrentProject.Name}.", ctx) ==
            MessageResult.Cancel) return;
        if (ctx.SelectedRows.Count == 0) return;
        Documents.Clear();
        foreach (var item in ctx.SelectedRows)
        {
            if (Documents.Any(_ => _.BankCode == item.BankCode && _.CashInDC == item.CashInDC &&
                                   _.CashOutDC == item.CashOutDC
                                   && _.WarehouseOrderInDC == item.WarehouseOrderInDC && _.WaybillDC == item.WaybillDC
                                   && _.AccruedClientRowId == item.AccruedClientRowId &&
                                   _.AccruedSupplierRowId == item.AccruedSupplierRowId
                                   && _.UslugaClientRowId == item.UslugaClientRowId
                                   && _.UslugaProviderRowId == item.UslugaProviderRowId
                                   && _.CurrencyConvertId == item.CurrencyConvertId
                                   && _.InvoiceProviderId == item.InvoiceProviderId
                                   && _.InvoiceClientId == item.InvoiceClientId))
                continue;
            myProjectRepository.AddDocumentInfo(item);
        }

        foreach (var prj in myProjectRepository.LoadProjectDocuments(CurrentProject.Id))
        {
            prj.myState = RowStatus.NotEdited;
            prj.SetCurrency();
            Documents.Add(prj);
        }

        SetCrsColumnsVisible();
    }

    public ICommand DeleteDocumentCommand
    {
        get { return new Command(DeleteDocument, _ => CurrentDocument is not null); }
    }

    private void DeleteDocument(object obj)
    {
        try
        {
            myProjectRepository.DeleteDocumentInfo(CurrentDocument.Id);
            Documents.Remove(CurrentDocument);
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public override bool IsDocumentOpenAllow => CurrentDocument is not null;

    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    protected override void DocumentOpen(object obj)
    {
        switch (CurrentDocument.DocumentType)
        {
            case DocumentType.CashIn:
                DocumentsOpenManager.Open(DocumentType.CashIn, CurrentDocument.CashInDC.Value);
                break;
            case DocumentType.CashOut:
                DocumentsOpenManager.Open(DocumentType.CashOut, CurrentDocument.CashOutDC.Value);
                break;
            case DocumentType.Bank:
                DocumentsOpenManager.Open(DocumentType.Bank, (int)CurrentDocument.BankCode);
                break;
            case DocumentType.InvoiceProvider:
                if (CurrentDocument.InvoiceProviderId is null) return;
                var dc4 = myProjectRepository.GetInvoiceProviderDC(CurrentDocument.InvoiceProviderId.Value,false,false);
                if (dc4 is not null)
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, dc4.Value);
                break;
            case DocumentType.InvoiceServiceProvider:
                if (CurrentDocument.InvoiceProviderId is null) return;
                var dc5 = myProjectRepository.GetInvoiceProviderDC(CurrentDocument.UslugaProviderRowId.Value,true,false);
                if (dc5 is not null)
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, dc5.Value);
                break;
                break;
            case DocumentType.InvoiceClient:
                if (CurrentDocument.InvoiceClientId is null) return;
                var dc = myProjectRepository.GetInvoiceClientDC(CurrentDocument.InvoiceClientId.Value,false);
                if (dc is not null)
                    DocumentsOpenManager.Open(DocumentType.InvoiceClient, dc.Value);
                break;
            case DocumentType.InvoiceServiceClient:
                if (CurrentDocument.UslugaClientRowId is null) return;
                var dc3 = myProjectRepository.GetInvoiceClientDC(CurrentDocument.UslugaClientRowId.Value, true);
                if (dc3 is not null)
                    DocumentsOpenManager.Open(DocumentType.InvoiceClient, dc3.Value);
                break;
            case DocumentType.NomenklCurrencyConverterProvider:
                if (CurrentDocument.UslugaProviderRowId is not null)
                {
                    var dc2 = myProjectRepository.GetInvoiceProviderDC(CurrentDocument.UslugaProviderRowId.Value,
                        true,false);
                    if (dc2 is not null)
                        DocumentsOpenManager.Open(DocumentType.InvoiceProvider, dc2.Value);
                }

                if (CurrentDocument.CurrencyConvertId is not null)
                {
                    var dc2 = myProjectRepository.GetInvoiceProviderDC(CurrentDocument.CurrencyConvertId.Value,false, true);
                    if (dc2 is not null)
                        DocumentsOpenManager.Open(DocumentType.InvoiceProvider, dc2.Value);
                }

                break;
        }
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

    #endregion

    #region Fields

    private readonly List<string> myCrsNames = ["RUB", "EUR", "USD", "CHF", "CNY", "SEK", "GBP"];

    private readonly IProjectRepository myProjectRepository;
    private readonly ALFAMEDIAEntities myContext;

    private Project myCurrentProject;
    private bool myIsAllProject;
    private ProjectDocumentInfo myCurrentDocument;
    private ProjectNomenklInfo myCurrentNomenklRow;
    private Visibility myIsNotInfoVisibility = Visibility.Visible;
    private Visibility myGridInfoVisibility = Visibility.Hidden;

    #endregion

    #region Properties

    public Visibility IsNotInfoVisibility
    {
        get => myIsNotInfoVisibility;
        set
        {
            if (value == myIsNotInfoVisibility) return;
            myIsNotInfoVisibility = value;
            RaisePropertyChanged();
        }
    }

    public Visibility GridInfoVisibility
    {
        get => myGridInfoVisibility;
        set
        {
            if (value == myGridInfoVisibility) return;
            myGridInfoVisibility = value;
            RaisePropertyChanged();
        }
    }

    public override bool IsCanSaveData => Documents.Any(_ => _.State != RowStatus.NotEdited);
    public override string WindowName => "Управление проектами";
    public override string LayoutName => "ProjectManagerWindowViewModel";

    public ObservableCollection<Project> Projects { set; get; } = [];

    public ObservableCollection<ProjectNomenklInfo> NomenklRows { set; get; } = [];

    public ObservableCollection<ProjectDocumentInfo> Documents { set; get; } =
        [];

    public ObservableCollection<ProjectDocumentInfo> SelectedDocuments { set; get; } =
        [];

    public ProjectNomenklInfo CurrentNomenklRow
    {
        get => myCurrentNomenklRow;
        set
        {
            if (Equals(value, myCurrentNomenklRow)) return;
            myCurrentNomenklRow = value;
            RaisePropertyChanged();
        }
    }

    public ProjectDocumentInfo CurrentDocument
    {
        get => myCurrentDocument;
        set
        {
            if (Equals(value?.Id, myCurrentDocument?.Id)) return;
            myCurrentDocument = value;
            if (myCurrentDocument is not null &&
                (myCurrentDocument.WaybillDC ?? myCurrentDocument.WarehouseOrderInDC) is not null)
            {
                LoadNomenklInfo(myCurrentDocument.DocumentType,
                    // ReSharper disable once PossibleInvalidOperationException
                    (decimal)(myCurrentDocument.WaybillDC ?? myCurrentDocument.WarehouseOrderInDC));
            }
            else
            {
                IsNotInfoVisibility = Visibility.Visible;
                GridInfoVisibility = Visibility.Hidden;
            }

            RaisePropertyChanged();
        }
    }

    public Project CurrentProject
    {
        set
        {
            if (Equals(value?.Id, myCurrentProject?.Id)) return;
            myCurrentProject = value;
            LoadDocuments(myCurrentProject?.Id);
            SetCrsColumnsVisible();
            RaisePropertyChanged();
        }
        get => myCurrentProject;
    }

    public bool IsAllProject
    {
        set
        {
            if (value == myIsAllProject) return;
            myIsAllProject = value;
            RaisePropertyChanged();
        }
        get => myIsAllProject;
    }

    #endregion

    #region Methods

    private void LoadNomenklInfo(DocumentType docType, decimal dc)
    {
        NomenklRows.Clear();
        switch (docType)
        {
            case DocumentType.StoreOrderIn:
                GridInfoVisibility = Visibility.Visible;
                IsNotInfoVisibility = Visibility.Hidden;
                foreach (var newItem in from row in myProjectRepository.GetNomenklRows(dc)
                         let nom = GlobalOptions.ReferencesCache.GetNomenkl(row.DDT_NOMENKL_DC) as Nomenkl
                         select new ProjectNomenklInfo
                         {
                             Note = row.DDT_NOTE,
                             // ReSharper disable once PossibleInvalidOperationException
                             Summa = row.DDT_KOL_PRIHOD * (decimal)row.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS /
                                     row.TD_26.SFT_KOL,
                             NomenklName = nom.Name,
                             NomenklNumber = nom.NomenklNumber,
                             Quantity = row.DDT_KOL_PRIHOD,
                             Unit = ((IName)nom.Unit).Name,
                             // ReSharper disable once PossibleInvalidOperationException
                             UnitPrice = (decimal)row.TD_26.SFT_ED_CENA
                         })
                    NomenklRows.Add(newItem);

                break;
            case DocumentType.Waybill:
                GridInfoVisibility = Visibility.Visible;
                IsNotInfoVisibility = Visibility.Hidden;
                foreach (var newItem in from row in myProjectRepository.GetNomenklRows(dc)
                         let nom = GlobalOptions.ReferencesCache.GetNomenkl(row.DDT_NOMENKL_DC) as Nomenkl
                         select new ProjectNomenklInfo
                         {
                             Note = row.DDT_NOTE,
                             // ReSharper disable once PossibleInvalidOperationException
                             Summa = row.DDT_KOL_RASHOD * (decimal)row.TD_84.SFT_SUMMA_K_OPLATE_KONTR_CRS /
                                     (decimal)row.TD_84.SFT_KOL,
                             NomenklName = nom.Name,
                             NomenklNumber = nom.NomenklNumber,
                             Quantity = row.DDT_KOL_RASHOD,
                             Unit = ((IName)nom.Unit).Name,
                             // ReSharper disable once PossibleInvalidOperationException
                             UnitPrice = (decimal)row.TD_84.SFT_ED_CENA
                         })
                    NomenklRows.Add(newItem);
                break;
            default:
                IsNotInfoVisibility = Visibility.Visible;
                GridInfoVisibility = Visibility.Hidden;
                break;
        }
    }

    private void LoadDocuments(Guid? currentProjectId)
    {
        Documents.Clear();
        if (currentProjectId is null) return;
        foreach (var prj in myProjectRepository.LoadProjectDocuments(currentProjectId.Value))
        {
            prj.myState = RowStatus.NotEdited;
            prj.SetCurrency();
            Documents.Add(prj);
        }

        SetCrsColumnsVisible();
    }

    private void SetCrsBandNotVisible(GridControl grid)
    {
        foreach (var b in grid.Bands)
            if (myCrsNames.Any(_ => _ == (string)b.Header))
                b.Visible = false;
    }

    private void SetCrsColumnsVisible()
    {
        if (Form is not ProjectManager frm) return;
        SetCrsBandNotVisible(frm.gridDocuments);
        var crsList = Documents.Where(_m => _m.Currency is not null).Select(_ => _.Currency).Distinct().ToList();
        //foreach (var col in frm.gridDocuments.Columns)
        //{
        if (crsList.Count == 0) return;
        foreach (var b in from crs in crsList
                 from b in frm.gridDocuments.Bands
                 where (string)b.Header == crs.Name
                 select b)
            b.Visible = true;
        //}
    }

    #endregion
}
