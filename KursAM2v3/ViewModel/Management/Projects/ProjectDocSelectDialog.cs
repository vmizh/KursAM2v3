using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.Base;
using DevExpress.Data;
using DevExpress.Xpf.Grid;
using KursAM2.View.Projects;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.Menu;
using KursDomain.References;
using KursRepositories.Repositories.Projects;

namespace KursAM2.ViewModel.Management.Projects;

public class ProjectDocSelectDialog : RSWindowViewModelBase
{
    #region Constructors

    public ProjectDocSelectDialog(Project project, IProjectRepository projectRepository)
    {
        IsDialog = true;
        myProject = project;
        myProjectRepository = projectRepository;
        DateEnd = DateTime.Today;
        DateStart = new DateTime(DateEnd.Month == 1 ? DateEnd.Year - 1 : DateEnd.Year,
            DateEnd.Month == 1 ? 12 : DateEnd.Month - 1, 1);
        RightMenuBar = MenuGenerator.RefreshOnlyRightBar(this);
    }

    #endregion

    #region Commands

    public override void UpdateVisualObjects()
    {
        if (CustomDataUserControl is ProjectDocSelectDialogView frm)
        {
            foreach (var col in frm.gridRows.Columns)
            {
                if (col.FieldName == "State")
                    col.Visible = false;
            }
            foreach (var col in frm.gridSelectedRows.Columns)
            {
                if (col.FieldName == "State")
                    col.Visible = false;
            }
            frm.gridRows.TotalSummary.Clear();
            frm.gridRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "SummaIn",
                DisplayFormat = "n2",
                ShowInColumn = "SummaIn",
                SummaryType = SummaryItemType.Sum

            });
            frm.gridRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "SummaOut",
                DisplayFormat = "n2",
                ShowInColumn = "SummaOut",
                SummaryType = SummaryItemType.Sum

            });
            frm.gridRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "SummaDiler",
                DisplayFormat = "n2",
                ShowInColumn = "SummaDiler",
                SummaryType = SummaryItemType.Sum

            });
            frm.gridRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "DocumentType",
                ShowInColumn = "DocumentType",
                SummaryType = SummaryItemType.Count
            });


            frm.gridSelectedRows.TotalSummary.Clear();
            frm.gridSelectedRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "SummaIn",
                DisplayFormat = "n2",
                ShowInColumn = "SummaIn",
                SummaryType = SummaryItemType.Sum

            });
            frm.gridSelectedRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "SummaOut",
                DisplayFormat = "n2",
                ShowInColumn = "SummaOut",
                SummaryType = SummaryItemType.Sum

            });
            frm.gridSelectedRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "SummaDiler",
                DisplayFormat = "n2",
                ShowInColumn = "SummaDiler",
                SummaryType = SummaryItemType.Sum

            });
            frm.gridSelectedRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "DocumentType",
                ShowInColumn = "DocumentType",
                SummaryType = SummaryItemType.Count
            });
        }
    }

    public override void RefreshData(object obj)
    {
        Rows.Clear();
        if (IsCashOrderIn)
            foreach (var doc in myProjectRepository.GetCashInForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    CashInDC = doc.DOC_CODE,
                    Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.CRS_DC) as Currency,
                    CashBox = GlobalOptions.ReferencesCache.GetCashBox(doc.CA_DC) as CashBox,
                    DocumentType = DocumentType.CashIn,
                    DocDate = doc.DATE_ORD ?? DateTime.MinValue,
                    InnerNumber = doc.NUM_ORD ?? 0,
                    SummaIn = doc.SUMM_ORD ?? 0,
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.KONTRAGENT_DC) as Kontragent,
                    Creator = doc.CREATOR,
                    Note = doc.NOTES_ORD
                };
                Rows.Add(newItem);
            }

        if (IsCashOrderOut)
            foreach (var doc in myProjectRepository.GetCashOutForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    CashOutDC = doc.DOC_CODE,
                    Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.CRS_DC) as Currency,
                    CashBox = GlobalOptions.ReferencesCache.GetCashBox(doc.CA_DC) as CashBox,
                    DocumentType = DocumentType.CashOut,
                    DocDate = doc.DATE_ORD ?? DateTime.MinValue,
                    InnerNumber = doc.NUM_ORD ?? 0,
                    SummaOut = doc.SUMM_ORD ?? 0,
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.KONTRAGENT_DC) as Kontragent,
                    Creator = doc.CREATOR,
                    Note = doc.NOTES_ORD
                };
                Rows.Add(newItem);
            }

        if (IsBank)
            foreach (var doc in myProjectRepository.GetBankForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    BankCode = doc.CODE,
                    Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.SD_101.SD_114.CurrencyDC) as Currency,
                    BankAccount = (GlobalOptions.ReferencesCache.GetBankAccount(doc.SD_101.VV_ACC_DC) as BankAccount)
                        ?.Name,
                    DocumentType = DocumentType.Bank,
                    DocDate = doc.SD_101.VV_START_DATE,
                    SummaIn = doc.VVT_VAL_PRIHOD ?? 0,
                    SummaOut = doc.VVT_VAL_RASHOD ?? 0,
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.VVT_KONTRAGENT) as Kontragent,
                    Note = doc.VVT_DOC_NUM,
                };
                Rows.Add(newItem);
            }

        if (IsWarehouseIn)
            foreach (var doc in myProjectRepository.GetWarehouseInForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var crs = GlobalOptions.ReferencesCache.GetCurrency(doc.TD_24.Select(t => t.TD_26).First().SD_26
                    .SF_CRS_DC) as Currency;
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    WarehouseOrderInDC = doc.DOC_CODE,
                    Currency = crs,
                    Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(doc.DD_SKLAD_POL_DC) as Warehouse,
                    DocumentType = DocumentType.StoreOrderIn,
                    DocDate = doc.DD_DATE,
                    InnerNumber = doc.DD_IN_NUM,
                    ExtNumber = doc.DD_EXT_NUM,
                    SummaIn = doc.TD_24.Sum(_ =>
                        _.DDT_KOL_PRIHOD * (_.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / _.TD_26.SFT_KOL),
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.DD_KONTR_OTPR_DC) as Kontragent,
                    Creator = doc.CREATOR,
                    Note = doc.DD_NOTES
                };
                Rows.Add(newItem);
            }

        if (IsWaybill)
            foreach (var doc in myProjectRepository.GetWaybillInForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var crs = GlobalOptions.ReferencesCache.GetCurrency(doc.SD_84.SF_CRS_DC) as Currency;
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    WaybillDC = doc.DOC_CODE,
                    Currency = crs,
                    Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(doc.DD_SKLAD_OTPR_DC) as Warehouse,
                    DocumentType = DocumentType.Waybill,
                    DocDate = doc.DD_DATE,
                    InnerNumber = doc.DD_IN_NUM,
                    ExtNumber = doc.DD_EXT_NUM,
                    SummaOut = doc.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD * (_.TD_84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / (decimal)_.TD_84.SFT_KOL),
                    SummaDiler = doc.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD * (_.TD_84.SFT_NACENKA_DILERA ?? 0) / (decimal)_.TD_84.SFT_KOL),
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.DD_KONTR_POL_DC) as Kontragent,
                    Creator = doc.CREATOR,
                    Note = doc.DD_NOTES
                };
                Rows.Add(newItem);
            }

        if (IsUslugaProvider)
        {
            foreach (var doc in myProjectRepository.GetUslagaProviderForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var crs = GlobalOptions.ReferencesCache.GetCurrency(doc.SD_26.SF_CRS_DC) as Currency;
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    UslugaProviderRowId = doc.Id,
                    Currency = crs,
                    DocumentType = DocumentType.InvoiceProvider,
                    DocDate = doc.SD_26.SF_POSTAV_DATE,
                    InnerNumber = doc.SD_26.SF_IN_NUM,
                    ExtNumber = doc.SD_26.SF_POSTAV_NUM,
                    SummaOut = doc.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD * (_.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / _.TD_26.SFT_KOL),
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.SD_26.SF_POST_DC) as Kontragent,
                    Creator = doc.SD_26.CREATOR,
                    Note = doc.SD_26.SF_NOTES,
                };
                newItem.DocInfo = myProjectRepository.GetDocDescription(DocumentType.InvoiceProvider, newItem);

                Rows.Add(newItem);
            }
        }

        if (IsUslugaClient)
        {
            foreach (var doc in myProjectRepository.GetUslagaClientForProject(myProject.Id, DateStart, DateEnd).ToList())
            {
                var crs = GlobalOptions.ReferencesCache.GetCurrency(doc.SD_84.SF_CRS_DC) as Currency;
                var newItem = new ProjectDocumentInfo
                {
                    ProjectId = myProject.Id,
                    UslugaClientRowId = doc.Id,
                    Currency = crs,
                    DocumentType = DocumentType.InvoiceClient,
                    DocDate = doc.SD_84.SF_DATE,
                    InnerNumber = doc.SD_84.SF_IN_NUM,
                    ExtNumber = doc.SD_84.SF_OUT_NUM,
                    SummaIn = doc.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD * (_.TD_84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / (decimal)_.TD_84.SFT_KOL),
                    SummaDiler = doc.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD * (_.TD_84.SFT_NACENKA_DILERA ?? 0) / (decimal)_.TD_84.SFT_KOL),
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.SD_84.SF_CLIENT_DC) as Kontragent,
                    Creator = doc.SD_84.CREATOR,
                    Note = doc.SD_84.SF_NOTE
                };
                newItem.DocInfo = myProjectRepository.GetDocDescription(DocumentType.InvoiceClient, newItem);
                Rows.Add(newItem);
            }
        }

        foreach (var r in Rows)
        {
            r.DocInfo = myProjectRepository.GetDocDescription(r.DocumentType, r);
        }
    }

    public ICommand AddRowCommand
    {
        get { return new Command(AddRow, _ => CurrentRow is not null); }
    }

    private void AddRow(object obj)
    {
        var ids = SelectedMainRows.Select(_ => _.Id).ToList();
        foreach (var r in from id in ids
                 where SelectedRows.All(_ => _.Id != id)
                 select Rows.FirstOrDefault(_ => _.Id == id)
                 into r
                 where r != null
                 select r)
            SelectedRows.Add(r);
        foreach (var r in ids.Select(id => Rows.FirstOrDefault(_ => _.Id == id)).Where(r => r != null)) Rows.Remove(r);
    }

    public ICommand DeleteRowCommand
    {
        get { return new Command(DeleteRow, _ => CurrentRow is not null); }
    }

    private void DeleteRow(object obj)
    {
        var ids = new List<Guid>(SelectedActualRows.Select(_ => _.Id));
        foreach (var r in ids.Select(id => SelectedRows.FirstOrDefault(_ => _.Id == id)).Where(r => r is not null))
        {
            SelectedRows.Remove(r);
            Rows.Add(r);
        }
    }

    #endregion

    #region Fields

    private ProjectDocumentInfoBase myCurrentRow;
    private readonly Project myProject;
    private bool myIsBank;
    private bool myIsCashOrderIn;
    private bool myIsCashOrderOut;
    private bool myIsWarehouseIn;
    private bool myIsWaybill;
    private bool myIsDirectProvider;
    private bool myIsDirectClient;
    private ProjectDocumentInfoBase myCurrentSelectedRow;
    private DateTime myDateStart;
    private DateTime myDateEnd;

    private readonly IProjectRepository myProjectRepository;
    private bool myIsUslugaClient;
    private bool myIsUslugaProvider;

    #endregion

    #region Properties

    public UserControl CustomDataUserControl { set; get; } = new ProjectDocSelectDialogView();
    public override string LayoutName => "ProjectDocSelectDialogView";

    public ObservableCollection<ProjectDocumentInfoBase> Rows { set; get; } =
        new ObservableCollection<ProjectDocumentInfoBase>();

    public ObservableCollection<ProjectDocumentInfoBase> SelectedRows { set; get; } =
        new ObservableCollection<ProjectDocumentInfoBase>();

    public ObservableCollection<ProjectDocumentInfoBase> SelectedMainRows { set; get; } =
        new ObservableCollection<ProjectDocumentInfoBase>();

    public ObservableCollection<ProjectDocumentInfoBase> SelectedActualRows { set; get; } =
        new ObservableCollection<ProjectDocumentInfoBase>();

    public ProjectDocumentInfoBase CurrentRow
    {
        get => myCurrentRow;
        set
        {
            if (Equals(value, myCurrentRow)) return;
            myCurrentRow = value;
            RaisePropertyChanged();
        }
    }

    public ProjectDocumentInfoBase CurrentSelectedRow
    {
        get => myCurrentSelectedRow;
        set
        {
            if (Equals(value, myCurrentSelectedRow)) return;
            myCurrentSelectedRow = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DateStart
    {
        get => myDateStart;
        set
        {
            if (value.Equals(myDateStart)) return;
            myDateStart = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DateEnd
    {
        get => myDateEnd;
        set
        {
            if (value.Equals(myDateEnd)) return;
            myDateEnd = value;
            RaisePropertyChanged();
        }
    }

    public bool IsBank
    {
        set
        {
            if (value == myIsBank) return;
            myIsBank = value;
            RaisePropertyChanged();
        }
        get => myIsBank;
    }

    public bool IsCashOrderIn
    {
        set
        {
            if (value == myIsCashOrderIn) return;
            myIsCashOrderIn = value;
            RaisePropertyChanged();
        }
        get => myIsCashOrderIn;
    }

    public bool IsCashOrderOut
    {
        set
        {
            if (value == myIsCashOrderOut) return;
            myIsCashOrderOut = value;
            RaisePropertyChanged();
        }
        get => myIsCashOrderOut;
    }

    public bool IsWarehouseIn
    {
        set
        {
            if (value == myIsWarehouseIn) return;
            myIsWarehouseIn = value;
            RaisePropertyChanged();
        }
        get => myIsWarehouseIn;
    }

    public bool IsWaybill
    {
        set
        {
            if (value == myIsWaybill) return;
            myIsWaybill = value;
            RaisePropertyChanged();
        }
        get => myIsWaybill;
    }

    public bool IsDirectProvider
    {
        set
        {
            if (value == myIsDirectProvider) return;
            myIsDirectProvider = value;
            RaisePropertyChanged();
        }
        get => myIsDirectProvider;
    }

    public bool IsDirectClient
    {
        set
        {
            if (value == myIsDirectClient) return;
            myIsDirectClient = value;
            RaisePropertyChanged();
        }
        get => myIsDirectClient;
    }


    public bool IsUslugaClient
    {
        set
        {
            if (value == myIsUslugaClient) return;
            myIsUslugaClient = value;
            RaisePropertyChanged();
        }
        get => myIsUslugaClient;
    }

    public bool IsUslugaProvider
    {
        set
        {
            if (value == myIsUslugaProvider) return;
            myIsUslugaProvider = value;
            RaisePropertyChanged();
        }
        get => myIsUslugaProvider;
    }

    #endregion
}
