using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.AktSpisaniya;
using KursAM2.ViewModel.Signatures;
using KursDomain;
using KursDomain.Documents.AktSpisaniya;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;
using Newtonsoft.Json;

// ReSharper disable IdentifierTypo

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class AktSpisaniyaNomenklTitleWIndowViewModel : RSWindowViewModelBase
    {
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        #region Constructors

        public AktSpisaniyaNomenklTitleWIndowViewModel(Guid? id = null)
        {
            GenericRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklTitleRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            SignatureRepository = new SignatureRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? GenericRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = Document =
                    new AktSpisaniyaNomenklTitleViewModel(AktSpisaniyaNomenklTitleRepository.CreateNew())
                    {
                        State = RowStatus.NewRow
                    };
            }
            else
            {
                Document = new AktSpisaniyaNomenklTitleViewModel(doc)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                foreach (var r in Document.Rows)
                {
                    var q = nomenklManager.GetNomenklQuantity(Document.Warehouse.DocCode, r.Nomenkl.DocCode,
                        Document.DocDate,
                        Document.DocDate);
                    var quan = q.Count > 0 ? q.First().OstatokQuantity : 0;
                    r.Prices = nomenklManager.GetNomenklPrice(r.Nomenkl.DocCode, Document.DocDate);
                    r.MaxQuantity = r.Quantity + quan;
                    r.RaisePropertyAllChanged();
                    r.myState = RowStatus.NotEdited;
                }
            }

            AktSpisaniyaNomenklTitleRepository.AktSpisaniya = Document.Entity;
            var signs = SignatureRepository.CreateSignes(72, Document.Id, out var issign, out var isSignNew);
            IsSigned = issign;
            foreach (var s in signs) SignatureRows.Add(s);
            if (Document.myState != RowStatus.NewRow)
                Document.myState = isSignNew ? RowStatus.Edited : RowStatus.NotEdited;
        }

        #endregion

        #region Methods

        public override bool IsCorrect()
        {
            return Document.IsCorrect();
        }

        private void RaiseAll()
        {
            if (Document != null)
            {
                Document.RaisePropertyAllChanged();
                foreach (var r in Document.Rows) r.RaisePropertyAllChanged();
            }
        }

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            unitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.DocCode = -1;
            Document.DocDate = DateTime.Today;
            Document.DocCreator = GlobalOptions.UserInfo.Name;
            Document.myState = RowStatus.NewRow;
            Document.Id = newId;
            unitOfWork.Context.AktSpisaniyaNomenkl_Title.Add(Document.Entity);
            if (isCopy)
            {
                var newCode = 1;
                foreach (var item in Document.Rows)
                {
                    unitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    item.DocCode = -1;
                    item.Id = Guid.NewGuid();
                    item.DocId = newId;
                    item.DocCode = newCode;
                    item.Nomenkl = item.Nomenkl;
                    item.State = RowStatus.NewRow;
                    newCode++;
                }

                foreach (var r in Document.Rows)
                {
                    unitOfWork.Context.AktSpisaniya_row.Add(r.Entity);
                    r.State = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    unitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.AktSpisaniya_row.Clear();
                }

                Document.Rows.Clear();
            }
        }

        public void ResetVisibleCurrency()
        {
            if (!(Form is AktSpisaniyaView f)) return;
            var bandCrs = f.gridRows.Bands.FirstOrDefault(_ => (string)_.Header == "Валюта");
            if (bandCrs != null)
                foreach (var b in bandCrs.Bands)
                    b.Visible = Document.Rows.Any(_ => _.Currency.Name == (string)b.Header);
        }

        #endregion

        #region Fields

        // ReSharper disable once InconsistentNaming
        private AktSpisaniyaNomenklTitleViewModel myDocument;

        // ReSharper disable once InconsistentNaming
        private Guid myId;
        private bool myIsSigned;

        // ReSharper disable once InconsistentNaming
        private AktSpisaniyaRowViewModel myCurrentRow;

        // ReSharper disable once InconsistentNaming
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(GlobalOptions.GetEntities());

        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> GenericRepository;
        public readonly IAktSpisaniyaNomenkl_TitleRepository AktSpisaniyaNomenklTitleRepository;

        public readonly ISignatureRepository SignatureRepository;

        #endregion

        #region Properties

        public override string MenuInfoString => "Акцептован";
        public override string LayoutName => "AktSpisaniyaNomenklTitleView";

        public override string WindowName =>
            Document == null
                ? "Акт списания материалов(новый)"
                : $"Акт списания материалов №{Document?.DocNumber} от {Document?.DocDate}";

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<AktSpisaniyaRowViewModel> SelectedRows { set; get; }
            = new ObservableCollection<AktSpisaniyaRowViewModel>();

        public ObservableCollection<SignatureViewModel> SignatureRows { set; get; } =
            new ObservableCollection<SignatureViewModel>();

        public List<Currency> CurrencyList =>
            GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>().ToList();

        public override Visibility IsMenuInfoVisibility => IsSigned ? Visibility.Visible : Visibility.Hidden;

        public bool IsSigned
        {
            get => myIsSigned;
            set
            {
                if (myIsSigned == value) return;
                myIsSigned = value;
                Document.IsSign = myIsSigned;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsMenuInfoVisibility));
                if (Form is AktSpisaniyaView view)
                {
                    var col = view.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Quantity");
                    if (col != null) col.ReadOnly = !IsSigned;
                }
            }
        }

        private SignatureViewModel myCurrentSignature;

        public SignatureViewModel CurrentSignature
        {
            get => myCurrentSignature;
            set
            {
                if (myCurrentSignature == value) return;
                myCurrentSignature = value;
                RaisePropertyChanged();
            }
        }

        public List<KursDomain.References.Warehouse> WarehouseList { set; get; }
            = GlobalOptions.ReferencesCache.GetWarehousesAll().Cast<KursDomain.References.Warehouse>()
                .OrderBy(_ => _.Name).ToList();

        public AktSpisaniyaNomenklTitleViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value)
                    return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public AktSpisaniyaRowViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value)
                    return;
                myCurrentRow = value;
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

        #region Command

        public bool IsWarehouseCanChanged => Document.State == RowStatus.NewRow && Document.Rows.Count == 0;
        public bool IsCurrencyCanChanged => Document.State == RowStatus.NewRow && Document.Rows.Count == 0;

        public override bool IsDocDeleteAllow => Document.State != RowStatus.NewRow;

        public override bool IsCanSaveData =>
            Document != null && Document.State != RowStatus.NotEdited
                             && Document.Rows.All(_ => _.Quantity <= _.MaxQuantity)
                             && Document.Warehouse != null
                             && Document.Currency != null;

        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.NewRow)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "Вы уверены, что хотите удалить документ?";
                var res = service.ShowDialog(MessageButton.YesNo, "Запрос", this);
                switch (res)
                {
                    case MessageResult.Yes:
                        unitOfWork.CreateTransaction();
                        var dsign = SignatureRepository.UnitOfWork.Context
                            .DocumentSignatures.FirstOrDefault(_ => _.DocId == Document.Id);
                        if (dsign != null)
                            SignatureRepository.UnitOfWork.Context
                                .DocumentSignatures.Remove(dsign);
                        AktSpisaniyaNomenklTitleRepository.Delete();
                        unitOfWork.Save();
                        unitOfWork.Commit();
                        DocumentsOpenManager.DeleteFromLastDocument(Document.Id, null);
                        Form?.Close();
                        return;
                }
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
                switch (res)
                {
                    case MessageResult.Yes:
                        SaveData(null);
                        return;
                }
            }

            foreach (var id in Document.Rows.Where(_ => _.State == RowStatus.NewRow).Select(_ => _.Id)
                         .ToList())
                Document.Rows.Remove(Document.Rows.Single(_ => _.Id == id));
            EntityManager.EntityReload(unitOfWork.Context);
            foreach (var r in Document.Rows)
            {
                var q = nomenklManager.GetNomenklQuantity(Document.Warehouse.DocCode, r.Nomenkl.DocCode,
                    Document.DocDate,
                    Document.DocDate);
                var quan = q.Count > 0 ? q.First().OstatokQuantity : 0;
                r.Prices = nomenklManager.GetNomenklPrice(r.Nomenkl.DocCode, Document.DocDate);
                r.MaxQuantity = r.Quantity + quan;
                r.myState = RowStatus.NotEdited;
                r.RaisePropertyAllChanged();
            }

            SignatureRows.Clear();
            // ReSharper disable once UnusedVariable
            var signs = SignatureRepository.CreateSignes(72, Document.Id, out var issign, out var isSignNew);
            foreach (var s in signs)
                SignatureRows.Add(s);
            IsSigned = SignatureRows.Where(_ => _.IsRequired).All(x => x.UserId != null);
            RaiseAll();
            ResetVisibleCurrency();
            Document.myState = isSignNew ? RowStatus.Edited : RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
        }

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow)
                Document.DocNumber = unitOfWork.Context.AktSpisaniyaNomenkl_Title.Any()
                    ? unitOfWork.Context.AktSpisaniyaNomenkl_Title.Max(_ => _.Num_Doc) + 1
                    : 1;

            unitOfWork.CreateTransaction();
            var oldSign = unitOfWork.Context.DocumentSignatures.FirstOrDefault(_ => _.DocId == Document.Id);
            if (oldSign == null)
            {
                unitOfWork.Context.DocumentSignatures.Add(new DocumentSignatures
                {
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Signatures = JsonConvert.SerializeObject(SignatureRows.Select(_ => _.ToJson())),
                    DocumentTypeId = 72,
                    IsSign = IsSigned
                });
            }
            else
            {
                oldSign.Signatures = JsonConvert.SerializeObject(SignatureRows.Select(_ => _.ToJson()));
                oldSign.IsSign = IsSigned;
            }

            unitOfWork.Save();
            NomenklCalculationManager.InsertNomenklForCalc(unitOfWork.Context,
                Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList());
            NomenklCalculationManager.CalcAllNomenklRemains(unitOfWork.Context);
            foreach (var n in Document.Rows.Select(_ => ((IDocCode)_.Nomenkl).DocCode))
            {
                var c = NomenklCalculationManager.GetNomenklStoreRemain(unitOfWork.Context, Document.DocDate,
                    n, Document.Warehouse.DocCode);
                if (c < 0)
                {
                    unitOfWork.Rollback();
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(n) as Nomenkl;
                    // ReSharper disable once PossibleNullReferenceException
                    WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                              // ReSharper disable once PossibleInvalidOperationException
                                              $"склад {Document.Warehouse} в кол-ве {c} ",
                        "Отрицательные остатки", MessageBoxImage.Error);
                    return;
                }
            }

            unitOfWork.Commit();
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.AktSpisaniya), Document.Id,
                0, null, (string)Document.ToJson());
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
            Document.myState = RowStatus.NotEdited;
            // ReSharper disable once UseNameofExpression
            Document.RaisePropertyChanged("State");
        }

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.AktSpisaniya, Document.Id, 0, null);
        }

        public ICommand SignedCommand
        {
            get
            {
                return new Command(Signed, _ => CurrentSignature != null
                                                && IsCanSigned());
            }
        }

        private bool IsCanSigned()
        {
            var isUser = CurrentSignature.UsersCanSigned.Any(_ => _.Id == GlobalOptions.UserInfo.KursId);

            return SignatureRows.All(_ => _.ParentId != CurrentSignature.Id) || (SignatureRows
                .Where(_ => _.ParentId == CurrentSignature.Id).All(x => x.UserId != null) && isUser);
        }

        private void Signed(object obj)
        {
            CurrentSignature.UserId = GlobalOptions.UserInfo.KursId;
            CurrentSignature.SignUserName = GlobalOptions.UserInfo.NickName;
            CurrentSignature.SignUserFullName = GlobalOptions.UserInfo.FullName;
            IsSigned = SignatureRows.Where(_ => _.IsRequired).All(x => x.UserId != null);
            if (Document.State != RowStatus.NewRow)
                Document.State = RowStatus.Edited;
        }

        public ICommand UnSignedCommand
        {
            get
            {
                return new Command(UnSigned,
                    _ => CurrentSignature != null && CurrentSignature.UserId != null && IsCanSigned());
            }
        }

        private void UnSigned(object obj)
        {
            CurrentSignature.UserId = null;
            CurrentSignature.SignUserName = null;
            CurrentSignature.SignUserFullName = null;
            IsSigned = SignatureRows.Where(_ => _.IsRequired).All(x => x.UserId != null);
            if (Document.State != RowStatus.NewRow)
                Document.State = RowStatus.Edited;
        }

        public ICommand AddNomenklCommand
        {
            get
            {
                return new Command(AddNomenkl, _ => Document?.Warehouse != null
                                                    && Document?.Currency != null
                                                    && IsSigned == false);
            }
        }

        private void AddNomenkl(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            var ctx = new DialogSelectExistNomOnSkaldViewModel(Document.Warehouse, Document.DocDate,
                Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList(),
                Document?.Currency);
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, $"Запрос для склада: {Document.Warehouse}", ctx) ==
                MessageResult.Cancel) return;
            if (ctx.NomenklSelectedList.Count == 0) return;
            foreach (var n in ctx.NomenklSelectedList)
            {
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.Nomenkl.DocCode))
                {
                    var nPrice = nomenklManager.GetNomenklPrice(n.Nomenkl.DocCode, Document.DocDate);
                    var newItem = new AktSpisaniyaRowViewModel
                    {
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n.Nomenkl,
                        Quantity = 1,
                        MaxQuantity = n.Quantity,
                        Prices = nPrice,
                        Code = newCode,
                        State = RowStatus.NewRow,
                        Parent = this
                    };
                    Document.Rows.Add(newItem);
                    Document.Entity.AktSpisaniya_row.Add(newItem.Entity);
                    if (Document.State != RowStatus.NewRow)
                        Document.State = RowStatus.Edited;
                }

                newCode++;
            }

            ResetVisibleCurrency();
            RaisePropertyChanged(nameof(Document));
        }

        public ICommand DeleteNomenklCommand
        {
            get
            {
                return new Command(DeleteRow, _ => (CurrentRow != null || SelectedRows.Count > 1) && IsSigned == false);
            }
        }

        private void DeleteRow(object obj)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Хотите удалить выделенные строки?";
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", this) == MessageResult.No)
                return;
            var list = SelectedRows.Select(_ => _.Id).ToList();
            foreach (var id in list)
            {
                var row = Document.Rows.Single(_ => _.Id == id);
                Document.Rows.Remove(row);
                unitOfWork.Context.Entry(row.Entity).State =
                    unitOfWork.Context.Entry(row.Entity).State == EntityState.Added
                        ? EntityState.Detached
                        : EntityState.Deleted;
            }

            Document.State = RowStatus.Edited;
            ResetVisibleCurrency();
        }

        public ICommand ResetSignatureSchemaCommand => new Command(ResetSignatureSchema,
            _ => SignatureRows.Where(x => x.IsRequired)
                .All(x => x.UserId == null));

        private void ResetSignatureSchema(object obj)
        {
            var signs = SignatureRepository.ResetSignes(72);
            IsSigned = false;
            SignatureRows.Clear();
            foreach (var s in signs) SignatureRows.Add(s);
            if (Document.myState != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel
            {
                Form = frm,
                State = RowStatus.NewRow
            };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (Document == null)
                return;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(Document.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };

            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (Document == null)
                return;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(Document.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };

            ctx.Form = frm;
            frm.Show();
        }

        #endregion
    }
}
