using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using System.Collections.Generic;

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

        public Dictionary<string, int> GetStatisticData(int userId)
        {
            Dictionary<string, int> statisticData = new Dictionary<string,int>();
            var user = Users.Find<User>(userId);
            statisticData.Add("countCreditCards", Wallet.GetDataCountForUser<CreditCard>(user));
            statisticData.Add("countSharedData", Wallet.GetDataCountForUser<SharedLoginData>(user));
            statisticData.Add("countPasswords", Wallet.GetDataCountForUser<PaypalAccount>(user)+ Wallet.GetDataCountForUser<LoginData>(user));
            statisticData.Add("countCompromised", Wallet.GetDataBreachCountForUser<PaypalAccount>(user) + Wallet.GetDataBreachCountForUser<LoginData>(user));

            return statisticData;
        }








    }
}
