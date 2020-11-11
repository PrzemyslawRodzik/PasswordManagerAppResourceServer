﻿using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.DataProtection;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using OtpNet;
using EmailService;
using PasswordManagerAppResourceServer.Controllers;
using PasswordManagerAppResourceServer.Results;
using PasswordManagerAppResourceServer.Handlers;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel;
using Microsoft.IdentityModel.Tokens;
using PasswordManagerAppResourceServer.Dtos;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using PasswordManagerAppResourceServer.CustomExceptions;

namespace PasswordManagerAppResourceServer.Services
{
    public class UserService : IUserService
    {



        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public DataProtectionHelper dataProtectionHelper;
        
        private readonly IConfiguration _config;
        private readonly EncryptionService _encryptService;
        private readonly IEmailSender _emailSender;
        

        


        public UserService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider,IEmailSender emailSender, IConfiguration config, EncryptionService encryptService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;  
             _emailSender = emailSender;
            dataProtectionHelper = new DataProtectionHelper(provider);
            _config = config;
            _encryptService = encryptService;

        }

        

        public User Create(string email, string password){  
            if (string.IsNullOrWhiteSpace(password))
                throw new AuthenticationException("Password is required");
            if (VerifyEmail(email))
                throw new AuthenticationException("Email \"" + email + "\" is already taken");
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            User user = new User
            {
                Email = email,
                Password = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                TwoFactorAuthorization = 0,
                PasswordNotifications = 1,
                AuthenticationTime = 5,
                Admin = 0
            };
            _unitOfWork.Users.Add<User>(user);
            _unitOfWork.SaveChanges();
            _emailSender.SendEmailAsync(new Message(new string[] { user.Email }, "Welcome to PasswordManagerApp.com!", "Welcome to PasswordManagerApp.com " + user.Email + " Your account was successfully created."));
            return user;
        }


        
        public int GetAuthUserId()
        {   
            
            try
            {
                int id =  Int32.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
            
                return id;
            }catch(NullReferenceException)
            {
                return -1;
            }
            
        }

        

        public void Update(User user, string password = null)
        {
            // TO DO
        }
        
        public void UpdatePasswordStatus(int userId, int compromised)
        {
            var user = _unitOfWork.Users.GetById<User>(userId);
            //user.Compromised = compromised;
            _unitOfWork.Users.Update<User>(user);
            _unitOfWork.SaveChanges();
        }



        public bool VerifyEmail(string email)
        {
        
            return _unitOfWork.Users.CheckIfUserExist(email);
        }




