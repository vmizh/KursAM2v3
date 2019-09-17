using System;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Personal
{
    public class PayRollVedomostViewModel : RSViewModelDataEntity<EMP_PR_DOC>
    {
        public override EMP_PR_DOC Entity { get; set; }

        public override void SetEntity(Guid entityId)
        {
            throw new NotImplementedException();
        }

        public override void SetEntity(decimal entityDocCode)
        {
            throw new NotImplementedException();
        }
    }
}