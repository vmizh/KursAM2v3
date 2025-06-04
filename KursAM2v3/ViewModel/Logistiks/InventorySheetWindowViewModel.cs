using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core.Helper;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Repositories;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;
using KursDomain.Repository.NomenklRepository;
using KursDomain.Repository.WarehouseRepository;
using KursDomain.ViewModel.Base2;
using KursDomain.ViewModel.Dialog;

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
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (myIsNomenklContains == value) return;
                myIsNomenklContains = value;
            }
        }
    }

    public sealed class InventorySheetWindowViewModel : RSWindowViewModelBase
    {
        #region Methods

        private void SetIsContainNomenkl(NomenklGroup2 grp)
        {
            if (grp == null) return;
            grp.IsNomenklContains = true;
            SetIsContainNomenkl(NomenklGroups.FirstOrDefault(_ => _.DocCode == grp.ParentDC));
        }

        #endregion

        #region Fields

        private readonly List<decimal> usedNomenklDCList = new List<decimal>();

        public readonly GenericKursDBRepository<SD_24> GenericInventorySheetRepository;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        // ReSharper disable once NotAccessedField.Local
        public readonly ISD_24Repository SD_24Repository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private InventorySheetRowViewModel myCurrentNomenkl;
        private NomenklGroup2 myCurrentNomenklGroup;

        private InventorySheetViewModel myDocument;

        //private bool myIsAllNomekl;
        private bool myIsGroupEnabled;

        private readonly INomenklRepository _NomenklRepository;

        #endregion

        #region Constructors

        public InventorySheetWindowViewModel()
        {
            GenericInventorySheetRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            _NomenklRepository = new NomenklRepository(UnitOfWork.Context);
            // ReSharper disable once VirtualMemberCallInContructor
            IsDocNewCopyRequisiteAllow = true;
            IsDocNewCopyAllow = false;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            RefreshData();
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

            RaisePropertyChanged(nameof(IsDateCanChanged));
        }

        #endregion

        #region Properties

        public ObservableCollection<InventorySheetRowViewModel> SelectedRows { set; get; } =
            new ObservableCollection<InventorySheetRowViewModel>();

        public override string WindowName => Document.State == RowStatus.NewRow
            ? "Инвентаризационная ведомость (новая)"
            : $"Инвентаризационная ведомость №{Document.Num} от {Document.Date.ToShortDateString()}" +
              $" на склад {Document.Warehouse}";

        public override string LayoutName => "InventorySheetWindowViewModel";

        public List<KursDomain.References.Warehouse> StoreCollection { set; get; } =
            new List<KursDomain.References.Warehouse>(GlobalOptions.ReferencesCache.GetWarehousesAll()
                .Cast<KursDomain.References.Warehouse>().OrderBy(_ => _.Name));


        public bool IsCannotChangeStore => Document.State == RowStatus.NewRow;

        public InventorySheetViewModel Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument, value)) return;
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

        public bool IsDateCanChanged
        {
            get => Document.State != RowStatus.NewRow;
            set { }
        }

        public ObservableCollection<InventorySheetRowViewModel> Rows { get; set; } =
            new ObservableCollection<InventorySheetRowViewModel>();


        public ObservableCollection<NomenklGroup2> NomenklGroups { set; get; } =
            new ObservableCollection<NomenklGroup2>();

        public InventorySheetRowViewModel CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                if (Equals(myCurrentNomenkl, value)) return;
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
            ((InventorySheetWindowViewModel)f.DataContext).Form = f;
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
            usedNomenklDCList.Clear();
            usedNomenklDCList.AddRange(Document.Rows.Select(_ => _.Nomenkl.DocCode));

            //LoadNomenklGroup();
            CurrentNomenklGroup = null;
            // ReSharper disable once PossibleNullReferenceException
            Document.myState = RowStatus.NotEdited;
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            // убираем лишние колонки из нижнего ряда суммирования (сумм)
            // Разница и цена

            if (Form is InventorySheetView2 prm)
            {
                var smmList = new List<GridSummaryItem>();
                foreach (var colummItem in prm.gridNomenklRows.TotalSummary)
                    if (colummItem.FieldName is nameof(InventorySheetRowViewModel.Difference)
                        or nameof(InventorySheetRowViewModel.Price))
                        smmList.Add(colummItem);

                foreach (var columnItem2 in smmList) prm.gridNomenklRows.TotalSummary.Remove(columnItem2);
                prm.DocDate.IsReadOnly = Document.State != RowStatus.NewRow;
                prm.DocDate.AllowDefaultButton = Document.State == RowStatus.NewRow;
            }
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
                    foreach (var r in Document.Rows) r.DocCode = Document.DocCode;

                    Document.Num = UnitOfWork.Context.SD_24.Any(_ =>
                        _.DD_TYPE_DC == MatearialDocumentType.InventorySheet
                        && _.DD_DATE.Year == DateTime.Today.Year)
                        ? UnitOfWork.Context.SD_24.Where(_ => _.DD_TYPE_DC == MatearialDocumentType.InventorySheet
                                                              && _.DD_DATE.Year == DateTime.Today.Year)
                            .Max(_ => _.DD_IN_NUM) + 1
                        : 1;
                }

                foreach (var t in Document.Rows)
                {
                    t.DocCode = Document.DocCode;
                    if (t.Entity.DDT_FACT_CRS_CENA is null) t.Entity.DDT_FACT_CRS_CENA = 0;
                    if (t.Entity.DDT_TAX_CRS_CENA is null) t.Entity.DDT_TAX_CRS_CENA = 0;
                    switch (t.Difference)
                    {
                        case 0:
                            t.Entity.DDT_KOL_PRIHOD = 0;
                            t.Entity.DDT_KOL_RASHOD = 0;
                            break;
                        case > 0:
                            t.Entity.DDT_KOL_PRIHOD = t.Difference;
                            t.Entity.DDT_KOL_RASHOD = 0;
                            break;
                        case < 0:
                            t.Entity.DDT_KOL_PRIHOD = 0;
                            t.Entity.DDT_KOL_RASHOD = -t.Difference;
                            break;
                    }
                }


                
                NomenklCalculationManager.InsertNomenklForCalc(UnitOfWork.Context,
                    Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList());
                NomenklCalculationManager.CalcAllNomenklRemains(UnitOfWork.Context);
                foreach (var n in usedNomenklDCList)
                {
                    var c = NomenklCalculationManager.GetNomenklStoreRemain(UnitOfWork.Context, Document.Date,
                        n, Document.Warehouse.DocCode);
                    var newQuan = Document.Rows.First(_ => _.Nomenkl.DocCode == n).Difference;
                    if (c + newQuan < 0)
                    {
                        //UnitOfWork.Rollback();
                        var nom = GlobalOptions.ReferencesCache.GetNomenkl(n) as Nomenkl;
                        WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                  // ReSharper disable once PossibleInvalidOperationException
                                                  $"склад {Document.Warehouse} в кол-ве {c} ",
                            "Отрицательные остатки", MessageBoxImage.Error);
                        return;
                    }
                }
                //UnitOfWork.CreateTransaction();
                UnitOfWork.Save();
                //UnitOfWork.Commit();
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InventoryList), Document.Id,
                    0, null, (string)Document.ToJson());
                foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                Document.myState = RowStatus.NotEdited;
                if (Form is InventorySheetView2 prm)
                {
                    prm.DocDate.IsReadOnly = true;
                    prm.DocDate.AllowDefaultButton = false;
                }

                Document.RaisePropertyChanged("State");
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
            var frm = new InventorySheetView2
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = new InventorySheetViewModel(SD_24Repository.CreateNew())
                },
                Owner = Application.Current.MainWindow
            };
            ((InventorySheetWindowViewModel)frm.DataContext).Document.State = RowStatus.NewRow;
            ((InventorySheetWindowViewModel)frm.DataContext).Document.Entity.DD_TYPE_DC =
                MatearialDocumentType.InventorySheet;
            ((InventorySheetWindowViewModel)frm.DataContext).Form = frm;

            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var doc = new InventorySheetViewModel(SD_24Repository.CreateRequisiteCopy(Document.DocCode))
            {
                State = RowStatus.NewRow
            };
            var frm = new InventorySheetView2
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = doc
                },
                Owner = Application.Current.MainWindow
            };
            ((InventorySheetWindowViewModel)frm.DataContext).Form = frm;
            frm.Show();
        }

        public override void DocNewCopy(object form)
        {
            var frm = new InventorySheetView2
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
            var maxCode = Document.Rows.Any() ? Document.Rows.Select(_ => _.Code).Max() + 1 : 1;
            var noms = NomenklCalculationManager
                .GetNomenklStoreRemains(Document.Date, Document.Warehouse.DocCode)
                .Where(_ => _.Remain != 0)
                .Where(nom => Document.Rows.All(_ => _.Nomenkl.DocCode != nom.NomenklDC));
            // ReSharper disable once PossibleMultipleEnumeration
            if (noms.Any() && Document.State != RowStatus.NewRow) Document.myState = RowStatus.Edited;
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var nom in noms)
            {
                var newItem = new InventorySheetRowViewModel(new TD_24())
                {
                    DocCode = Document.DocCode,
                    Code = maxCode,
                    Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(nom.NomenklDC) as Nomenkl,
                    QuantityFact = nom.Remain,
                    QuantityCalc = nom.Remain,
                    Price = nom.PriceWithNaklad != 0 ? nom.PriceWithNaklad : nom.Price,
                    State = RowStatus.NewRow,
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Parent = Document
                };
                Document.Rows.Add(newItem);
                UnitOfWork.Context.TD_24.Add(newItem.Entity);
                if (!usedNomenklDCList.Contains(newItem.Nomenkl.DocCode))
                    usedNomenklDCList.Add(newItem.Nomenkl.DocCode);

                maxCode++;
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
            //IsAllNomekl = true;
            if (Document.State != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
            var code = Document.Rows.Any() ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var nom in noms.Where(nom => Document.Rows.All(_ => _.Nomenkl.DocCode != nom.DocCode)))
            {
                var q = nomenklManager.GetNomenklQuantity(Document.Warehouse.DocCode, nom.DocCode,
                    new DateTime(2000, 1, 1), Document.Date.AddDays(-1));
                var quan = q.Count == 0 ? 0 : q.Last().OstatokQuantity;
                var newItem = new InventorySheetRowViewModel(new TD_24())
                {
                    DocCode = Document.DocCode,
                    Code = code,
                    Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(nom.DocCode) as Nomenkl,
                    QuantityFact = quan,
                    QuantityCalc = quan,
                    State = RowStatus.NewRow,
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Parent = Document
                };
                Document.Rows.Add(newItem);
                if (!usedNomenklDCList.Contains(newItem.Nomenkl.DocCode))
                    usedNomenklDCList.Add(newItem.Nomenkl.DocCode);
                UnitOfWork.Context.TD_24.Add(newItem.Entity);
                code++;
            }
        }

        public ICommand RecalcNomenklCommand
        {
            get { return new Command(RecalcNomenkl, _ => CurrentNomenkl != null); }
        }

        private void RecalcNomenkl(object obj)
        {
            foreach (var row in SelectedRows)
            {
                row.QuantityCalc = NomenklCalculationManager.GetNomenklStoreRemain(UnitOfWork.Context, Document.Date,
                    row.Nomenkl.DocCode, Document.WarehouseIn.DocCode);
            }
        }

        public ICommand AddWarehouseNomenklCommand
        {
            get { return new Command(AddWarehouseNomenkl, _ => Document.Warehouse != null && !Document.IsClosed); }
        }

        private void AddWarehouseNomenkl(object obj)
        {
            var code = Document.Rows.Any() ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            var vm = new WarehouseRemainsViewModel(Document.Date, Document.Warehouse,
                new WarehouseRepository(GlobalOptions.GetEntities()), null,
                Document.Rows.Select(_ => _.Nomenkl.DocCode).ToList());
            vm.Show();
            switch (vm.Result)
            {
                case KursDialogResult.Ok:
                    foreach (var item in vm.SelectDocumentItems)
                    {
                        var newItem = new InventorySheetRowViewModel(new TD_24())
                        {
                            DocCode = Document.DocCode,
                            Code = code,
                            Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.Nomenkl.DocCode) as Nomenkl,
                            QuantityFact = item.Remain,
                            QuantityCalc = item.Remain,
                            State = RowStatus.NewRow,
                            // Price = item.PriceWithNaklad != 0 ? item.PriceWithNaklad : item.Price,
                            Price = item.Price,
                            Id = Guid.NewGuid(),
                            DocId = Document.Id,
                            Parent = Document
                        };
                        Document.State = RowStatus.Edited;
                        Document.Rows.Add(newItem);
                        if (!usedNomenklDCList.Contains(newItem.Nomenkl.DocCode))
                            usedNomenklDCList.Add(newItem.Nomenkl.DocCode);
                        UnitOfWork.Context.TD_24.Add(newItem.Entity);
                        code++;
                    }

                    break;
                case KursDialogResult.Cancel:
                    break;
            }
        }

        public ICommand RemoveNomenklCommand
        {
            get { return new Command(RemoveNomenkl, _ => CurrentNomenkl != null && !Document.IsClosed); }
        }

        private void RemoveNomenkl(object obj)
        {
            foreach (var nom in SelectedRows.Select(_ => _.Nomenkl).ToList())
            {
                var row = Document.Rows.FirstOrDefault(_ => _.Nomenkl == nom);
                if (row == null) continue;
                if (usedNomenklDCList.Contains(row.Nomenkl.DocCode))
                    usedNomenklDCList.Remove(row.Nomenkl.DocCode);
                UnitOfWork.Context.TD_24.Remove(row.Entity);
                if (CurrentNomenkl.State != RowStatus.NewRow) Document.DeletedRows.Add(row);
                Document.Rows.Remove(row);
            }

            //UnitOfWork.Context.Entry(CurrentNomenkl.Entity).State = EntityState.Detached;
            Document.State = RowStatus.Edited;
            RaisePropertyChanged(nameof(IsRedoAllow));
        }

        #endregion
    }
}
