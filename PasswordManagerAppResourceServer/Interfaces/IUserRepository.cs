


using PasswordManagerAppResourceServer.Models;

namespace PasswordManagerAppResourceServer.Interfaces
{
    public interface IUserRepository :IRepositoryBase
    {
        bool CheckIfUserExist(string email);
        public string GetActiveToken(User authUser);
        public bool IsTokenActive(User authUser);
    }
}