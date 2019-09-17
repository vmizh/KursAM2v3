using System;

namespace KursAM2.Managers.Base
{
    public abstract class DocumentManager<T> : ReferenceManager<T> where T : class
    {
        public string CheckedInfo { set; get; } = "Нет информации";
        public abstract T Load();
        public abstract T Load(decimal dc);
        public abstract T Load(Guid id);
        public abstract T Save(T doc);

        /// <summary>
        ///     Проверяет, что все поля в документе заполненны корректно и
        ///     документ может быть сохраен в базу данных
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public abstract bool IsChecked(T doc);

        public abstract T New();
        public abstract T NewFullCopy(T doc);
        public abstract T NewRequisity(T doc);
        public abstract void Delete(T doc);
        public abstract void Delete(decimal dc);
        public abstract void Delete(Guid id);
    }
}