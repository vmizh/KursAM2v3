using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Shop
{
    public class NomenklInfo : RSViewModelBase
    {

        public bool IsInDataBase { set; get; }
        public string NomenklNumber { set; get; }
        public string FullName { set; get; }
        public bool IsUsluga { set; get; } = false;
        public bool IsNakladExpense { set; get; } = false;
        public bool IsComplex { set; get; } = false;
        public bool IsDelete { set; get; } = false;
        public bool IsRentabelnost { set; get; } = false;
        public bool IsCurrencyTransfer { set; get; } = false;
        public NomenklGroup Category { set; get; }
        public Unit Unit { set; get; }
        public NomenklProductType NomType { set; get; }
        
    }
}