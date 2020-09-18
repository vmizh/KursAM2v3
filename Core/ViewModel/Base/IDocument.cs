namespace Core.ViewModel.Base
{
    public interface IDocument<T>
    {
        bool IsCanSave { set; get; }
        T Document { set; get; }
        void RefreshData();
        void Save();
        bool Check();
        T NewDocument();
        T CopyDocument();
        T CopyRequisite();
        void UnDeleteRows();
    }
}