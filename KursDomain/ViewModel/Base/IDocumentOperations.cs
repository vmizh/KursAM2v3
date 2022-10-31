using System;

namespace Core.EntityViewModel.Base
{
    public interface IDocumentOperations<T> where T : class
    {
        T CreateNew();
        T CreateCopy(T ent);
        T CreateCopy(Guid id);
        T CreateCopy(decimal dc);

        T CreateRequisiteCopy(T ent);
        T CreateRequisiteCopy(Guid id);
        T CreateRequisiteCopy(decimal dc);
    }

    public interface IDocumentWithRowOperations<T, R> : IDocumentOperations<T> where T : class
    {
        R CreateRowNew(T head);
        R CreateRowCopy(R oldent);
        void DeleteRow(R ent);
        void LoadRow(T ent, R rowent);
    }
}