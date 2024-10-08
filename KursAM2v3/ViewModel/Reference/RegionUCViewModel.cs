﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursAM2.View.DialogUserControl;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference
{
    public class RegionUCViewModel : RSWindowViewModelBase
    {
        private Region myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public RegionUCViewModel()
        {
            WindowName = "Справочник регионов";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RefreshData(null);
        }

        #region command

        public override void RefreshData(object o)
        {
            ItemsCollection = new ObservableCollection<Region>(GlobalOptions.ReferencesCache.GetRegionsAll()
                .Cast<Region>().OrderBy(_ => _.Name));
            RaisePropertyChanged(nameof(ItemsCollection));
        }

        //<MenuItem Header = "Добавить" Command="{Binding AddNewItemCommand}" />
        //<MenuItem Header = "Удалить" Command="{Binding RemoveItemCommand}" />
        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => true); }
        }

        private void AddNewItem(object obj)
        {
            var newRow = new Region
            {
                DocCode = -1
            };
            ItemsCollection.Add(newRow);
            CurrentItem = newRow;
        }

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem, _ => true); }
        }

        private void RemoveItem(object obj)
        {
            var dList = new List<Region>();
            foreach (var d in SelectedItems)
                dList.Add(d);
            foreach (var r in dList) ItemsCollection.Remove(r);
            var drows = new ObservableCollection<Region>(SelectedItems);
            foreach (var d in drows)
            {
                ItemsCollection.Remove(d);
                ItemsDeleteCollection.Add(d);
            }
        }

        #endregion

        #region properties

        public ObservableCollection<Region> ItemsCollection { set; get; } =
            new ObservableCollection<Region>();

        public ObservableCollection<Region> SelectedItems { set; get; } =
            new ObservableCollection<Region>();

        public ObservableCollection<Region> ItemsDeleteCollection { set; get; } =
            new ObservableCollection<Region>();

        public StandartDialogSelectUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
            get => myDataUserControl;
        }

        public Region CurrentItem
        {
            set
            {
                if (Equals(myCurrentItem, value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
            get => myCurrentItem;
        }

        #endregion
    }
}
