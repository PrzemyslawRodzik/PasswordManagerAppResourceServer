
using PasswordManagerAppResourceServer.Data;
using PasswordManagerAppResourceServer.Models;
using System.Collections.Generic;


namespace PasswordManagerAppResourceServer.Interfaces
{
    public interface IWalletRepository: IRepositoryBase
    {
        IEnumerable<LoginData> GetAllLoginDataBreach();
        IEnumerable<PaypalAccount> GetAllPaypallBreach();
        int GetDataCountForUser<TEntity>(User user) where TEntity : UserRelationshipModel;
        int GetDataBreachCountForUser<TEntity>(User user) where TEntity : UserRelationshipModel, ICompromisedModel;
        IEnumerable<LoginData> GetUnchangedPasswordsForUser(int userId);

        List<T> GetAllUserData<T>(int userId) where T : UserRelationshipModel;
        List<T> GetAllUserPhonesOrAddresses<T>(int userId) where T : PersonalModel;
        List<LoginData> GetOutOfDateLoginsForUser(int userId);
        
    }
}
