using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference
{
    public class CountriesRefWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private StandartErrorManager errorManager;

        #endregion

        #region Constructors

        public CountriesRefWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<CountriesViewModel> CountryCollection { set; get; } =
            new ObservableCollection<CountriesViewModel>();

        public ObservableCollection<CountriesViewModel> CountryDeleteCollection { set; get; } =
            new ObservableCollection<CountriesViewModel>();

        public ObservableCollection<CountriesViewModel> SelectedCountries { set; get; } =
            new ObservableCollection<CountriesViewModel>();

        private CountriesViewModel myCurrentCountry;

        public CountriesViewModel CurrentCountry
        {
            get => myCurrentCountry;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentCountry == value) return;
                myCurrentCountry = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsCanSaveData => CountryCollection.Any(_ => _.State != RowStatus.NotEdited)
                                              || CountryDeleteCollection.Count > 0;

        public override void RefreshData(object obj)
        {
            var WinManager = new WindowManager();
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    try
                    {
                        SaveData(null);
                    }
                    catch (Exception ex)
                    {
                        WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                    }
            }

            CountryCollection.Clear();
            CountryDeleteCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.Countries.ToList())
                    CountryCollection.Add(new CountriesViewModel(item) {myState = RowStatus.NotEdited});
            }
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                errorManager = new StandartErrorManager(ctx, "CountryRefWindow", true);
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        foreach (var d in CountryDeleteCollection)
                        {
                            var old = ctx.Countries.FirstOrDefault(_ => _.Id == d.Id);
                            if (old != null) ctx.Countries.Remove(old);
                        }

                        foreach (var r in CountryCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (r.State)
                            {
                                case RowStatus.Edited:
                                    var ed = ctx.Countries.FirstOrDefault(_ => _.Id == r.Id);
                                    if (ed == null) continue;
                                    ed.ForeignName = r.ForeignName;
                                    ed.Name = r.Name;
                                    ed.Iso = r.Iso;
                                    ed.Note = r.Note;
                                    ed.Sign2 = r.Sign2;
                                    ed.Sign3 = r.Sign3;
                                    break;
                                case RowStatus.NewRow:
                                    ctx.Countries.Add(new Countries
                                        {
                                            Id = r.Id,
                                            ForeignName = r.ForeignName,
                                            Name = r.Name,
                                            Iso = r.Iso,
                                            Note = r.Note,
                                            Sign2 = r.Sign2,
                                            Sign3 = r.Sign3
                                        }
                                    );
                                    break;
                            }

                        ctx.SaveChanges();
                        tnx.Complete();
                        foreach (var r in CountryCollection)
                            r.myState = RowStatus.NotEdited;
                        CountryDeleteCollection.Clear();
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(null, ex);
                        errorManager.WriteErrorMessage(ex);
                        return;
                    }
                }
            }

            RefreshData(null);
        }

        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => true); }
        }

        private void AddNewItem(object obj)
        {
            var newRow = new CountriesViewModel
            {
                State = RowStatus.NewRow,
                Id = Guid.NewGuid()
            };
            CountryCollection.Add(newRow);
            CurrentCountry = newRow;
        }

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem, _ => true); }
        }

        private void RemoveItem(object obj)
        {
            var dList = new List<CountriesViewModel>();
            foreach (var d in SelectedCountries)
                if (d.State == RowStatus.NewRow)
                    dList.Add(d);
            foreach (var r in dList) CountryCollection.Remove(r);
            var drows = new ObservableCollection<CountriesViewModel>(SelectedCountries);
            foreach (var d in drows)
            {
                CountryCollection.Remove(d);
                CountryDeleteCollection.Add(d);
            }
        }

        public override void CloseWindow(object form)
        {
            var WinManager = new WindowManager();
            var vin = form as Window;
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.No)
                    vin?.Close();
                if (res != MessageBoxResult.Yes) return;
                try
                {
                    SaveData(null);
                    vin?.Close();
                }
                catch (Exception ex)
                {
                    WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                }
            }
            else
            {
                vin?.Close();
            }
        }

        #endregion
    }
}
