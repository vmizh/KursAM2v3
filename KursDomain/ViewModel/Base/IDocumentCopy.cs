using System;

namespace Core.ViewModel.Base
{
    public interface IDocumentCopy
    {
        void SetAsNewCopyRequisite(Guid? id);
        void SetAsNewCopy();

    }
}