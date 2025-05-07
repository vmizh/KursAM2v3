using Data;
using KursDomain;

namespace KursRepositories.Repositories.Base
{
    public interface IBaseRepository
    {
        ALFAMEDIAEntities Context { set; get; }
    }

    public abstract class BaseRepository : IBaseRepository
    {
        public ALFAMEDIAEntities Context { get; set; } = GlobalOptions.GetEntities();
    }

   
}
