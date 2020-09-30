
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;

namespace PasswordManagerAppResourceServer.Interfaces
{
   public interface IUnitOfWork
    {
        
        IUserRepository Users { get; }
        IWalletRepository Wallet { get; }

        
        ApplicationDbContext Context { get; }

        
        int SaveChanges();
    }
}
