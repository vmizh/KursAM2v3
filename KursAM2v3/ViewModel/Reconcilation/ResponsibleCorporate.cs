using System.Collections.Generic;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Reconcilation
{
    public class ResponsibleCorporate : RSViewModelBase
    {
        public ResponsibleCorporate()
        {
            Acts = new List<ActOfResponsibleShort>();
        }

        public List<ActOfResponsibleShort> Acts { set; get; }
        public int ActCount => Acts.Count;
    }
}