using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AutoMapper.Internal;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Dogovora;
using Core.EntityViewModel.Invoices;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.View.Helper;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Methods

        public override bool IsCorrect()
        {
            return Document.IsCorrect() && Document.Rows.All(_ => _.IsCorrect());
        }

        #endregion

        #region Fields

        private DogovorClientViewModel myDocument;
        private Guid myId;
        private DogovorClientFactViewModel myCurrentFactRow;
        private DogovorClientRowViewModel myCurrentRow;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorClient> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorClientRepository DogovorClientRepository;
        private LinkDocumentInfo myCurrentLinkDocument;

        #endregion

        #region Constructors

        public DogovorClientWindowViewModel()
        {
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Договор клиенту";
            Document = new DogovorClientViewModel(DogovorClientRepository.CreateNew(), RowStatus.NewRow);
            Id = Document.Id;
        }

        public DogovorClientWindowViewModel(Guid id)
        {
            Id = id;
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            RefreshData(id);
        }

        public override string LayoutName => "DogovorClientView";

        #endregion

        #region Properties

        public ObservableCollection<DogovorClientFactViewModel> FactsAll { set; get; } =
            new ObservableCollection<DogovorClientFactViewModel>();

        public ObservableCollection<LinkDocumentInfo> Documents { set; get; } =
            new ObservableCollection<LinkDocumentInfo>();

        public ObservableCollection<InvoicePaymentDocument> PaymentList { set; get; } =
            new ObservableCollection<InvoicePaymentDocument>();

        public ObservableCollection<DogovorResult> Results { set; get; } =
            new ObservableCollection<DogovorResult>();


        public override string WindowName => Document == null
            ? "Договор клиенту"
            : $"{Document} Отгружено: {FactsAll.Sum(_ => _.Summa)} Оплачено: {PaymentList.Sum(_ => _.Summa)}";

        public DogovorClientViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DogovorClientRowViewModel> SelectedRows { set; get; }
            = new ObservableCollection<DogovorClientRowViewModel>();

        public List<ContractType> ContractTypeList => MainReferences.ContractTypes.Values.Where(_ => _.IsSale).ToList();
        public List<PayCondition> PayConditionList => MainReferences.PayConditions.Values.ToList();
        public List<FormPay> FormPayList => MainReferences.FormRaschets.Values.ToList();

        public DogovorClientRowViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public LinkDocumentInfo CurrentLinkDocument
        {
            get => myCurrentLinkDocument;
            set
            {
                if (myCurrentLinkDocument == value) return;
                myCurrentLinkDocument = value;
                RaisePropertyChanged();
            }
        }

        public DogovorClientFactViewModel CurrentFactRow
        {
            get => myCurrentFactRow;
            set
            {
                if (myCurrentFactRow == value) return;
                myCurrentFactRow = value;
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            get => myId;
            set
            {
                if (myId == value) return;
                myId = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.DogovorClient,Document.Id,null, null);

        }

        public override bool IsCanSaveData =>
            Document != null && Document.State != RowStatus.NotEdited && Document.IsCorrect();

        public ICommand CalcResultCommand
        {
            get { return new Command(CalcResult, _ => true); }
        }

        private void CalcResult(object obj)
        {
        }

        public ICommand OpenSFactCommand
        {
            get { return new Command(OpenSFact, _ => CurrentFactRow != null); }
        }

        private void OpenSFact(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentFactRow.SFactDC);
        }

        public ICommand OpenWayBillCommand
        {
            get { return new Command(OpenWayBill, _ => CurrentFactRow != null); }
        }

        private void OpenWayBill(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.Waybill, CurrentFactRow.WayBillDC);
        }

        public ICommand OpenLinkDocumentCommand
        {
            get { return new Command(OpenLinkDocument, _ => CurrentLinkDocument != null); }
        }

        private void OpenLinkDocument(object obj)
        {
            DocumentsOpenManager.Open(CurrentLinkDocument.DocumentType,
                CurrentLinkDocument.DocumentType == DocumentType.Bank
                    ? CurrentLinkDocument.Code
                    : CurrentLinkDocument.DocCode);
        }


        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document?.Currency != null); }
        }

        private void AddNomenkl(object obj)
        {
            decimal defaultNDS;
            var nomenkls = StandartDialogs.SelectNomenkls(Document.Currency, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            using (var entctx = GlobalOptions.GetEntities())
            {
                defaultNDS = Convert.ToDecimal(entctx.PROFILE
                    .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
            }

            foreach (var newRow in from n in nomenkls
                where Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode)
                select new DogovorClientRowViewModel
                {
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Nomenkl = n,
                    Quantity = 1,
                    Price = 0,
                    IsCalcBack = Document.IsCalcBack,
                    NDSPercent = n.NDSPercent ?? defaultNDS,
                    Parent = Document,
                    State = RowStatus.NewRow,
                    Facts = new ObservableCollection<DogovorClientFactViewModel>()
                })
            {
                Document.Entity.DogovorClientRow.Add(newRow.Entity);
                Document.Rows.Add(newRow);
            }

            RaisePropertyChanged(nameof(State));
        }

        public ICommand AddUslugaCommand
        {
            get { return new Command(AddUsluga, _ => true); }
        }

        private void AddUsluga(object obj)
        {
            var k = Document.Currency == null
                ? StandartDialogs.SelectNomenkls()
                : StandartDialogs.SelectNomenkls(Document.Currency);
            if (k != null)
            {
                decimal defaultNDS;
                using (var entctx = GlobalOptions.GetEntities())
                {
                    defaultNDS = Convert.ToDecimal(entctx.PROFILE
                        .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
                }

                foreach (var newRow in from n in k
                    where Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode)
                    select new DogovorClientRowViewModel
                    {
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n,
                        Quantity = 1,
                        Price = 0,
                        IsCalcBack = Document.IsCalcBack,
                        NDSPercent = n.NDSPercent ?? defaultNDS,
                        Parent = Document,
                        State = RowStatus.NewRow,
                        Facts = new ObservableCollection<DogovorClientFactViewModel>()
                    })
                {
                    Document.Entity.DogovorClientRow.Add(newRow.Entity);
                    Document.Rows.Add(newRow);
                }
            }

            if (Document.State != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }

        public ICommand DeleteRowCommand
        {
            get { return new Command(DeleteRow, _ => SelectedRows.Count > 0); }
        }

        private void DeleteRow(object obj)
        {
            if (WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить строки", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var list = SelectedRows.Select(_ => _.Id).ToList();
                foreach (var id in list)
                {
                    var row = Document.Rows.Single(_ => _.Id == id);
                    if (FactsAll.All(_ => _.Nomenkl.DocCode != row.Nomenkl.DocCode))
                    {
                        Document.Rows.Remove(row);
                        unitOfWork.Context.Entry(row.Entity).State =
                            unitOfWork.Context.Entry(row.Entity).State == EntityState.Added
                                ? EntityState.Detached
                                : EntityState.Deleted;
                    }
                }

                Document.State = RowStatus.Edited;
            }
        }


        public override void RefreshData(object obj)
        {
            if (Document != null) EntityManager.ContextClear(unitOfWork.Context);
            Document = new DogovorClientViewModel(DogovorClientRepository.GetByGuidId(Id));
            DogovorClientRepository.Dogovor = Document.Entity;
            FactsAll.Clear();
            foreach (var f in DogovorClientRepository.GetOtgruzkaInfo().ToList()) FactsAll.Add(f);
            Documents.Clear();
            foreach (var d in DogovorClientRepository.GetLinkDocuments().ToList()) Documents.Add(d);
            PaymentList.Clear();
            foreach (var p in DogovorClientRepository.GetPayments()) PaymentList.Add(p);
            if (Document.IsCalcBack) return;
            var factNoms = FactsAll.Select(_ => _.Nomenkl.DocCode).Distinct().ToList();
            var dogNoms = Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList();
            var noms = factNoms.Except(dogNoms).ToList();
            if (noms.Count == 0) return;
            if (WinManager.ShowWinUIMessageBox(
                "В счетах присутствуеют номенклатуры, не указанные в договоре. Добавить?",
                "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                decimal defaultNDS;
                using (var entctx = GlobalOptions.GetEntities())
                {
                    defaultNDS = Convert.ToDecimal(entctx.PROFILE
                        .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
                }

                foreach (var n in noms)
                {
                    var newRow = new DogovorClientRowViewModel
                    {
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = MainReferences.GetNomenkl(n),
                        Quantity = 1,
                        Price = 0,
                        IsCalcBack = Document.IsCalcBack,
                        NDSPercent = MainReferences.GetNomenkl(n).NDSPercent ?? defaultNDS,
                        Parent = Document,
                        State = RowStatus.NewRow,
                        Facts = new ObservableCollection<DogovorClientFactViewModel>()
                    };
                    Document.Entity.DogovorClientRow.Add(newRow.Entity);
                    Document.Rows.Add(newRow);
                }
            }
        }

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow && string.IsNullOrWhiteSpace(Document.DogNum))
            {
                var n = unitOfWork.Context.DogovorClient.Count() + 1;
                Document.DogNum = n.ToString();
            }

            try
            {
                unitOfWork.CreateTransaction();
                unitOfWork.Save();
                unitOfWork.Commit();
                Document.Rows.ForAll(_ => _.myState = RowStatus.NotEdited);
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        public ICommand KontragentSelectCommand
        {
            get { return new Command(KontragentSelect, _ => FactsAll.Count == 0); }
        }

        private void KontragentSelect(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent();
            if (kontr == null) return;
            Document.Client = kontr;
        }

        #endregion

        #region IDataErrorInfo

        //public string this[string columnName]
        //{
        //    get
        //    {
        //        switch (columnName)
        //        {
        //            case "Document.Client":
        //                return Document.Client == null ? "Клиент должен быть обязательно выбран" : null;
        //            default:
        //                return null;

        //        } 
        //    }
        //}

        public string this[string columnName] => null;

        [Display(AutoGenerateField = false)] public string Error { get; } = "";

        #endregion
    }
}