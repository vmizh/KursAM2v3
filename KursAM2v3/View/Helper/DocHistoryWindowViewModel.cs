using System.Collections.Generic;
using System.Windows;
using Core.ViewModel.Base;
using Core.ViewModel.Common;

namespace KursAM2.View.Helper
{
    public class DocHistoryWindowViewModel : RSWindowViewModelBase
    {
        private DocHistoryViewModel myCurrentItem;
        public List<DocHistoryViewModel> DocumentList { set; get; }

        public DocHistoryWindowViewModel(List<DocHistoryViewModel> list)
        {
            DocumentList = list;
        }

        public DocHistoryViewModel CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            } 
        }

        public override bool IsDocumentOpenAllow  =>  CurrentItem != null;

        public override void DocumentOpen(object obj)
        {
            DocumentHistoryManager.ShowDocument(CurrentItem.DocData);
        }
    }
}