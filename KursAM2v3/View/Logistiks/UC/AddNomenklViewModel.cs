using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.CodeView;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

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

        public override string LayoutName => "DialogSelectNomenklLayout2";

        public NomenklGroup CurrentGroup
        {
            set
            {
                if (Equals(myCurrentGroup, value)) return;
                myCurrentGroup = value;
                GetNomenklItem();
            }
            get => myCurrentGroup;
        }

        public bool IsNotUsluga
        {
            set
            {
                if (myIsNotUsluga == value) return;
                myIsNotUsluga = value;
            }
            get => myIsNotUsluga;
        }

        public AddNomenklUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
            }
            get => myDataUserControl;
        }

        public Nomenkl CurrentNomenkl
        {
            set
            {
                if (Equals(myCurrentNomenkl, value)) return;
                myCurrentNomenkl = value;
            }
            get => myCurrentNomenkl;
        }

        public Nomenkl CurrentSelectNomenkl
        {
            set
            {
                if (Equals(myCurrentSelectNomenkl, value)) return;
                myCurrentSelectNomenkl = value;
            }
            get => myCurrentSelectNomenkl;
        }

        public void GetNomenklItem()
        {
            NomenklItem.Clear();
            foreach (var item in NomenklItemCollection)
                if (((IDocCode)item.Group).DocCode == CurrentGroup.DocCode)
                    NomenklItem.Add(item);
        }

        #region command

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            NomenklItemCollection.Clear();
            SelectedNomenkl.Clear();
            NomenklGroup.Clear();
            RaisePropertyChanged(nameof(IsNotUsluga));
            try
            {
                //using (var ctx = GlobalOptions.GetEntities())
                //{
                NomenklGroup =
                    new ObservableCollection<NomenklGroup>(GlobalOptions.ReferencesCache.GetNomenklGroupAll()
                        .Cast<NomenklGroup>());

                //foreach (var item in ctx.SD_82.ToList())
                //    NomenklGroup.Add(new NomenklGroup(item));
                if (IsNotUsluga)
                {
                    NomenklItemCollection.AddRange(currentCrs == null
                        ? GlobalOptions.ReferencesCache.GetNomenklsAll().Cast<Nomenkl>().ToList()
                        : GlobalOptions.ReferencesCache.GetNomenklsAll()
                            .Where(_ => ((IDocCode)_.Currency).DocCode == currentCrs.DocCode).Cast<Nomenkl>().ToList());
                }
                else
                {
                    myDataUserControl.treeListPermissionStruct.IsEnabled = false;
                    NomenklItemCollection.AddRange(GlobalOptions.ReferencesCache.GetNomenklsAll()
                        .Where(_ => _.IsUsluga).Cast<Nomenkl>());
                    NomenklItem.AddRange(NomenklItemCollection);
                }

                //foreach (var item in MainReferences.ALLNomenkls.Values)
                //    if (IsNotUsluga)
                //    {
                //        if (currentCrs == null)
                //            NomenklItemCollection.Add(item);
                //        else if (item.NOM_SALE_CRS_DC == currentCrs.DocCode)
                //            NomenklItemCollection.Add(item);
                //    }
                //    else
                //    {
                //        if (item.NOM_0MATER_1USLUGA == 1)
                //            NomenklItem.Add(item);
                //    }
                //}

                CalcCommonSumAsync();
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        private async Task CalcCommonSumAsync()
        {
            await Task.Run(() => CalcCommonSum());
        }

        private void CalcCommonSum()
        {
            var parents = NomenklGroup.Select(_ => _.ParentDC).Distinct().ToList();
            var lasts = new List<NomenklGroup>();
            foreach (var n in NomenklGroup)
                if (parents.All(_ => _ != n.DocCode))
                    lasts.Add(n);
            foreach (var node in lasts)
            {
                node.NomenklCount = GlobalOptions.ReferencesCache.GetNomenklsAll().Cast<Nomenkl>()
                    .Count(_ => ((IDocCode)_.Group).DocCode == node.DocCode);
                var prevn = node;
                var n = NomenklGroup.FirstOrDefault(_ => _.DocCode == node.ParentDC);
                if (n == null) continue;
                while (n != null)
                {
                    var c = GlobalOptions.ReferencesCache.GetNomenklsAll().Cast<Nomenkl>()
                        .Count(_ => ((IDocCode)_.Group).DocCode == n.DocCode);
                    n.NomenklCount = n.NomenklCount + prevn.NomenklCount + c;
                    prevn = n;
                    n = NomenklGroup.FirstOrDefault(_ => _.DocCode == n.ParentDC);
                }
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
            foreach (var n in GlobalOptions.ReferencesCache.GetNomenklsAll().Cast<Nomenkl>().ToList())
            {
                var srch = SearchText.ToUpper();
                if (n.Name.ToUpper().Contains(srch) || (n.FullName?.ToUpper().Contains(srch) ?? false)
                                                    || n.NomenklNumber.ToUpper().Contains(srch)
                                                    || (n.Notes?.ToUpper().Contains(srch) ?? false))
                {
                    if (currentCrs != null)
                    {
                        if (((IDocCode)n.Currency).DocCode == currentCrs.DocCode)
                            NomenklItem.Add(n);
                    }
                    else
                    {
                        NomenklItem.Add(n);
                    }
                }

                if (NomenklItem.Count > 0) myDataUserControl.treeListPermissionStruct.IsEnabled = false;
            }
        }

        public ICommand ClearSearchCommand
        {
            get { return new Command(ClearSearch, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public void ClearSearch(object obj)
        {
            NomenklItem.Clear();
            myDataUserControl.treeListPermissionStruct.IsEnabled = true;
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
