using System.Collections.Generic;
using Core.ViewModel.Base;
using KursDomain.Documents.Systems;

namespace KursAM2.View.Helper
{
    public class DocHistoryWindowViewModel : RSWindowViewModelBase
    {
        private DocHistoryViewModel myCurrentItem;

        public DocHistoryWindowViewModel(List<DocHistoryViewModel> list)
        {
            DocumentList = list;
        }

        public List<DocHistoryViewModel> DocumentList { set; get; }

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

        public override bool IsDocumentOpenAllow => CurrentItem != null;

        protected override void DocumentOpen(object obj)
        {
            DocumentHistoryManager.ShowDocument(CurrentItem.DocData);
        }
    }
}
