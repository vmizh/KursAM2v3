using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;

namespace KursAM2.ViewModel.Logistiks
{
    public class DogovorForClientSearchViewModel : RSWindowSearchViewModelBase
    {
        private DogovorForClientViewModel myCurrentDogovor;

        public DogovorForClientSearchViewModel()
        {
            WindowName = "Договора реализации для клиентов";
            Result = new ObservableCollection<DogovorForClientViewModel>();
        }

        public DogovorForClientSearchViewModel(Window form) : base(form)
        {
            WindowName = "Договора реализации для клиентов";
            FirstSearchName = "Контрагент";
            SecondSearchName = "Номенклатура";
            Result = new ObservableCollection<DogovorForClientViewModel>();
        }

        public DogovorForClientViewModel CurrentDogovor
        {
            get => myCurrentDogovor;
            set
            {
                if (myCurrentDogovor != null && myCurrentDogovor.Equals(value)) return;
                myCurrentDogovor = value;
                if (myCurrentDogovor != null)
                {
                    IsDocumentOpenAllow = true;
                    IsDocNewCopyAllow = true;
                    IsDocNewCopyRequisiteAllow = true;
                    IsPrintAllow = true;
                }

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<DogovorForClientViewModel> Result { set; get; }

        public override void RefreshData(object data)
        {
            base.RefreshData(data);
            IsDocumentOpenAllow = false;
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = false;
            IsPrintAllow = false;
            try
            {
                Result.Clear();
                foreach (var item in GlobalOptions.GetEntities()
                    .SD_9
                    .Include(_ => _.SD_43)
                    .Include(_ => _.SD_431)
                    .Include(_ => _.SD_432)
                    .Include(_ => _.SD_437)
                    .Include(_ => _.SD_301)
                    .Include(_ => _.SD_189)
                    .Include(_ => _.SD_102)
                    .Include(_ => _.TD_9)
                    .Include("TD_9.SD_83")
                    .ToList())
                    Result.Add(new DogovorForClientViewModel(item));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            RaisePropertyChanged(nameof(Result));
        }
    }
}