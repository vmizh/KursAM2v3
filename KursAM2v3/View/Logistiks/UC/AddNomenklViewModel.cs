using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.CodeView;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.References.RedisCache;
using KursDomain.Repository.NomenklRepository;

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

        private readonly NomenklRepository nomenklRepository = new NomenklRepository(GlobalOptions.GetEntities());

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
            foreach (var nom in nomenklRepository.GetByGroupDC(CurrentGroup.DocCode))
            {
                if(currentCrs is null)
                    NomenklItem.Add(nom);
                else
                {
                    if(((IDocCode)nom.Currency).DocCode == currentCrs.DocCode)
                        NomenklItem.Add(nom);
                }
            }
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
                NomenklGroup =
                    new ObservableCollection<NomenklGroup>(GlobalOptions.ReferencesCache.GetNomenklGroupAll()
                        .Cast<NomenklGroup>());
                if (!IsNotUsluga)
                {
                    myDataUserControl.treeListPermissionStruct.IsEnabled = false;
                    NomenklItemCollection.AddRange(currentCrs is not null
                        ? nomenklRepository.GetUsluga().Where(_ => Equals(_.Currency, currentCrs))
                        : nomenklRepository.GetUsluga());
                    NomenklItem.AddRange(NomenklItemCollection);
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
            while (((RedisCacheReferences)GlobalOptions.ReferencesCache).isNomenklCacheLoad)
                Thread.Sleep(new TimeSpan(0, 0, 5));
            foreach (var n in nomenklRepository.FindByName(SearchText))
                switch (IsNotUsluga)
                {
                    case true:
                        if(currentCrs is null)
                            NomenklItem.Add(n);
                        else 
                            if(((IDocCode)n.Currency).DocCode == currentCrs.DocCode)
                                NomenklItem.Add(n);
                        break;
                    default:
                    {
                        if (n.IsUsluga)
                            if (currentCrs is null)
                                NomenklItem.Add(n);
                            else if (((IDocCode)n.Currency).DocCode == currentCrs.DocCode)
                                NomenklItem.Add(n);
                        break;
                    }
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
