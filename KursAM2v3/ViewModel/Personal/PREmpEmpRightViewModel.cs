using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using DevExpress.Xpf.Grid;
using KursDomain.Documents.Employee;

namespace KursAM2.ViewModel.Personal
{
    public class PREmpEmpRightViewModel : RSViewModelBase, IViewModel<Employee>
    {
        public PREmpEmpRightViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(Employee));
            foreach (var col in TableViewInfo.Columns)
                col.ReadOnly = true;
            Source = new ObservableCollection<Employee>();
            DeletedItems = new List<Employee>();
        }

        #region IViewModel<Persona> Members

        public GridTableViewInfo TableViewInfo { get; set; }
        public GridControl Grid { get; set; }
        public ObservableCollection<Employee> Source { get; set; }
        public ObservableCollection<Employee> SourceAll { get; set; }
        public List<Employee> DeletedItems { get; set; }

        public void ResetSummary()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
