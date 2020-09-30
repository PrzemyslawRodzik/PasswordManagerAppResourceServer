
using PasswordManagerAppResourceServer.Data;
using PasswordManagerAppResourceServer.Models;
using System.Collections.Generic;


namespace PasswordManagerAppResourceServer.Interfaces
{
    public interface IWalletRepository: IRepositoryBase
    {
        IEnumerable<LoginData> GetAllLoginDataBreach();
        IEnumerable<PaypallAcount> GetAllPaypallBreach();
        int GetDataCountForUser<TEntity>(User user) where TEntity : UserRelationshipModel;
        int GetDataBreachForUser<TEntity>(User user) where TEntity : class, ICompromisedEntity;
         IEnumerable<LoginData> GetUnchangedPasswordsForUser(int userId);
    }
}
