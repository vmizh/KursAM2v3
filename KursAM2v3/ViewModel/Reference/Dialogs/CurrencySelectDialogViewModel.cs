using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.ViewModel.Base;
using KursAM2.View.DialogUserControl;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference.Dialogs
{
    public class CurrencySelectDialogViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public CurrencySelectDialogViewModel(IEnumerable<Currency> withoutCrs = null)
        {
            myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            WindowName = "Выбор валюты";
            if (withoutCrs != null && withoutCrs.Any())
            {
                ItemsCollection = new ObservableCollection<Currency>();
                foreach (var c in GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>())
                {
                    if (withoutCrs.Any(_ => _.DocCode == c.DocCode)) continue;
                    ItemsCollection.Add(c);
                }
            }
            else
            {
                ItemsCollection =
                    new ObservableCollection<Currency>(
                        GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>());
            }
        }

        #endregion

        #region Commands

        public override void Cancel(object obj)
        {
            CurrentItem = null;
            base.Cancel(obj);
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private Currency myCurrentItem;

        #endregion

        #region Properties

        public StandartDialogSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Currency> ItemsCollection { set; get; }

        public Currency CurrentItem
        {
            get => myCurrentItem;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
