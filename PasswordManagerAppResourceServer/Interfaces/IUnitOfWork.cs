
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using System.Collections.Generic;

namespace PasswordManagerAppResourceServer.Interfaces
{
   public interface IUnitOfWork
    {
        
        IUserRepository Users { get; }
        IWalletRepository Wallet { get; }

        
        ApplicationDbContext Context { get; }

        
        int SaveChanges();
        Dictionary<string, int> GetStatisticData(int userId);
    }
}
