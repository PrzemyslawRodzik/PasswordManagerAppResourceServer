


using PasswordManagerAppResourceServer.Models;
using System.Collections.Generic;

namespace PasswordManagerAppResourceServer.Interfaces
{
    public interface IUserRepository :IRepositoryBase
    {
        bool CheckIfUserExist(string email);
        public string GetActiveToken(User authUser);
        public bool IsTokenActive(User authUser);
        
    }
}