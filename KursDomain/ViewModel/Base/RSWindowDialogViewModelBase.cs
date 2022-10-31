namespace Core.ViewModel.Base
{
    public abstract class RSWindowDialogViewModelBase : RSWindowViewModelBase
    {
        public virtual object ReturnData { set; get; }

        public virtual void Ok()
        {
        }

        public virtual void Cancel()
        {
        }
    }
}