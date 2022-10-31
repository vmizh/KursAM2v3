namespace Core.ViewModel.Base
{
    public interface ITreeReference<T, P>
    {
        T KeyId { get; }
        P ParentId { get; }
        string Name { set; get; }
    }
}