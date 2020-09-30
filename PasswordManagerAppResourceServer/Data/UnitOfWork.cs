using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;


namespace PasswordManagerAppResourceServer.Data
{
    public class UnitOfWork: IUnitOfWork
    {
        public  ApplicationDbContext Context { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            Context = context;
            Users = new UserRepository(Context);
            Wallet = new WalletRepository(Context);
            
        }

        public IWalletRepository Wallet { get; private set; }
        public IUserRepository Users { get; private set; }

        

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

       
    }
}
