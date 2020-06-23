namespace Core.ViewModel.Base
{
    public interface IViewModelToEntity<T> where T : class
    {
        T Entity { set; get; }
    }
}