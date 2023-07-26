using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursDomain.ICommon;

namespace KursAM2.View.DialogUserControl.Standart
{

    public class NomenklWithQuantityWindowView<T> : TableSearchWindowViewMove<T>
    {
        public NomenklWithQuantityWindowView(GetData loadDatamethod, string winName, string layoutName) : base(loadDatamethod, winName, layoutName)
        {
            
        }
    }

    public class TableSearchWindowViewMove<T> : RSWindowViewModelBase
    {
        public delegate IEnumerable<T> GetData(string searchText);

        #region Constructors

        public TableSearchWindowViewMove(GetData loadDatamethod, string winName, string layoutName)
        {
            LayoutName = layoutName;
            WindowName = winName;
            loadMethod = loadDatamethod;
            CustomDataUserControl = new TableWithSearch();
        }

        #endregion

        #region Fields

        private T myCurrentItem;
        private readonly GetData loadMethod;

        #endregion

        #region Properties

        public TableWithSearch CustomDataUserControl { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local

        public ObservableCollection<T> ItemsCollection { set; get; } = new ObservableCollection<T>();

        // ReSharper disable CollectionNeverUpdated.Global
        public ObservableCollection<T> SelectedItems { set; get; } = new ObservableCollection<T>();
        // ReSharper restore CollectionNeverUpdated.Global

        public T CurrentItem
        {
            set
            {
                if (((IDocCode)myCurrentItem)?.DocCode == ((IDocCode)value)?.DocCode) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
            get => myCurrentItem;
        }

        #endregion

        #region Commands

        //public override void RefreshData(object obj)
        //{
        //    ItemsCollection.Clear();
        //    ItemsCollection = new ObservableCollection<T>(loadMethod(SearchText));
        //}

        public override void Ok(object obj)
        {
            DialogResult = true;
            Form?.Close();
        }

        public override void Cancel(object obj)
        {
            SelectedItems.Clear();
            DialogResult = false;
            Form?.Close();
        }

        public ICommand SearchTextChangingCommand
        {
            get { return new Command(SearchTextChanging, _ => SearchText?.Length >= 3); }
        }

        private void SearchTextChanging(object obj)
        {
            ItemsCollection.Clear();
            foreach (var n in loadMethod(SearchText)) ItemsCollection.Add(n);
        }

        #endregion

        #region Methods

        #endregion
    }
}
