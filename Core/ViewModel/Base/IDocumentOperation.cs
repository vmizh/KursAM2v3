namespace Core.ViewModel.Base
{
    public interface IDocumentOperation
    {
        void Save();
        bool CanSave();
        void Load();
        bool CanLoad();
    }
}