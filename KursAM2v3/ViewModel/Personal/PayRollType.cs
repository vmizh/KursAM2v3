using Core.ViewModel.Base.Column;
using Data;
using KursDomain.Documents.Employee;

namespace KursAM2.ViewModel.Personal
{
    public class PayrollType : EMP_PAYROLL_TYPEViewModel
    {
        public PayrollType(EMP_PAYROLL_TYPE entity) : base(entity)
        {
        }

        public PayrollType()
        {
        }

        public new bool Type
        {
            get => Entity.Type == 1;
            set
            {
                if (Entity.Type == (value ? 1 : 0)) return;
                Entity.Type = value ? 1 : 0;
                RaisePropertyChanged();
            }
        }
    }
}
