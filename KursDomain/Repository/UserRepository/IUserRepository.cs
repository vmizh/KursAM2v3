using System;
using System.Collections.Generic;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.UserRepository;


public interface IUserRepository : IKursGenericRepository<Users,Guid>
{
    List<Users> GetAllUsers();
    Users GetById(Guid id);
    List<SD_27> GetStores(Guid id);
    EXT_USERS GetUsers(string name);
    public List<Users> GetUsersForStores(decimal storeDC);

}
