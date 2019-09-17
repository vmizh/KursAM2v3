using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;

namespace KursAM2.ViewModel.Management.BreakEven
{
    public class BreakEvenDocumentViewModel : KursViewModelBase, IViewModel<DocumentRow>
    {
        public BreakEvenDocumentViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(DocumentRow));
            Source = new ObservableCollection<DocumentRow>();
        }

        #region IViewModel<DocumentRow> Members

        public ObservableCollection<DocumentRow> Source { get; set; }
        public ObservableCollection<DocumentRow> SourceAll { get; set; }
        public List<DocumentRow> DeletedItems { get; set; }

        #endregion
    }
}