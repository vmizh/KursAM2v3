using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.View.Dogovors;
using KursAM2.View.Helper;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Dogovora;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using ContractType = KursDomain.Documents.Dogovora.ContractType;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructors

        public DogovorClientWindowViewModel(Guid? id)
        {
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            //DialogService = GetService<IDialogService>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? BaseRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = new DogovorClientViewModel { State = RowStatus.NewRow };
                unitOfWork.Context.DogovorClient.Add(Document.Entity);
            }
            else
            {
                Document = new DogovorClientViewModel(doc)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                foreach (var r in Document.Rows)
                    r.State = RowStatus.NotEdited;
                //LoadLinkDocuments();
                Document.myState = RowStatus.NotEdited;
            }
        }

        #endregion

        #region Methods

        public override bool IsCorrect()
        {
            return Document.IsCorrect() && Document.Rows.All(_ => _.IsCorrect());
        }

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            unitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.Id = newId;
            Document.DogDate = DateTime.Today;
            Document.Creator = GlobalOptions.UserInfo.Name;
            Document.myState = RowStatus.NewRow;
            Document.Rows.Clear();
            unitOfWork.Context.DogovorClient.Add(Document.Entity);
            if (isCopy)
            {
                foreach (var row in Document.Rows)
                {
                    unitOfWork.Context.Entry(row.Entity).State = EntityState.Detached;
                    row.Id = Guid.NewGuid();
                    row.DocId = newId;
                    Document.Entity.DogovorClientRow.Add(row.Entity);
                    row.myState = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    unitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.DogovorClientRow.Clear();
                }

                Document.Rows.Clear();
            }
        }

        private void RaiseAll()
        {
            Document.RaisePropertyAllChanged();
            foreach (var r in Document.Rows) r.RaisePropertyAllChanged();
        }

        #endregion

        #region Fields

        private DogovorClientViewModel myDocument;
        private Guid myId;
        private DogovorClientFactViewModel myCurrentFactRow;
        private DogovorClientRowViewModel myCurrentRow;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorClient> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorClientRepository DogovorClientRepository;
        private LinkDocumentInfo myCurrentLinkDocument;

        #endregion

        #region Properties

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DogovorClientFactViewModel> FactsAll { set; get; } =
            new ObservableCollection<DogovorClientFactViewModel>();

        public ObservableCollection<LinkDocumentInfo> Documents { set; get; } =
            new ObservableCollection<LinkDocumentInfo>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<InvoicePaymentDocument> PaymentList { set; get; } =
            new ObservableCollection<InvoicePaymentDocument>();

        public ObservableCollection<DogovorResult> Results { set; get; } =
            new ObservableCollection<DogovorResult>();

        public override string LayoutName => "DogovorClientWindowViewModel";

        public override string WindowName => Document?.State == RowStatus.NewRow
            ? "Договор клиенту (новый)"
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

        public List<ContractType> ContractTypeList => GlobalOptions.ReferencesCache.GetContractsTypeAll()
            .Cast<ContractType>().Where(_ => _.IsSale).ToList();

        public List<PayCondition> PayConditionList =>
            GlobalOptions.ReferencesCache.GetPayConditionAll().Cast<PayCondition>().ToList();

        public List<PayForm> FormPayList => GlobalOptions.ReferencesCache.GetPayFormAll() as List<PayForm>;

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

        public override void DocNewCopy(object form)
        {
            var ctx = new DogovorClientWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(true);
            var frm = new DogovorClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new DogovorClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new DogovorClientWindowViewModel(null)
            {
                Form = view,
                ParentFormViewModel = this
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var ctx = new DogovorClientWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(false);
            var frm = new DogovorClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.DogovorClient, Document.Id, null);
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
                         NDSPercent = n.DefaultNDSPercent ?? defaultNDS,
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
                             NDSPercent = n.DefaultNDSPercent ?? defaultNDS,
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
            var winManager = new WindowManager();
            if (winManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить строки", "Запрос",
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
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }

            foreach (var id in Document.Rows.Where(_ => _.State == RowStatus.NewRow).Select(_ => _.Id)
                         .ToList())
                Document.Rows.Remove(Document.Rows.Single(_ => _.Id == id));
            EntityManager.EntityReload(unitOfWork.Context);
            foreach (var entity in unitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
            RaiseAll();
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
            Document.myState = RowStatus.NotEdited;
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
                foreach (var r in Document.Rows)
                    r.myState = RowStatus.NotEdited;
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
