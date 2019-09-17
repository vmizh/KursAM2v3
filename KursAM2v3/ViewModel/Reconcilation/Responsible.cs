using System.Collections.Generic;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Reconcilation
{
    public class Responsible : KursViewModelBase
    {
        public Responsible()
        {
            Corporates = new List<ResponsibleCorporate>();
        }

        public int? TabelNumber { set; get; }
        public int AllCorporate { set; get; }
        public int QuantityAOF { set; get; }
        public int QuatityNotAOF { set; get; }
        public List<ResponsibleCorporate> Corporates { set; get; }
    }
}