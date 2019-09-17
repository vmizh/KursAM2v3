using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Logistiks.UC;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklSelectedDialogViewModel : RSWindowViewModelBase
    {
        private Nomenkl myCurrentNomenkl;
        private NomenklGroup myCurrentNomenklGroup;
        private Nomenkl myCurrentSelectedNomenkl;
        private NomenklGroup myCurretNomenklGroup;
        private SelectNomenkls myDataUserControl;
        private bool myIsGroupEnable;

        public NomenklSelectedDialogViewModel()
        {
            myDataUserControl = new SelectNomenkls();
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            LoadNomenklGroups();
            IsGroupEnable = true;
        }

        public ObservableCollection<Nomenkl> NomenklCollection { set; get; } = new ObservableCollection<Nomenkl>();
        public ObservableCollection<Nomenkl> SelectedToAddNomenkls { set; get; } = new ObservableCollection<Nomenkl>();
        public ObservableCollection<Nomenkl> SelectedNomenkls { set; get; } = new ObservableCollection<Nomenkl>();

        public ObservableCollection<Nomenkl> SelectedToRemoveNomenkls { set; get; } =
            new ObservableCollection<Nomenkl>();

        public ObservableCollection<NomenklGroup> NomenklGroups { set; get; } =
            new ObservableCollection<NomenklGroup>();

        public NomenklGroup CurrentNomenklGroup
        {
            get => myCurrentNomenklGroup;
            set
            {
                if (myCurrentNomenklGroup != null && myCurrentNomenklGroup.Equals(value)) return;
                myCurrentNomenklGroup = value;
                LoadNomenklForGroup();
                RaisePropertyChanged();
            }
        }

        public NomenklGroup CurretNomenklGroup
        {
            get => myCurretNomenklGroup;
            set
            {
                if (myCurretNomenklGroup != null && myCurretNomenklGroup.Equals(value)) return;
                myCurretNomenklGroup = value;
                RaisePropertyChanged();
            }
        }

        public bool IsGroupEnable
        {
            get => myIsGroupEnable;
            set
            {
                if (Equals(myIsGroupEnable, value)) return;
                myIsGroupEnable = value;
                RaisePropertyChanged();
            }
        }

        public SelectNomenkls DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                if (!string.IsNullOrWhiteSpace(mySearchText))
                {
                    CurrentNomenklGroup = null;
                    IsGroupEnable = false;
                }
                else
                {
                    IsGroupEnable = true;
                }

                RaisePropertyChanged();
            }
        }

        public Nomenkl CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                SelectedToRemoveNomenkls.Clear();
                RaisePropertiesChanged(nameof(SelectedToRemoveNomenkls));
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
                myCurrentNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public Nomenkl CurrentSelectedNomenkl
        {
            get => myCurrentSelectedNomenkl;
            set
            {
                SelectedToAddNomenkls.Clear();
                RaisePropertiesChanged(nameof(SelectedToAddNomenkls));
                if (myCurrentSelectedNomenkl != null && myCurrentSelectedNomenkl.Equals(value)) return;
                myCurrentSelectedNomenkl = value;
                RaisePropertyChanged();
            }
        }

        private void LoadNomenklForGroup()
        {
            NomenklCollection.Clear();
            RaisePropertiesChanged(nameof(NomenklCollection));
            if (CurrentNomenklGroup == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var n in ctx.SD_83.Where(_ => _.NOM_CATEG_DC == CurrentNomenklGroup.DocCode))
                    NomenklCollection.Add(new Nomenkl
                    {
                        DocCode = n.DOC_CODE,
                        Name = n.NOM_NAME,
                        NameFull = n.NOM_FULL_NAME,
                        Note = n.NOM_NOTES,
                        NomenklNumber = n.NOM_NOMENKL,
                        Currency = MainReferences.Currencies[n.NOM_SALE_CRS_DC.Value]
                    });
            }
        }

        private void LoadNomenklGroups()
        {
            NomenklGroups.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var cat in ctx.SD_82.ToList())
                    NomenklGroups.Add(new NomenklGroup
                    {
                        DocCode = cat.DOC_CODE,
                        ParentDC = cat.CAT_PARENT_DC,
                        Name = cat.CAT_NAME
                    });
            }

            RaisePropertiesChanged(nameof(NomenklGroups));
        }

        #region Commands

        public ICommand ClearSelectedNomenklCommand
        {
            get { return new Command(ClearSelectedNomenkl, _ => SelectedToAddNomenkls?.Count > 0); }
        }

        private void ClearSelectedNomenkl(object obj)
        {
            SelectedToAddNomenkls.Clear();
            RaisePropertiesChanged(nameof(SelectedToAddNomenkls));
        }

        public ICommand SearchExecuteCommand
        {
            get { return new Command(SearchExecute, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public void SearchExecute(object obj)
        {
            NomenklCollection.Clear();
            foreach (var n in NomenklManager.GetNomenklsSearch(SearchText))
                NomenklCollection.Add(n);
            RaisePropertiesChanged(nameof(NomenklCollection));
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            NomenklCollection.Clear();
            CurrentNomenkl = null;
            SelectedToAddNomenkls.Clear();
            RaisePropertiesChanged(nameof(NomenklCollection));
            RaisePropertiesChanged(nameof(SelectedToAddNomenkls));
        }

        public ICommand AddToSelectedCommand
        {
            get { return new Command(AddToSelected, _ => true); }
        }

        private void AddToSelected(object obj)
        {
            foreach (var n in SelectedToAddNomenkls)
            {
                var isExist = false;
                // ReSharper disable once UnusedVariable
                foreach (var n2 in SelectedNomenkls.Where(n2 => n.DocCode == n2.DocCode))
                    isExist = true;
                if (!isExist)
                    SelectedNomenkls.Add(n);
            }

            RaisePropertiesChanged(nameof(SelectedNomenkls));
        }

        public ICommand RemoveAllSelectedCommand
        {
            get { return new Command(RemoveAllSelected, _ => SelectedNomenkls.Count > 0); }
        }

        private void RemoveAllSelected(object obj)
        {
            SelectedNomenkls.Clear();
            RaisePropertiesChanged(nameof(SelectedNomenkls));
        }

        public ICommand RemoveFromSelectedCommand
        {
            get
            {
                return new Command(RemoveFromSelected,
                    _ => SelectedToRemoveNomenkls != null && SelectedToRemoveNomenkls.Count > 0);
            }
        }

        private void RemoveFromSelected(object obj)
        {
            var dcs = SelectedToRemoveNomenkls.Select(n2 => n2.DocCode).ToList();
            foreach (
                var n in dcs.Select(dc => SelectedNomenkls.FirstOrDefault(_ => _.DocCode == dc)).Where(n => n != null))
                SelectedNomenkls.Remove(n);
            RaisePropertiesChanged(nameof(SelectedNomenkls));
        }

        public ICommand AddCurrentCommand
        {
            get { return new Command(AddCurrent, _ => true); }
        }

        private void AddCurrent(object obj)
        {
            if (SelectedNomenkls.Any(n => n.DocCode == CurrentNomenkl.DocCode))
                return;
            SelectedNomenkls.Add(CurrentNomenkl);
            RaisePropertiesChanged(nameof(SelectedNomenkls));
        }

        public ICommand RemoveCurrentCommand
        {
            get { return new Command(RemoveCurrent, _ => true); }
        }

        private void RemoveCurrent(object obj)
        {
            SelectedNomenkls.Remove(CurrentSelectedNomenkl);
            RaisePropertiesChanged(nameof(SelectedNomenkls));
        }

        #endregion Commands
    }
}