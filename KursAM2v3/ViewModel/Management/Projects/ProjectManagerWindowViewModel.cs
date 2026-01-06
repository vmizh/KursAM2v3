using Core.ViewModel.Base;
using Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using KursAM2.Managers;
using KursAM2.View.KursReferences;
using KursAM2.View.Projects;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Reference;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.WindowsManager.WindowsManager;
using KursRepositories.Repositories.Projects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
        DocDateEnd = DateTime.Today;
        var d = DocDateEnd.AddMonths(-3);
        DocDateStart = new DateTime(d.Year, d.Month, 1);
        RefreshData(null);
    }

    #endregion

    #region Commands

    public ICommand ExcludeDocumentFromAllProjectOpenCommand
    {
        get { return new Command(ExcludeDocumentFromAllProjectOpen, _ => DocCurrentDocument is not null); }
    }

    private void ExcludeDocumentFromAllProjectOpen(object obj)
    {
        var winManager = new WindowManager();
        if (winManager.ShowWinUIMessageBox("Вы уверены, что хотите исключить документ из всех проектов?", "Запрос",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            return;
        foreach (var p in DocProjects)
        {
            switch (DocCurrentDocument.DocType)
            {
                case DocumentType.InvoiceClient:
                    myProjectRepository.ExcludeInvoiceClientFromProject(DocCurrentDocument.Id, p.Id);
                    break;
                case DocumentType.InvoiceProvider:
                    myProjectRepository.ExcludeInvoiceProviderFromProject(DocCurrentDocument.Id, p.Id);
                    break;
            }
        }
        DocDocuments.Remove(DocCurrentDocument);
        DocProjects.Clear();
    }

    public ICommand DocProjectRemoveCommand
    {
        get { return new Command(DocProjectRemove, _ => DocCurrentProject is not null); }
    }

    private void DocProjectRemove(object obj)
    {
        switch (DocCurrentDocument.DocType)
        {
            case DocumentType.InvoiceClient:
                myProjectRepository.ExcludeInvoiceClientFromProject(DocCurrentDocument.Id, DocCurrentProject.Id);
                break;
            case DocumentType.InvoiceProvider:
                myProjectRepository.ExcludeInvoiceProviderFromProject(DocCurrentDocument.Id, DocCurrentProject.Id);
                break;
        }

        DocProjects.Remove(DocCurrentProject);
        if (DocProjects.Count == 0)
            DocDocuments.Remove(DocCurrentDocument);
    }


    public ICommand ProjectDocumentOpenCommand
    {
        get { return new Command(ProjectDocumentOpen, _ => DocCurrentDocument is not null); }
    }

    private void ProjectDocumentOpen(object obj)
    {
        DocumentsOpenManager.Open(DocCurrentDocument.DocType, DocCurrentDocument.DocCode);
    }


    public ICommand DocInvoicesLoadCommand
    {
        get { return new Command(DocInvoicesLoad, _ => true); }
    }

    private void DocInvoicesLoad(object obj)
    {
        DocDocuments.Clear();
        foreach (var item in myProjectRepository.GetDocumentsIncludesInProjects(DocDateStart, DocDateEnd))
        {
            DocDocuments.Add(item);
        }
    }


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
    
    public ICommand ExcludeFromProjectCommand
    {
        get { return new Command(ExcludeFromProject, _ => CurrentInvoiceNomenklRow?.IsExclude == false); }
    }

    private void ExcludeFromProject(object obj)
    {
        myProjectRepository.ExcludeNomenklFromProjects([CurrentProject.Id], CurrentInvoiceNomenklRow.DocumentType, CurrentInvoiceNomenklRow.Id);
        CurrentInvoiceNomenklRow.IsExclude = true;
        CurrentDocument.HasExcludeRow = InvoiceNomenklRows.Any(_ => _.IsExclude);
        if (!IsShowExcluded)
        {
            CurrentDocument.QuantityOutDocument -= CurrentInvoiceNomenklRow.Quantity;
            CurrentDocument.QuantityOutShipped -=
                CurrentInvoiceNomenklRow.IsUsluga ? 0 : CurrentInvoiceNomenklRow.Shipped;
            CurrentDocument.UslugaSummaIn -= CurrentInvoiceNomenklRow.IsUsluga ? CurrentInvoiceNomenklRow.Summa : 0;
            CurrentDocument.SummaShipped -= CurrentInvoiceNomenklRow.ShippedSumma ?? 0;
            CurrentDocument.SummaOut -= CurrentInvoiceNomenklRow.IsUsluga ? 0 : CurrentInvoiceNomenklRow.Summa;

            InvoiceNomenklRows.Remove(CurrentInvoiceNomenklRow);
        }

        CurrentDocument.RaisePropertyAllChanged();
       
    }

    public ICommand IncludeIntoProjectCommand
    {
        get { return new Command(IncludeToProject, _ => CurrentInvoiceNomenklRow?.IsExclude == true); }
    }

    private void IncludeToProject(object obj)
    {
        myProjectRepository.IncludeNomenklToProject([CurrentProject.Id], CurrentInvoiceNomenklRow.DocumentType,
            CurrentInvoiceNomenklRow.Id); 
        CurrentInvoiceNomenklRow.IsExclude = false; 
        CurrentDocument.HasExcludeRow = InvoiceNomenklRows.Any(_ => _.IsExclude);
       
    }

    public override void UpdateVisualObjects()
    {
        var colNameNotVisible = new List<string>([nameof(ProjectDocumentInfo.DocInfo),nameof(ProjectDocumentInfo.HasExcludeRow)]);
        base.UpdateVisualObjects();
        if (Form is not ProjectManager frm) return;
        var sumNames = new List<string>();
        foreach (var col in frm.gridDocuments.Columns)
        {
            col.ReadOnly = col.FieldName switch
            {
                nameof(ProjectDocumentInfo.ProjectNote) => false,
                _ => true
            };

            if (colNameNotVisible.Contains(col.FieldName)) col.Visible = false;
        }

        foreach (var col in frm.gridDocDocuments.Columns)
        {
            col.ReadOnly = true;
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

        frm.tableViewDocuemts.FormatConditions.Clear();
        frm.tableViewInvoiceRows.FormatConditions.Clear();
        frm.tableViewPtojects.FormatConditions.Clear();
        var excludeRows = new FormatCondition
        {
            //Expression = "[SummaFact] < [Summa]",
            FieldName = "HasExcludeRow",
            ApplyToRow = true,
            Format = new Format
            {
                Foreground = Brushes.Blue
            },
            ValueRule = ConditionRule.Equal,
            Value1 = true
        };
        var excludeInvoiceRows = new FormatCondition
        {
            //Expression = "[SummaFact] < [Summa]",
            FieldName = "IsExclude",
            ApplyToRow = true,
            Format = new Format
            {
                Foreground = Brushes.Blue
            },
            ValueRule = ConditionRule.Equal,
            Value1 = true
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
        frm.tableViewDocuemts.FormatConditions.Add(excludeRows);
        frm.tableViewInvoiceRows.FormatConditions.Add(excludeInvoiceRows);
        frm.tableViewPtojects.FormatConditions.Add(notProfitLossCondition);
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
        var currentProjectId = CurrentProject?.Id;
        var currendDocumentId = CurrentDocument?.Id;

        try
        {
            Projects.Clear();
            foreach (var prj in myProjectRepository.LoadReference().OrderBy(_ => _.Name))
            {
                var newItem = new Project();
                newItem.LoadFromEntity(prj, GlobalOptions.ReferencesCache);
                Projects.Add(newItem);
            }

            if (currentProjectId is null) return;
            if (Form is not ProjectManager frm) return;

            var cProj = Projects.FirstOrDefault(_ => _.Id == currentProjectId);
            if (cProj == null) return;
            if (cProj.Id != CurrentProject.Id)
            {
                CurrentProject = cProj;
                frm.gridProjects.SelectedItem = cProj;
            }
            if (currendDocumentId is null) return;
            var cDoc = Documents.FirstOrDefault(_ => _.Id == currendDocumentId);
            if (cDoc is null) return;
            CurrentDocument = cDoc;
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public ICommand AddDocumentCommand
    {
        get { return new Command(AddDocument, _ => CurrentProject is not null && !CurrentProject.IsClosed); }
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

        foreach (var prj in myProjectRepository.LoadProjectDocuments(CurrentProject.Id, IsShowExcluded))
        {
            prj.myState = RowStatus.NotEdited;
            prj.SetCurrency();
            Documents.Add(prj);
        }

        SetCrsColumnsVisible();
    }

    public ICommand DeleteDocumentCommand
    {
        get
        {
            return new Command(DeleteDocument,
                _ => CurrentProject is not null && CurrentDocument is not null && !CurrentProject.IsClosed);
        }
    }

    private void DeleteDocument(object obj)
    {
        try
        {
            myProjectRepository.DeleteDocumentInfo(CurrentDocument.Id);
            myProjectRepository.DeleteRowExcludeForDocGuid(CurrentDocument.Id);
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
            case DocumentType.StoreOrderIn:
                DocumentsOpenManager.Open(DocumentType.StoreOrderIn, CurrentDocument.WarehouseOrderInDC.Value);
                break;
            case DocumentType.Waybill:
                DocumentsOpenManager.Open(DocumentType.Waybill, CurrentDocument.WaybillDC.Value);
                break;
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
    private Visibility myGridInvoiceInfoVisibility = Visibility.Hidden;
    private ProjectInvoiceNomenklInfo myCurrentInvoiceNomenklRow;
    private bool myIsShowExcluded;
    private ProjectManagerDocumentInfo myDocCurrentDocument;

    #endregion

    #region Properties

    public DateTime DocDateStart
    {
        get;
        set
        {
            if (value.Equals(field)) return;
            field = value;
            RaisePropertyChanged();
        }
    }
    public DateTime DocDateEnd
    {
        get;
        set
        {
            if (value.Equals(field)) return;
            field = value;
            RaisePropertyChanged();
        }
    }

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

    //GridInvoiceInfoVisibility
    public Visibility GridInvoiceInfoVisibility
    {
        get => myGridInvoiceInfoVisibility;
        set
        {
            if (value == myGridInvoiceInfoVisibility) return;
            myGridInvoiceInfoVisibility = value;
            RaisePropertyChanged();
        }
    }

    public override bool IsCanSaveData => Documents.Any(_ => _.State != RowStatus.NotEdited);
    public override string WindowName => "Управление проектами";
    public override string LayoutName => "ProjectManagerWindowViewModel";

    public ObservableCollection<Project> Projects { set; get; } = [];

    public ObservableCollection<ProjectNomenklInfo> NomenklRows { set; get; } = [];

    public ObservableCollection<ProjectInvoiceNomenklInfo> InvoiceNomenklRows { set; get; } = [];

    public ObservableCollection<ProjectDocumentInfo> Documents { set; get; } =
        [];

    public ObservableCollection<ProjectDocumentInfo> SelectedDocuments { set; get; } =
        [];

    public ObservableCollection<ProjectManagerDocumentInfo> DocDocuments { set; get; } = [];

    public ObservableCollection<ProjectsForDocumentInfo> DocProjects { set; get; } = [];

    public ProjectManagerDocumentInfo DocCurrentDocument
    {
        get => myDocCurrentDocument;
        set
        {
            if (Equals(value, myDocCurrentDocument)) return;
            myDocCurrentDocument = value;
            LoadProjectsForDocument(myDocCurrentDocument.Id);
            RaisePropertyChanged();
        }
    }

    private void LoadProjectsForDocument(Guid id)
    {
        DocProjects.Clear();
        foreach (var pId in myProjectRepository.GetProjetFromDocument(DocCurrentDocument.Id))
        {
            var item = GetProjectFullName(Projects,pId);
            DocProjects.Add(new ProjectsForDocumentInfo()
            {
                Id = pId,
                Name = item.Item1,
                Name2 = item.Item2,
                Name3 = item.Item3
            });
        }
    }

    private Tuple<string, string, string> GetProjectFullName(IEnumerable<Project> projects, Guid projectId)
    {
        var ret = new Tuple<string, string, string>(string.Empty, string.Empty, string.Empty);
        if (projects is null) return ret;
        var pp = projects.ToList();
        var p = pp.FirstOrDefault(_ => _.Id == projectId);
        if (p is null) return ret;
        if (p.ParentId is null)
        {

            return new Tuple<string, string, string>(p.Name, string.Empty, string.Empty);
        }

        var par1 = pp.FirstOrDefault(_ => _.Id == p.ParentId);
        if (par1.ParentId is null)
            return new Tuple<string, string, string>(par1?.Name, p.Name, string.Empty);
        var par2 = pp.FirstOrDefault(_ => _.Id == par1.ParentId);
        return new Tuple<string, string, string>(par2?.Name, par1.Name, p.Name);
    }

    public ProjectsForDocumentInfo DocCurrentProject
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            field = value;
            RaisePropertyChanged();
        }
    }

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

    public ProjectInvoiceNomenklInfo CurrentInvoiceNomenklRow
    {
        get => myCurrentInvoiceNomenklRow;
        set
        {
            if (Equals(value, myCurrentInvoiceNomenklRow)) return;
            myCurrentInvoiceNomenklRow = value;
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
            if (myCurrentDocument == null)
            {
                InvoiceNomenklRows.Clear();
                return;
            }
            if (myCurrentDocument is not null)
            {
                if(myCurrentDocument.WaybillDC != null || myCurrentDocument.WarehouseOrderInDC != null
                   || myCurrentDocument.InvoiceProviderId != null || myCurrentDocument.InvoiceClientId != null)
                {
                    LoadNomenklInfo(myCurrentDocument.DocumentType,
                        // ReSharper disable once PossibleInvalidOperationException
                        myCurrentDocument.WaybillDC ?? myCurrentDocument.WarehouseOrderInDC,
                        myCurrentDocument.InvoiceProviderId ?? myCurrentDocument.InvoiceClientId);
                }
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
            IsNotInfoVisibility = Visibility.Visible;
            GridInfoVisibility = Visibility.Hidden;
            GridInvoiceInfoVisibility = Visibility.Hidden;
            var frm = Form as ProjectManager;
            if (myCurrentProject != null)
            {
                Task.Run(async () =>
                {
                    frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                    //var data = LoadDocuments(myCurrentProject?.Id).ToList();
                    var data = await ((ProjectRepository)myProjectRepository).LoadProjectDocuments2Async(myCurrentProject.Id, IsShowExcluded);
                    frm?.Dispatcher.Invoke(() =>
                    {
                        Documents.Clear();
                        foreach (var d in data.ToList())
                        {  d.myState = RowStatus.NotEdited;
                            d.SetCurrency();
                            Documents.Add(d);
                        }

                        frm.loadingIndicator.Visibility = Visibility.Hidden;
                    });

                });
            }

            SetCrsColumnsVisible();
            RaisePropertyChanged();
        }
        get => myCurrentProject;
    }

    public bool IsShowExcluded
    {
        set
        {
            if (value == myIsShowExcluded) return;
            var frm = Form as ProjectManager;
            ProjectDocumentInfo oldCurrent = null;
            int currentRow = -1;
            if (frm != null)
            {
                oldCurrent = frm.gridDocuments.CurrentItem as ProjectDocumentInfo;
                currentRow = frm.tableViewDocuemts.FocusedRowHandle;
            }
            
            myIsShowExcluded = value;
            if (myCurrentProject != null)
            {
                LoadDocuments(myCurrentProject?.Id);
                SetCrsColumnsVisible();
            }
            InvoiceNomenklRows.Clear();
            if (oldCurrent != null)
            {
                CurrentDocument = null;
                CurrentDocument = oldCurrent;
                frm.tableViewDocuemts.FocusedRowHandle = currentRow;
            }

            RaisePropertyChanged();
        }
        get => myIsShowExcluded;
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

    private void LoadNomenklInfo(DocumentType docType, decimal? dc, Guid? id)
    {
        NomenklRows.Clear();
        InvoiceNomenklRows.Clear();
        var exclude = myProjectRepository.GetDocumentsRowExclude([CurrentProject.Id]);
        switch (docType)
        {
            case DocumentType.InvoiceClient:
                if (id is null) return;
                GridInfoVisibility = Visibility.Hidden;
                IsNotInfoVisibility = Visibility.Hidden;
                GridInvoiceInfoVisibility = Visibility.Visible;
                var rows = myProjectRepository.GetInvoiceClientRows(id.Value,CurrentProject.Id);
                foreach (var r in rows)
                {
                    if (!IsShowExcluded && exclude.Select(_ => _.SFClientRowId).Contains(r.Id)) continue;
                    var newInv = new ProjectInvoiceNomenklInfo()
                    {
                        Note = r.SFT_TEXT,
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0,
                        NomenklName = r.SD_83.NOM_NAME,
                        NomenklNumber = r.SD_83.NOM_NOMENKL,
                        Quantity = (decimal)r.SFT_KOL,
                        Unit = r.SD_83.SD_175.ED_IZM_NAME,
                        // ReSharper disable once PossibleInvalidOperationException
                        UnitPrice = (decimal)r.SFT_ED_CENA,
                        Shipped = (r.TD_24?.Sum(_ => _.DDT_KOL_RASHOD) ?? 0) > (decimal)r.SFT_KOL ?
                            (decimal)r.SFT_KOL : r.SD_83.NOM_0MATER_1USLUGA == 1 ? 1 : r.TD_24?.Sum(_ => _.DDT_KOL_RASHOD) ?? 0,
                        ShippedSumma = (decimal?)r.SFT_KOL == 0 ? 0 :
                            r.SD_83.NOM_0MATER_1USLUGA == 1 ? r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0 :
                            (r.TD_24?.Sum(_ => _.DDT_KOL_RASHOD) ?? 0) * r.SFT_SUMMA_K_OPLATE_KONTR_CRS /
                            (decimal?)r.SFT_KOL ?? 0,
                        IsUsluga = r.SD_83.NOM_0MATER_1USLUGA == 1,
                        DocumentType = DocumentType.InvoiceClient,
                        IsExclude = exclude.Select(_ => _.SFClientRowId).Contains(r.Id),
                        Id = r.Id,
                    };
                    if (newInv.Summa == 0 || newInv.Summa != newInv.UnitPrice * newInv.Quantity)
                    {
                        newInv.IsManualChanged = true;
                    }

                    InvoiceNomenklRows.Add(newInv);

                }
                break;
            case DocumentType.InvoiceProvider:
                if(id is null) return;
                GridInfoVisibility = Visibility.Hidden;
                IsNotInfoVisibility = Visibility.Hidden;
                GridInvoiceInfoVisibility = Visibility.Visible;
                var rows2 = myProjectRepository.GetInvoiceProviderRows(id.Value);
                foreach (var r in rows2)
                {
                    if (r.TD_26_CurrencyConvert.Count > 0)
                    {
                        
                        foreach (var c in r.TD_26_CurrencyConvert)
                        {
                            if (!IsShowExcluded && exclude.Select(_ => _.NomCurrencyConvertRowId).Contains(c.Id)) continue;
                            InvoiceNomenklRows.Add(new ProjectInvoiceNomenklInfo()
                            {
                                Note = r.SFT_TEXT,
                                // ReSharper disable once PossibleInvalidOperationException
                                Summa = r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0,
                                NomenklName = r.SD_83.NOM_NAME,
                                NomenklNumber = r.SD_83.NOM_NOMENKL,
                                Quantity = r.SFT_KOL,
                                Unit = r.SD_83.SD_175.ED_IZM_NAME,
                                // ReSharper disable once PossibleInvalidOperationException
                                UnitPrice = (decimal)r.SFT_ED_CENA,
                                Shipped = r.SD_83.NOM_0MATER_1USLUGA == 1 ? 1 : r.TD_24?.Sum(_ => _.DDT_KOL_PRIHOD) ?? 0,
                                ShippedSumma = r.SD_83.NOM_0MATER_1USLUGA == 1
                                    ? r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0
                                    : (r.TD_24?.Sum(_ => _.DDT_KOL_PRIHOD) ?? 0) * r.SFT_SUMMA_K_OPLATE_KONTR_CRS /
                                    r.SFT_KOL ?? 0,
                                IsUsluga = r.SD_83.NOM_0MATER_1USLUGA == 1,
                                IsExclude = exclude.Select(_ => _.NomCurrencyConvertRowId).Contains(c.Id),
                                DocumentType = DocumentType.NomenklCurrencyConverterProvider,
                                Id = c.Id
                            });
                        }
                    }
                    else
                    {
                        if (!IsShowExcluded && exclude.Select(_ => _.SFProviderRowId).Contains(r.Id)) continue;
                        InvoiceNomenklRows.Add(new ProjectInvoiceNomenklInfo()
                        {
                            IsExclude = exclude.Select(_ => _.SFProviderRowId).Contains(r.Id),
                            Note = r.SFT_TEXT,
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0,
                            NomenklName = r.SD_83.NOM_NAME,
                            NomenklNumber = r.SD_83.NOM_NOMENKL,
                            Quantity = r.SFT_KOL,
                            Unit = r.SD_83.SD_175.ED_IZM_NAME,
                            // ReSharper disable once PossibleInvalidOperationException
                            UnitPrice = (decimal)r.SFT_ED_CENA,
                            Shipped = r.SD_83.NOM_0MATER_1USLUGA == 1 ? 1 : r.TD_24?.Sum(_ => _.DDT_KOL_PRIHOD) ?? 0,
                            ShippedSumma = r.SD_83.NOM_0MATER_1USLUGA == 1
                                ? r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0
                                : (r.TD_24?.Sum(_ => _.DDT_KOL_PRIHOD) ?? 0) * r.SFT_SUMMA_K_OPLATE_KONTR_CRS /
                                r.SFT_KOL ?? 0,
                            IsUsluga = r.SD_83.NOM_0MATER_1USLUGA == 1,
                            DocumentType = DocumentType.InvoiceProvider,
                            Id = r.Id
                        });
                    }
                }
                break;
            case DocumentType.StoreOrderIn:
                GridInfoVisibility = Visibility.Visible;
                GridInvoiceInfoVisibility = Visibility.Hidden;
                IsNotInfoVisibility = Visibility.Hidden;
                if (dc is null) return;
                foreach (var newItem in from row in myProjectRepository.GetNomenklRows(dc.Value)
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
                GridInvoiceInfoVisibility = Visibility.Hidden;
                if (dc is null) return;
                foreach (var newItem in from row in myProjectRepository.GetNomenklRows(dc.Value)
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
                GridInvoiceInfoVisibility = Visibility.Hidden;
                break;
        }
    }

    private List<ProjectDocumentInfo> LoadDocuments(Guid? currentProjectId)
    {
        var ret = new List<ProjectDocumentInfo>();  
        if (currentProjectId is null) return ret;
        var data = myProjectRepository.LoadProjectDocuments2(currentProjectId.Value, IsShowExcluded).ToList();
        foreach (var doc in data)
        {
            doc.myState = RowStatus.NotEdited;
            doc.SetCurrency();
            ret.Add(doc);
        }

        return ret;

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
