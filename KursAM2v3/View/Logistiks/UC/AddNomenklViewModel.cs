using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;

namespace KursAM2.View.Logistiks.UC
{
    public class AddNomenklViewModel : RSWindowViewModelBase
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private Currency currentCrs;
        private NomenklGroup myCurrentGroup;
        private Nomenkl myCurrentNomenkl;
        private Nomenkl myCurrentSelectNomenkl;
        private AddNomenklUC myDataUserControl;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private bool myIsNotUsluga;

        public AddNomenklViewModel(Currency crs = null, bool isNotUsluga = false)
        {
            currentCrs = crs;
            IsNotUsluga = isNotUsluga;
            myDataUserControl = new AddNomenklUC();
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ObservableCollection<NomenklGroup> NomenklGroup { set; get; } =
            new ObservableCollection<NomenklGroup>();

        public ObservableCollection<Nomenkl> NomenklItem { set; get; } =
            new ObservableCollection<Nomenkl>();

        public ObservableCollection<Nomenkl> NomenklItemCollection { set; get; } =
            new ObservableCollection<Nomenkl>();

        public ObservableCollection<Nomenkl> SelectedNomenkl { set; get; } =
            new ObservableCollection<Nomenkl>();

        public List<Nomenkl> ListNomenklCollection { set; get; } =
            new List<Nomenkl>();

        public NomenklGroup CurrentGroup
        {
            set
            {
                if (myCurrentGroup != null && myCurrentGroup.Equals(value)) return;
                myCurrentGroup = value;
                GetNomenklItem();
                RaisePropertiesChanged();
            }
            get => myCurrentGroup;
        }

        public bool IsNotUsluga
        {
            set
            {
                if (myIsNotUsluga == value) return;
                myIsNotUsluga = value;
                RaisePropertiesChanged();
            }
            get => myIsNotUsluga;
        }

        public AddNomenklUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertiesChanged();
            }
            get => myDataUserControl;
        }

        public Nomenkl CurrentNomenkl
        {
            set
            {
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
                myCurrentNomenkl = value;
                RaisePropertiesChanged();
            }
            get => myCurrentNomenkl;
        }

        public Nomenkl CurrentSelectNomenkl
        {
            set
            {
                if (myCurrentSelectNomenkl != null && myCurrentSelectNomenkl.Equals(value)) return;
                myCurrentSelectNomenkl = value;
                RaisePropertiesChanged();
            }
            get => myCurrentSelectNomenkl;
        }

        public void GetNomenklItem()
        {
            NomenklItem.Clear();
            foreach (var item in NomenklItemCollection)
                if (item.NOM_CATEG_DC == CurrentGroup.DocCode)
                    NomenklItem.Add(item);
        }

        #region command

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            NomenklItemCollection.Clear();
            SelectedNomenkl.Clear();
            NomenklGroup.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var item in ctx.SD_82.ToList())
                        NomenklGroup.Add(new NomenklGroup(item));
                    foreach (var item in ctx.SD_83.ToList())
                        if (IsNotUsluga)
                        {
                            if (currentCrs == null)
                                NomenklItemCollection.Add(new Nomenkl(item));
                            else if (item.NOM_SALE_CRS_DC == currentCrs.DocCode)
                                NomenklItemCollection.Add(new Nomenkl(item));
                        }
                        else
                        {
                            if (item.NOM_0MATER_1USLUGA == 1)
                                NomenklItem.Add(new Nomenkl(item));
                            RaisePropertiesChanged(nameof(NomenklItem));
                        }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand AddNomenklToSelectCommand
        {
            get { return new Command(AddNomenklToSelect, _ => true); }
        }

        private void AddNomenklToSelect(object obj)
        {
            if (!SelectedNomenkl.Contains(CurrentNomenkl))
                SelectedNomenkl.Add(CurrentNomenkl);
            if (!ListNomenklCollection.Contains(CurrentNomenkl))
                ListNomenklCollection.Add(CurrentNomenkl);
        }

        public ICommand DeletedNomenklInSelecktCommand
        {
            get { return new Command(DeletedNomenklInSeleckt, _ => true); }
        }

        private void DeletedNomenklInSeleckt(object obj)
        {
            if (CurrentSelectNomenkl != null && SelectedNomenkl.Count > 0)
                SelectedNomenkl.Remove(CurrentSelectNomenkl);
        }

        public override bool IsCanSearch => SearchText != null;

        public override void Search(object obj)
        {
            SearchNomenkl(null);
        }

        public void SearchNomenkl(object obj)
        {
            NomenklItem.Clear();
            foreach (var n in MainReferences.ALLNomenkls.Values)
            {
                var srch = SearchText.ToUpper();
                if (n.NOM_NAME.ToUpper().Contains(srch) || (n.NOM_FULL_NAME?.ToUpper().Contains(srch) ?? false)
                                                        || (n.NOM_POLNOE_IMIA?.ToUpper().Contains(srch) ?? false)
                                                        || n.NOM_NOMENKL.ToUpper().Contains(srch)
                                                        || (n.NOM_NOTES?.ToUpper().Contains(srch) ?? false))
                {
                    if (currentCrs != null)
                    {
                        if (n.Currency.DocCode == currentCrs.DocCode)
                            NomenklItem.Add(n);
                    }
                    else
                    {
                        NomenklItem.Add(n);
                    }
                }

                if (NomenklItem.Count > 0) myDataUserControl.treeListPermissionStruct.IsEnabled = false;
            }

            //foreach (var item in NomenklItemCollection)
            //    // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            //    // ReSharper disable once MergeConditionalExpression
            //    if (item.NOM_POLNOE_IMIA != null
            //        ? item.NOM_POLNOE_IMIA.ToUpper().Contains(SearchText.ToUpper())
            //        : false
            //          || item.NOM_NOMENKL.ToUpper().Contains(SearchText.ToUpper()))
            //        NomenklItem.Add(item);
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            myDataUserControl.treeListPermissionStruct.IsEnabled = true;
            NomenklItem.Clear();
            RefreshData(null);
        }

        #endregion
    }
}