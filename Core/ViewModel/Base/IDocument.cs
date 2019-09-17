namespace Core.ViewModel.Base
{
    public interface IDocument<T>
    {
        bool IsCanSave { set; get; }
        void RefreshData();
        void Save();
        bool Check();
        T NewDocument();
        T CopyDocument();
        T CopyRequisite();
        void UnDeleteRows();
        T Document { set; get; }
    }
}