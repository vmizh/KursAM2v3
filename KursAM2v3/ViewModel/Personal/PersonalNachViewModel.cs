using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;

namespace KursAM2.ViewModel.Personal
{
    public class PersonalNachViewModel : KursViewModelBase, IViewModel<NachForEmployeeModelOld>
    {
        public PersonalNachViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(NachForEmployeeModelOld));
            Source = new ObservableCollection<NachForEmployeeModelOld>();
            DeletedItems = new List<NachForEmployeeModelOld>();
        }

        #region IViewModel<NachForEmployeeModelOld> Members

        public ObservableCollection<NachForEmployeeModelOld> Source { get; set; }
        public ObservableCollection<NachForEmployeeModelOld> SourceAll { get; set; }
        public List<NachForEmployeeModelOld> DeletedItems { get; set; }

        #endregion
    }
}