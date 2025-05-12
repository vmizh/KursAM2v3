using KursDomain.Documents.Vzaimozachet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Data;

namespace KursRepositories.Repositories.MutualAccounting
{
    public interface IMutualAccountingRepository
    {
        string CheckedInfo { set; get; }

        SD_110 Get(decimal dc);

        IEnumerable<SD_110> GetForDates(DateTime dateStart, DateTime dateEnd, bool isConvert);
        IEnumerable<SD_110> GetForDates(DateTime dateStart, DateTime dateEnd);

        SD_110ViewModel Load(decimal dc);
        bool CheckDocumentForOld(decimal dc);
        SD_110ViewModel Save(SD_110ViewModel doc);
        bool IsChecked(SD_110ViewModel doc);
        public SD_110ViewModel New();
        public SD_110ViewModel NewFullCopy(SD_110ViewModel doc);
        public SD_110ViewModel NewRequisity(SD_110ViewModel doc);
        public void Delete(SD_110ViewModel doc);
        public void Delete(decimal dc);

        public TD_110ViewModel AddRow(ObservableCollection<TD_110ViewModel> rows, TD_110ViewModel row,
            // ReSharper disable once OptionalParameterHierarchyMismatch
            short dolg = 0);

        public TD_110ViewModel DeleteRow(ObservableCollection<TD_110ViewModel> rows,
            ObservableCollection<TD_110ViewModel> deletedrows, TD_110ViewModel row);

        public Tuple<bool, string> IsRowChecked(TD_110ViewModel r);

    }
}
