using System;

namespace KursAM2.ViewModel.Fabric.Base
{
    public abstract class ViewModelFabric
    {
        public virtual object CreateViewModel()
        {
            return new object();
        }

        public virtual object CreateViewModel(decimal docCode)
        {
            return new object();
        }

        public virtual object CreateViewModel(Guid id)
        {
            return new object();
        }
    }
}