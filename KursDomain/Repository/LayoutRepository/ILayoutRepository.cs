using System;
using System.Threading.Tasks;
using Data;
using KursDomain.Annotations;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.LayoutRepository
{
    public interface ILayoutRepository : IKursGenericRepository<FormLayout, Guid>
    {
        [CanBeNull]
        Task<FormLayout> GetByFormNameAsync(string formName);

        Task SaveLayoutAsync (string formName, string layout, string WinState);
    }
}
