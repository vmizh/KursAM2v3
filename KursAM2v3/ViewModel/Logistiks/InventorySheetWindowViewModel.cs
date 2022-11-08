using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers.Nomenkl;
using KursAM2.Repositories;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklGroup2 : NomenklGroup
    {
        private bool myIsNomenklContains;

        public bool IsNomenklContains
        {
            get => myIsNomenklContains;
            set
            {
                if (myIsNomenklContains == value) return;
                myIsNomenklContains = value;
            }
        }
    }

    public sealed class InventorySheetWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        public readonly GenericKursDBRepository<SD_24> GenericInventorySheetRepository;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        // ReSharper disable once NotAccessedField.Local
        public readonly ISD_24Repository SD_24Repository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private readonly NomenklManager myNomenklManager = new NomenklManager();
        private InventorySheetRowViewModel myCurrentNomenkl;
        private NomenklGroup2 myCurrentNomenklGroup;
        private InventorySheetViewModel myDocument;
        private bool myIsAllNomekl;
        private bool myIsGroupEnabled;
        private ObservableCollection<InventorySheetRowViewModel> myRows;

        #endregion

        #region Constructors

        public InventorySheetWindowViewModel()
        {
            GenericInventorySheetRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            // ReSharper disable once VirtualMemberCallInContructor
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            LoadNomenklGroup();
            IsAllNomekl = true;
        }

        public InventorySheetWindowViewModel(decimal? docCode = null) : this()
        {
            if (docCode == null)
            {
                Document = new InventorySheetViewModel(SD_24Repository.CreateNew(MatearialDocumentType.InventorySheet));
            }
            else
            {
                Document = new InventorySheetViewModel(SD_24Repository.GetByDC(docCode.Value));
                myState = RowStatus.NotEdited;
            }
        }

        #endregion

        #region Properties

        public override string WindowName => Document.State == RowStatus.NewRow
            ? "Инвентаризационная ведомость (новая)"
            : $"Инвентаризационная ведомость №{Document.Num} от {Document.Date.ToShortDateString()}" +
              $" на склад {Document.Warehouse}";

        public override string LayoutName => "InventorySheetWindowViewModel";

        public List<KursDomain.Documents.NomenklManagement.Warehouse> StoreCollection { set; get; } =
            new List<KursDomain.Documents.NomenklManagement.Warehouse>(MainReferences.Warehouses.Values);

        public bool IsCannotChangeStore => Document.State == RowStatus.NewRow;

        public InventorySheetViewModel Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument ,value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public bool IsGroupEnabled
        {
            get => myIsGroupEnabled;
            set
            {
                if (Equals(myIsGroupEnabled, value)) return;
                myIsGroupEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAllNomekl
        {
            get => myIsAllNomekl;
            set
            {
                if (myIsAllNomekl == value) return;
                myIsAllNomekl = value;
                IsGroupEnabled = !myIsAllNomekl;
                if (!IsAllNomekl)
                    UpdateNomenklGroupsForContainsNomenkls();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Rows));
            }
        }

        public ObservableCollection<InventorySheetRowViewModel> Rows
        {
            get
            {
                if (Document == null) return myRows;
                if (IsAllNomekl)
                    return Document.Rows;
                if (CurrentNomenklGroup == null) return null;
                var listDC = GetNomenklGroupChilds(CurrentNomenklGroup).Distinct().ToList();
                var ret = new ObservableCollection<InventorySheetRowViewModel>();
                foreach (var n in from dc in listDC
                         from n in Document.Rows
                         where ((IDocCode) n.Nomenkl.Group).DocCode == dc
                         select n
                        )
                    ret.Add(n);
                return ret;
            }
            set
            {
                myRows = value;
                RaisePropertiesChanged();
            }
        }

        public ObservableCollection<NomenklGroup2> NomenklGroups { set; get; } = new ObservableCollection<NomenklGroup2>();

        public InventorySheetRowViewModel CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                if (Equals(myCurrentNomenkl,value)) return;
                myCurrentNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public NomenklGroup2 CurrentNomenklGroup
        {
            get => myCurrentNomenklGroup;
            set
            {
                //if (myCurrentNomenklGroup == value) return;
                myCurrentNomenklGroup = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Rows));
            }
        }

        #endregion

        #region Methods

        private List<decimal> GetNomenklGroupChilds(NomenklGroup2 grp)
        {
            var ret = new List<decimal>();
            ret.AddRange(
                Document.Rows.Where(_ => ((IDocCode) _.Nomenkl.Group).DocCode == grp.DocCode)
                    .Select(_ => ((IDocCode) _.Nomenkl.Group).DocCode));
            if (NomenklGroups.Count(_ => _.ParentDC == grp.DocCode) == 0) return ret;
            foreach (var g in NomenklGroups.Where(_ => _.ParentDC == grp.DocCode))
            {
                var gg = GetNomenklGroupChilds(g);
                if (gg != null && gg.Count > 0)
                    ret.AddRange(gg);
            }

            return ret;
        }

        private void LoadNomenklGroup()
        {
            NomenklGroups.Clear();
            foreach (var ng in myNomenklManager.SelectNomenklGroups())
                NomenklGroups.Add(new NomenklGroup2
                {
                    DocCode = ng.DocCode,
                    Name = ng.Name,
                    ParentDC = ng.ParentDC,
                    IsNomenklContains = false
                });
            RaisePropertyChanged(nameof(NomenklGroups));
        }

        private void SetIsContainNomenkl(NomenklGroup2 grp)
        {
            if (grp == null) return;
            grp.IsNomenklContains = true;
            SetIsContainNomenkl(NomenklGroups.FirstOrDefault(_ => _.DocCode == grp.ParentDC));
        }

        private void UpdateNomenklGroupsForContainsNomenkls()
        {
            foreach (var ng in NomenklGroups)
                ng.IsNomenklContains = false;
            foreach (
                var ng in
                Document.Rows.Select(_ => ((IDocCode) _.Nomenkl.Group).DocCode)
                    .Distinct()
                    .ToList()
                    .Select(ngDC => NomenklGroups.FirstOrDefault(_ => _.DocCode == ngDC))
                    .Where(ng => ng != null))
                SetIsContainNomenkl(ng);
            RaisePropertyChanged(nameof(NomenklGroups));
        }

        #endregion

        #region IDataErrorInfo

        #endregion

        #region Commands

        public override bool IsRedoAllow => Document != null && Document.DeletedRows.Count > 0;

        public override bool IsCanSaveData =>
            Document != null && Document.State != RowStatus.NotEdited && Document.Warehouse != null;

        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsDocDeleteAllow => true;

        public override void ResetLayout(object form)
        {
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
                switch (res)
                {
                    case MessageResult.Yes:
                        SaveData(null);
                        break;
                }
            }

            Form?.Close();
            using (var sctx = GlobalOptions.KursSystem())
            {
                var oldLayout = sctx.FormLayout.FirstOrDefault(_ =>
                    _.UserId == GlobalOptions.UserInfo.KursId && _.ControlName == LayoutName);
                if (oldLayout != null)
                {
                    sctx.FormLayout.Remove(oldLayout);
                    sctx.SaveChanges();
                }
            }

            var f = new InventorySheetView2
            {
                DataContext = new InventorySheetWindowViewModel(Document.DocCode),
                Owner = Application.Current.MainWindow
            };
            ((InventorySheetWindowViewModel) f.DataContext).Form = f;
            f.Show();
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
                        break;
                }
            }

            if (Document != null) Document = new InventorySheetViewModel(SD_24Repository.GetByDC(Document.DocCode));

            LoadNomenklGroup();
            CurrentNomenklGroup = null;
            Document.myState = RowStatus.NotEdited;
        }

        public override void SaveData(object data)
        {
            try
            {
                if (Document.State == RowStatus.NewRow)
                {
                    Document.DocCode = UnitOfWork.Context.SD_24.Any()
                        ? UnitOfWork.Context.SD_24.Max(_ => _.DOC_CODE) + 1
                        : 10240000001;
                    Document.Num = UnitOfWork.Context.SD_24.Any(_ =>
                        _.DD_TYPE_DC == MatearialDocumentType.InventorySheet
                        && _.DD_DATE.Year == DateTime.Today.Year)
                        ? UnitOfWork.Context.SD_24.Where(_ => _.DD_TYPE_DC == MatearialDocumentType.InventorySheet
                                                              && _.DD_DATE.Year == DateTime.Today.Year)
                            .Max(_ => _.DD_IN_NUM) + 1
                        : 1;
                    foreach (var t in Document.Rows) t.DocCode = Document.DocCode;
                }

                UnitOfWork.CreateTransaction();

                UnitOfWork.Save();
                NomenklCalculationManager.InsertNomenklForCalc(UnitOfWork.Context,
                    Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList());
                NomenklCalculationManager.CalcAllNomenklRemains(UnitOfWork.Context);
                foreach (var n in Document.Rows.Select(_ => _.Nomenkl.DocCode))
                {
                    var c = NomenklCalculationManager.GetNomenklStoreRemain(UnitOfWork.Context, Document.Date,
                        n, Document.Warehouse.DocCode);
                    if (c < 0)
                    {
                        UnitOfWork.Rollback();
                        var nom = MainReferences.GetNomenkl(n);
                        WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                  // ReSharper disable once PossibleInvalidOperationException
                                                  $"склад {Document.Warehouse} в кол-ве {c} ",
                            "Отрицательные остатки", MessageBoxImage.Error);
                        return;
                    }
                }

                UnitOfWork.Commit();
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InventoryList), Document.Id,
                    0, null, (string) Document.ToJson());
                foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                Document.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        public override void Redo(object form)
        {
            if (Document.DeletedRows.Count == 0) return;
            foreach (var d in Document.DeletedRows)
                Rows.Add(d);
            Document.DeletedRows.Clear();
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Действительно хотите удалить ведомость?", "Запрос",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            var f = form as Window;
            switch (res)
            {
                case MessageBoxResult.Yes:
                    if (Document.State == RowStatus.NewRow)
                    {
                        f?.Close();
                        return;
                    }

                    try
                    {
                        UnitOfWork.CreateTransaction();
                        GenericInventorySheetRepository.Delete(Document.Entity);
                        UnitOfWork.Save();
                        UnitOfWork.Commit();
                        f?.Close();
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        UnitOfWork.Rollback();
                    }

                    return;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new InventorySheetView
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = new InventorySheetViewModel(SD_24Repository.CreateNew())
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var frm = new InventorySheetView
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = new InventorySheetViewModel(SD_24Repository.CreateRequisiteCopy(Document.DocCode))
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        public override void DocNewCopy(object form)
        {
            var frm = new InventorySheetView
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = new InventorySheetViewModel(SD_24Repository.CreateCopy(Document.DocCode))
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        public ICommand AddAllNomenklCommand
        {
            get { return new Command(AddAllNomenkl, _ => Document.Warehouse != null && !Document.IsClosed); }
        }

        private void AddAllNomenkl(object obj)
        {
            var noms = NomenklCalculationManager
                .GetNomenklStoreRemains(Document.Date, Document.Warehouse.DocCode)
                .Where(_ => _.Remain != 0)
                .Where(nom => Document.Rows.All(_ => _.Nomenkl.DocCode != nom.NomenklDC));
            // ReSharper disable once PossibleMultipleEnumeration
            if (noms.Any() && Document.State != RowStatus.NewRow) Document.myState = RowStatus.Edited;
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var nom in noms)
            {
                var newItem = new InventorySheetRowViewModel(null)
                {
                    DocCode = Document.DocCode,
                    Nomenkl = MainReferences.GetNomenkl(nom.NomenklDC),
                    QuantityFact = nom.Remain,
                    QuantityCalc = nom.Remain,
                    Price = nom.PriceWithNaklad != 0 ? nom.PriceWithNaklad : nom.Price,
                    State = RowStatus.NewRow,
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Parent = Document
                };
                Document.Rows.Add(newItem);
            }
        }

        public ICommand AddNewNomenklCommand
        {
            get { return new Command(AddNewNomenkl, _ => Document.Warehouse != null && !Document.IsClosed); }
        }

        private void AddNewNomenkl(object obj)
        {
            var noms = StandartDialogs.SelectNomenklsDialog();
            if (noms == null || noms.Count == 0) return;
            IsAllNomekl = true;
            if (Document.State != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
            var code = Document.Rows.Any() ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var nom in noms.Where(nom => Document.Rows.All(_ => _.Nomenkl.DocCode != nom.DocCode)))
            {
                var q = nomenklManager.GetNomenklQuantity(Document.Warehouse.DocCode, nom.DocCode,
                    DateTime.Today, DateTime.Today);
                var quan = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                var newItem = new InventorySheetRowViewModel(new TD_24())
                {
                    DocCode = Document.DocCode,
                    Code = code,
                    Nomenkl = MainReferences.GetNomenkl(nom.DocCode),
                    QuantityFact = quan,
                    QuantityCalc = quan,
                    State = RowStatus.NewRow,
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Parent = Document
                };
                Document.Rows.Add(newItem);
                UnitOfWork.Context.TD_24.Add(newItem.Entity);
                code++;
            }
        }

        public ICommand RemoveNomenklCommand
        {
            get { return new Command(RemoveNomenkl, _ => CurrentNomenkl != null && !Document.IsClosed); }
        }

        private void RemoveNomenkl(object obj)
        {
            if (CurrentNomenkl.State != RowStatus.NewRow)
            {
                Document.DeletedRows.Add(CurrentNomenkl);
                UnitOfWork.Context.TD_24.Remove(CurrentNomenkl.Entity);
            }

            Rows.Remove(CurrentNomenkl);
            UnitOfWork.Context.Entry(CurrentNomenkl.Entity).State = EntityState.Detached;
            Document.State = RowStatus.Edited;
            RaisePropertyChanged(nameof(IsRedoAllow));
        }

        #endregion
    }
}