        public User Authenticate(string email, string password)
        {

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _unitOfWork.Users.SingleOrDefault<User>(x => x.Email == email);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, Convert.FromBase64String(user.Password), Convert.FromBase64String(user.PasswordSalt)))
                return null;

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _unitOfWork.Users.GetAll<User>();
        }




        public User GetById(int? id)
        {   
            if(id is null)
                return null;
            return _unitOfWork.Users.Find<User>((int)id);
            
        }

        public bool DeleteUser(int id)
        {
             User user = GetById(id);


            if (user != null)
            {
                _unitOfWork.Users.Remove<User>(user);
                _unitOfWork.SaveChanges();
                return true;
            }
            return false;

           
           
        }


        public Task<User> AuthenticateExternal(string id)
        {
            throw new NotImplementedException();
        }

        public Task<User> AddExternal(string id, string email)
        {
            throw new NotImplementedException();
        }
        public bool ChangeMasterPassword(string oldPassword,string newPassword,string authUserId)
        {   
            var authUserIdToInt32 = Int32.Parse(authUserId);
            User user = _unitOfWork.Users.Find<User>(authUserIdToInt32);
            byte[] passwordHash, passwordSalt;
            EncryptWithNewPassword(authUserIdToInt32, oldPassword,newPassword);
            CreatePasswordHash(newPassword, out passwordHash, out passwordSalt);
            user.Password = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);
            try
            {
                _unitOfWork.Users.Update<User>(user);
                _unitOfWork.SaveChanges();
                _emailSender.SendEmailAsync(new Message(new string[] { user.Email }, "PasswordManagerApp Password Change", "Your password has been changed  " + DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd' 'HH:mm:ss") + "."));
                return true;
            }
            catch(Exception)
            {
                throw new UserServiceException("There was an during password change. Contact support or try again later.");
            }
         }
        private void EncryptWithNewPassword(int userId,string oldPassword,string newPassword)
        {
            var logins = _unitOfWork.Wallet.GetAllUserData<LoginData>(userId);
            var paypalAccounts = _unitOfWork.Wallet.GetAllUserData<PaypalAccount>(userId);
            var notes = _unitOfWork.Wallet.GetAllUserData<Note>(userId);
            var addresses = _unitOfWork.Wallet.GetAllUserPhonesOrAddresses<Address>(userId);
            var personals = _unitOfWork.Wallet.GetAllUserData<PersonalInfo>(userId);
            var phoneNumbers = _unitOfWork.Wallet.GetAllUserPhonesOrAddresses<PhoneNumber>(userId);
            var creditCards = _unitOfWork.Wallet.GetAllUserData<CreditCard>(userId);
            
            foreach (var x in logins)
            {
                x.Password = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.Password));
            }
            foreach (var x in paypalAccounts)
            {
                x.Password = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.Password));
            }
            foreach (var x in notes)
            {
                x.Details = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.Details));
            }
            foreach (var x in addresses)
            {
                x.Street = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.Street));
                x.ZipCode = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.ZipCode));
                x.City = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.City));
            }
            foreach (var x in phoneNumbers)
            {
               
                x.TelNumber = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.TelNumber));

            }
            foreach (var x in creditCards)
            {
                x.CardNumber = _encryptService.Encrypt(newPassword,_encryptService.Decrypt(oldPassword, x.CardNumber));
                x.CardHolderName = _encryptService.Encrypt(newPassword,_encryptService.Decrypt(oldPassword, x.CardHolderName));
                x.SecurityCode = _encryptService.Encrypt(newPassword,_encryptService.Decrypt(oldPassword, x.SecurityCode));
            }
            foreach (var x in personals)
            {
                x.Name = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.Name));
                x.LastName = _encryptService.Encrypt(newPassword, _encryptService.Decrypt(oldPassword, x.LastName));
                
            }
            _unitOfWork.Context.LoginDatas.UpdateRange(logins);
            _unitOfWork.Context.PaypalAccounts.UpdateRange(paypalAccounts);
            _unitOfWork.Context.Notes.UpdateRange(notes);
            _unitOfWork.Context.Addresses.UpdateRange(addresses);
            _unitOfWork.Context.PhoneNumbers.UpdateRange(phoneNumbers);
            _unitOfWork.Context.CreditCards.UpdateRange(creditCards);
            _unitOfWork.Context.PersonalInfos.UpdateRange(personals);

            _unitOfWork.SaveChanges();

        }
        


        private  void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new UserServiceException("Password is null");
            if (string.IsNullOrWhiteSpace(password)) throw new UserServiceException("Value cannot be empty or whitespace only string.");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        
        private string GenerateTotpToken(User authUser)
        {   string totpToken;
            string sysKey = _config["TotpKey"];
            var key_b = Encoding.UTF8.GetBytes(sysKey + authUser.Email);


            Totp totp = new Totp(secretKey: key_b, mode: OtpHashMode.Sha512, step: 300, timeCorrection: new TimeCorrection(DateTime.UtcNow));
            totpToken = totp.ComputeTotp(DateTime.UtcNow);


            return totpToken;
        }
        private void SaveToDb(User authUser, string totpToken)
        {
            bool tokenIsActive = _unitOfWork.Users.IsTokenActive(authUser);  
            if (tokenIsActive)
                return;
            
            _unitOfWork.Context.Totp_Users.Add(
                new Totp_user() {
                    Token = totpToken,
                    Create_date = DateTime.UtcNow,
                    Expire_date = DateTime.UtcNow.AddSeconds(300),
                    User = authUser




                });
            _unitOfWork.SaveChanges();
        }
        public bool CheckUserGuidDeviceInDb(string GuidDeviceHashFromCookie, int userId)
        {
            
            if (_unitOfWork.Context.UserDevices.Any(ud => ud.UserId == userId && ud.DeviceGuid == GuidDeviceHashFromCookie))
                return true;
            else
                return false;

        }
        private string GetUserIpAddress() => _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        public bool CheckPreviousUserIp(int userId, string ipAddress) => 
            _unitOfWork.Context.UserDevices.Any(x => x.UserId == userId && x.IpAddress.Equals(ipAddress));
        
            
           
            
        
        private string dataToSHA256(string data)
        {
            SHA256 mysha256 = SHA256.Create();
            return Convert.ToBase64String(mysha256.ComputeHash(Encoding.UTF8.GetBytes(data)));

        }


        

        public bool VerifyPasswordHash(string password, byte[] storedHash, 
            byte[] storedSalt)
        {
            if (password == null || 
                string.IsNullOrWhiteSpace(password) || 
                storedHash.Length != 64 || 
                storedSalt.Length != 128)
                    return false;
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        public bool AddNewDeviceToDb(string newOsHash, int userId, string ipAddress)
        {
            
            if (!_unitOfWork.Context.UserDevices.Any(b => b.UserId == userId && b.DeviceGuid == newOsHash))
            {

                UserDevice ud = new UserDevice();
                ud.UserId = userId;
                ud.Authorized = 1;
                ud.DeviceGuid = newOsHash;
                ud.IpAddress = ipAddress;

                _unitOfWork.Context.UserDevices.Add(ud);
                _unitOfWork.SaveChanges();


                return true;


            }
            return false;
        }
       

        
        
        public void InformAllUsersAboutOldPasswords()
        {   
            
                var allloginDatasList = _unitOfWork.Context.LoginDatas.ToList();
                if(allloginDatasList is null)
                    return;
                var loginDatasListWithOldPasswords = allloginDatasList.Where(x => (DateTime.UtcNow.ToLocalTime() - x.ModifiedDate).Days>=30 ).ToList();
                if(loginDatasListWithOldPasswords is null)
                    return;
                string websitesList = "";
                foreach (var item in loginDatasListWithOldPasswords.GroupBy(x => x.UserId))
                {   
                    websitesList="";
                    var userEmail = GetById(item.Key).Email;
                    item.ToList().ForEach(x => websitesList+=x.Website+", "   );


                    string message = $"wykryto {item.Count()} hasła nie zmieniane od 30 dni dla podanych stron internetowych : {websitesList}!";


                    _emailSender.SendEmailAsync(new Message(new string[] { userEmail },"PasswordManagerApp stare hasła", message));


                }
       
            
            
            
            
            
            
                
        }
        public void InformUserAboutOldPasswords(int userId)
        {
             string userEmail = GetById(userId).Email;
             var allUserLoginData = _unitOfWork.Context.LoginDatas.Where(x=>x.UserId==userId).ToList();
                if(allUserLoginData is null)
                    return;
                var loginDataListWithOldPasswords = allUserLoginData.Where(x => (DateTime.UtcNow.ToLocalTime() - x.ModifiedDate).Days>=30 ).ToList();
                if(loginDataListWithOldPasswords is null)
                    return;
                string websitesList = "";

                loginDataListWithOldPasswords.ForEach(x=>websitesList+=x.Website+", ");
                
                string message = $"wykryto {loginDataListWithOldPasswords.Count} hasła nie zmieniane od 30 dni dla podanych stron internetowych : {websitesList}.";

              //  _emailSender.SendEmailAsync(new Message(new string[] { userEmail },"PasswordManagerApp stare hasła", message));
        }
        

       
        public void CreateAndSendAuthorizationToken(int authUserId,string userPassword)
        {
            User authUser = GetById(authUserId);
            string token = authUser.Id.ToString() + "|" + authUser.Email + "|" + DateTime.UtcNow.AddMinutes(10).ToString();
            token = dataProtectionHelper.Encrypt(token,userPassword);
            string url  = QueryHelpers.AddQueryString($"{_config["WebDomain"]}auth/deleteaccount2step", "token", token);
            _emailSender.SendEmailAsync(new Message(new string[] { authUser.Email }, 
                "Link do usunięcia konta. Pass Manager App", "Link do usunięcia konta w serwisie Pass Manager App : " + url + " dla uzytkownika: " 
                + authUser.Email + " Podany link będzie aktywny przez 10 minut."));
        }


        public bool ValidateToken(string token,string password){   
            var authUserId = _httpContextAccessor.
                                HttpContext.User.Identity.Name;
            User authUser = GetById(Int32.Parse(authUserId));
            string decryptedToken = "";
            try
            {
                decryptedToken = dataProtectionHelper.Decrypt(token, password);
            }
            catch(CryptographicException)
            {
                return false;
            }
            var tokenArray = decryptedToken.Split("|");
            if(tokenArray[0].Equals(authUserId) && tokenArray[1].Equals(authUser.Email) )
            {
                DateTime expiredDate = DateTime.Parse(tokenArray[2]);
                if(DateTime.Compare(DateTime.UtcNow,expiredDate)<0)
                {
                    return true;
                }
            }
            return false;
        }










        public void SendTotpToken(User authUser)
        {
            string totpToken;
            bool isActive =  _unitOfWork.Users.IsTokenActive(authUser);
           
            if (isActive)
            {
                totpToken = _unitOfWork.Users.GetActiveToken(authUser);
                _emailSender.SendEmailAsync(new Message(new string[] { authUser.Email }, "Jednorazowy kod dostępu. Pass Manager App", "Jednorazowy kod dostępu do konta: " + totpToken + " dla uzytkownika: " + authUser.Email + " Podany kod musisz wprowadzic w ciagu 5min"));
                
                return;
            }
            totpToken = GenerateTotpToken(authUser);
            SaveToDb(authUser, totpToken);
            _emailSender.SendEmailAsync(new Message(new string[] { authUser.Email }, "Jednorazowy kod dostępu. Pass Manager App", "Jednorazowy kod dostępu do konta: " + totpToken + " dla uzytkownika: " + authUser.Email + " Podany kod musisz wprowadzic w ciagu 5min"));
        }

        

        private enum ResultsToken
        {
            NotMatched,
            Matched,
            Expired,
            
        }


        public int VerifyTotpToken(User authUser,string totpToken)
        {
            string sysKey = _config["TotpKey"];
            long lastUse;
            Totp totp = new Totp(secretKey: Encoding.UTF8.GetBytes(sysKey + authUser.Email), mode: OtpHashMode.Sha512, step: 300,timeCorrection:new TimeCorrection(DateTime.UtcNow));
            var activeTokenRecordFromDb = _unitOfWork.Context.Totp_Users.FirstOrDefault(b => b.UserId == authUser.Id && b.Token == totpToken);
            if (activeTokenRecordFromDb != null)
            {
               if (activeTokenRecordFromDb.Expire_date >= DateTime.UtcNow)
                {
                    return totp.VerifyTotp(totpToken, out lastUse,window:new VerificationWindow(1,1)) ? (int)ResultsToken.Matched:(int)ResultsToken.NotMatched; 
                }
                else
                {
                    return (int)ResultsToken.Expired;
                }

            }
            return (int)ResultsToken.NotMatched;
        }

        public AccessToken GenerateAuthToken(User user)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretEncryptionKey"]);
            var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(
                source: Convert.FromBase64String(_config["JwtSettings:Asymmetric:PrivateKey"]),
                bytesRead: out int _);
            var signingCredentials = new SigningCredentials(
                key: new RsaSecurityKey(rsa),
                algorithm: SecurityAlgorithms.RsaSha256
            );
            var cryptoKey = new EncryptingCredentials(symmetricSecurityKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);
            var expirationDate = DateTime.UtcNow.AddMinutes(user.AuthenticationTime != 0 ? user.AuthenticationTime : 5);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                  {
                     new Claim(ClaimTypes.Name,user.Id.ToString()), 
                     new Claim(ClaimTypes.Email,user.Email),
                     new Claim("Admin", user.Admin.ToString()),
                     new Claim("TwoFactorAuth", user.TwoFactorAuthorization.ToString()),
                     new Claim("PasswordNotifications", user.PasswordNotifications.ToString()),
                     new Claim("AuthTime", user.AuthenticationTime.ToString()),
                  }),
                Expires = expirationDate,
                Audience = _config["JwtSettings:Audience"],
                Issuer = _config["JwtSettings:Issuer"],
                NotBefore = DateTime.UtcNow.AddMilliseconds(-2000),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                EncryptingCredentials = cryptoKey
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenJson = tokenHandler.WriteToken(token);
            return new AccessToken
            {
                JwtToken = tokenJson,
                Expire = expirationDate
            };
        }



        public void UpdatePreferences(UpdatePreferencesWrapper upPreferences, int userId)
        {
            User user = _unitOfWork.Users.Find<User>(userId);
            user.AuthenticationTime = Int32.Parse(upPreferences.VerificationTime);
            user.PasswordNotifications = Int32.Parse(upPreferences.PassNotifications);
            user.TwoFactorAuthorization = Int32.Parse(upPreferences.TwoFactor);
            try
            {
                _unitOfWork.Users.Update<User>(user);
                _unitOfWork.SaveChanges();    
            }
            catch (Exception)
            {
                throw new UserServiceException(
                    "There was an error during update. Try again or contact support.");
            }
        }

        
    }
}