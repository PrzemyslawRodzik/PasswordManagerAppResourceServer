
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Data
{
    public class WalletRepository : RepositoryBase, IWalletRepository
    {
        public WalletRepository(ApplicationDbContext context) : base(context)
        {
        }

        public ApplicationDbContext ApplicationDbContext
        {
            get { return Context as ApplicationDbContext; }
        }

        public IEnumerable<LoginData> GetAllLoginDataBreach() 
        {
            try
            {
                return ApplicationDbContext.LoginDatas.Where(ld => ld.Compromised == 1).ToList();
            }
            catch (Exception )
            {
                return null;
            }
            
        }
        public IEnumerable<PaypalAccount> GetAllPaypallBreach()
        {

            try
            {
                return ApplicationDbContext.PaypalAccounts.Where(ld => ld.Compromised == 1).ToList();
            }
            catch (Exception)
            {
                return null;
            }



          
        } 
        public IEnumerable<LoginData> GetUnchangedPasswordsForUser(int userId)
        {   
            var allloginDatasList = ApplicationDbContext.LoginDatas.Where(x=>x.UserId==userId).ToList();
            var loginDatasList = allloginDatasList.Where(x => (DateTime.UtcNow.ToLocalTime() - x.ModifiedDate).Days>=30 ).ToList();
            
            return loginDatasList;
        }
        public int  GetDataCountForUser<TEntity>(User user) where TEntity: UserRelationshipModel
        {
            try
            {

            return ApplicationDbContext.Set<TEntity>().Where(ld => ld.UserId == user.Id).ToList().Count();
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public int GetDataBreachCountForUser<TEntity>(User user) where TEntity : UserRelationshipModel,ICompromisedModel
        {
            try
            {
            return ApplicationDbContext.Set<TEntity>().Where(ld => ld.UserId == user.Id && ld.Compromised==1).ToList().Count();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public List<T> GetAllUserData<T>(int userId) where T:UserRelationshipModel
        {
           
                return ApplicationDbContext.Set<T>().Where(tt => tt.UserId == userId).ToList();
            
            
            
        }
        public List<T> GetAllUserPhonesOrAddresses<T>(int userId) where T : PersonalModel
        {
            try
            {
                int personalId = 0;
                var personalInfo = ApplicationDbContext.PersonalInfos.FirstOrDefault(tt => tt.UserId == userId);
                if (!(personalInfo is null))
                    personalId = personalInfo.Id; 
                return ApplicationDbContext.Set<T>().Where(tt => tt.PersonalInfoId == personalId).ToList();

            }
            catch (Exception)
            {
                return null;
            }

        }

        public List<LoginData> GetOutOfDateLoginsForUser(int userId)
        {
            var logins = GetAllUserData<LoginData>(userId);
            if (logins is null)
                return null;
             var newOutOfDateLogins = logins.Where(x => (DateTime.UtcNow.ToLocalTime() - x.ModifiedDate).Days >= 30  && x.OutOfDate==0  ).ToList();
            
            if (newOutOfDateLogins is null)
                return null;
            UpdateOutOfDateLogins(newOutOfDateLogins);
            return newOutOfDateLogins;
        }
        private int UpdateOutOfDateLogins(List<LoginData> outOfDateLogins)
        {
            foreach (LoginData login in outOfDateLogins)
                login.OutOfDate = 1;
            ApplicationDbContext.LoginDatas.UpdateRange(outOfDateLogins);
            return ApplicationDbContext.SaveChanges();
        }
        
        





    }
    
}
