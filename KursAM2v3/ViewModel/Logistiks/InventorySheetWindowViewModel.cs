using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Logistiks;

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
                RaisePropertyChanged();
            }
        }
    }

    public sealed class InventorySheetWindowViewModel : RSWindowViewModelBase
    {
        private readonly InventorySheetManager myManager = new InventorySheetManager();
        private readonly NomenklManager myNomenklManager = new NomenklManager();
        private InventorySheetRowViewModel myCurrentNomenkl;
        private NomenklGroup2 myCurrentNomenklGroup;
        private InventorySheetViewModel myDocument;
        private bool myIsAllNomekl;
        private bool myIsGroupEnabled;
        private ObservableCollection<InventorySheetRowViewModel> myRows;

        public InventorySheetWindowViewModel()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            //LoadReference();
            LoadNomenklGroup();
            IsAllNomekl = true;
        }

        public InventorySheetWindowViewModel(decimal docCode) : this()
        {
            Document = new InventorySheetViewModel
            {
                DocCode = docCode
            };
            RefreshData(null);
        }

        public bool IsCannotChangeStore
        {
            get => Document.State == RowStatus.NewRow;
            // ReSharper disable once ValueParameterNotUsed
            private set => RaisePropertiesChanged();
        }

        public InventorySheetViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
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
                foreach (var n in from dc in listDC from n in Document.Rows where n.Nomenkl.Group.DocCode == dc select n
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

        public ObservableCollection<NomenklGroup2> NomenklGroups { set; get; } =
            new ObservableCollection<NomenklGroup2>();

        public InventorySheetRowViewModel CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
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

        public override bool IsRedoAllow => Document != null && Document.DeletedRows.Count > 0;
        public override bool IsCanSaveData => Document != null && Document.State != RowStatus.NotEdited;
        public List<Core.EntityViewModel.NomenklManagement.Warehouse> StoreCollection => myManager.StoreCollection;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsDocDeleteAllow => true;

        private List<decimal> GetNomenklGroupChilds(NomenklGroup2 grp)
        {
            var ret = new List<decimal>();
            ret.AddRange(
                Document.Rows.Where(_ => _.Nomenkl.Group.DocCode == grp.DocCode).Select(_ => _.Nomenkl.Group.DocCode));
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
            RaisePropertiesChanged(nameof(NomenklGroups));
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
                Document.Rows.Select(_ => _.Nomenkl.NOM_CATEG_DC)
                    .Distinct()
                    .ToList()
                    .Select(ngDC => NomenklGroups.FirstOrDefault(_ => _.DocCode == ngDC))
                    .Where(ng => ng != null))
                SetIsContainNomenkl(ng);
            RaisePropertiesChanged(nameof(NomenklGroups));
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            RaisePropertiesChanged(nameof(StoreCollection));
            var dc = Document.DocCode;
            Document = myManager.Load(dc);
            if (Document == null) return;
            LoadNomenklGroup();
            //if (Document.WarehouseIn != null)
            //{
            //    Document.WarehouseIn = myManager.StoreCollection.SingleOrDefault(_ => _.DocCode == Document.WarehouseIn.DocCode);
            //}
            Document.State = RowStatus.NotEdited;
            RaisePropertiesChanged(nameof(NomenklGroups));
            CurrentNomenklGroup = null;
            IsCannotChangeStore = true;
            RaisePropertiesChanged(nameof(Document));
        }

        public override void SaveData(object data)
        {
            myManager.Save(Document);
            RefreshData(null);
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

                    myManager.Delete(Document.DocCode);
                    f?.Close();
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
                    Document = myManager.CreateNew()
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
                    Document = myManager.CreateNew(Document, DocumentCopyType.Requisite)
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        #region Commands

        public ICommand AddAllNomenklCommand
        {
            get { return new Command(AddAllNomenkl, _ => Document.Warehouse != null && !Document.IsClosed); }
        }

        private void AddAllNomenkl(object obj)
        {
            foreach (
                var nom in NomenklCalculationManager.GetNomenklStoreRemains(Document.Date, Document.Warehouse.DocCode)
                    .Where(_ => _.Remain != 0)
                    .Where(nom => Document.Rows.All(_ => _.Nomenkl.DocCode != nom.NomenklDC)))
                Document.Rows.Add(new InventorySheetRowViewModel
                {
                    DocCode = Document.DocCode,
                    Nomenkl = MainReferences.GetNomenkl(nom.NomenklDC),
                    QuantityFact = nom.Remain,
                    QuantityCalc = nom.Remain,
                    Price = nom.PriceWithNaklad != 0 ? nom.PriceWithNaklad : nom.Price,
                    State = RowStatus.NewRow,
                    Id = Guid.NewGuid()
                });
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
            foreach (var nom in noms.Where(nom => Document.Rows.All(_ => _.Nomenkl.DocCode != nom.DocCode)))
                Document.Rows.Add(new InventorySheetRowViewModel
                {
                    DocCode = Document.DocCode,
                    Nomenkl = MainReferences.GetNomenkl(nom.DocCode),
                    QuantityFact =
                        NomenklManager.GetNomenklCount(Document.Date, nom.DocCode, Document.Warehouse.DocCode),
                    QuantityCalc =
                        NomenklManager.GetNomenklCount(Document.Date, nom.DocCode, Document.Warehouse.DocCode),
                    State = RowStatus.NewRow,
                    Id = Guid.NewGuid()
                });
        }

        public ICommand RemoveNomenklCommand
        {
            get { return new Command(RemoveNomenkl, _ => CurrentNomenkl != null && !Document.IsClosed); }
        }

        private void RemoveNomenkl(object obj)
        {
            if (CurrentNomenkl.State != RowStatus.NewRow)
                Document.DeletedRows.Add(CurrentNomenkl);
            Rows.Remove(CurrentNomenkl);
            RaisePropertiesChanged(nameof(IsRedoAllow));
        }

        #endregion
    }
}