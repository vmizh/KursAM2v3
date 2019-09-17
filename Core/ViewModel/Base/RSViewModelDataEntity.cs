using System;
using JetBrains.Annotations;

namespace Core.ViewModel.Base
{
    public abstract class RSViewModelDataEntity<T> : RSViewModelData where T : class
    {
        public abstract T Entity { set; get; }

        public virtual void SetEntity([NotNull] T entity)
        {
            Entity = entity;
        }

        public abstract void SetEntity(Guid entityId);
        public abstract void SetEntity(decimal entityDocCode);
    }
}