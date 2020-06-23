using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Data;
using KursRepozit.Repository.Base;

namespace KursRepozit.Repository
{
    public interface IUsersRepository : IGenericRepository<Users>
    {
        Users GetById(Guid id);
        Users GetByName(string name);
        List<Users> GetAllWithDataSources();

        List<DataSources> GetAllDataSources();

    }

    public class UserRepository : GenericRepository<Users>, IUsersRepository
    {

        // ReSharper disable once NotAccessedField.Local
       public UserRepository(IUnitOfWork<KursSystemEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public UserRepository(KursSystemEntities context) : base(context)
        {
        }
        public Users GetById(Guid id)
        {
            return Context.Users.SingleOrDefault(_ => _.Id == id);
        }

        public Users GetByName(string name)
        {
            return Context.Users.SingleOrDefault(_ => _.Name == name);
        }

        public List<Users> GetAllWithDataSources()
        {
            return Context.Users.Include(_ => _.DataSources).OrderBy(_ => _.Name).ToList();
        }

        public List<DataSources> GetAllDataSources()
        {
            return Context.DataSources.ToList();
        }
        
    }
}