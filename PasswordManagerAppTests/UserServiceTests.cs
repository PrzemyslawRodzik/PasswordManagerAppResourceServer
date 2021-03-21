
using EmailService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;
using PasswordManagerAppResourceServer.Data;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using PasswordManagerAppResourceServer.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PasswordManagerAppTests
{
    public class UserServiceTests
    {

        private IUnitOfWork _unitOfWork;
        private IUserService _userService;
        private DbContextOptions<ApplicationDbContext> _options;
        private IConfiguration _config;

        // Mocks

        private Mock<IEmailSender> mockEmail = new Mock<IEmailSender>();
        private Mock<IEncryptionService> mockEncryptionService = new Mock<IEncryptionService>();


        public UserServiceTests()
        {

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
         .UseInMemoryDatabase(databaseName: "TestDatabase")
               .Options;











         


            mockEmail.Setup(x => x.SendEmailAsync(It.IsAny<Message>())).Returns(Task.FromResult(true));

            mockEncryptionService.Setup(x => x.Encrypt(It.IsAny<string>(), (It.IsAny<string>()))).Returns("random string");

            _config = new ConfigurationBuilder()
           .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
           .AddJsonFile("appsettings.json")
           .Build();


            
            
            _unitOfWork = new UnitOfWork(new ApplicationDbContext(_options));
            _userService = new UserService(_unitOfWork, new Mock<IHttpContextAccessor>().Object, new Mock<IDataProtectionProvider>().Object, mockEmail.Object, _config,  mockEncryptionService.Object);




        }

        private enum ResultsToken
        {
            NotMatched,
            Matched,
            Expired,

        }

        public User CreateRandomUser(string email = "test@test.com",
            string password="zxczxc", int tFA = 0, int PNot = 1, int authTime= 25)
        {
            byte[] phash, psalt;
            _userService.CreatePasswordHash(password, out phash, out psalt);
            return new User
            {
                Email = email,
                Password = Convert.ToBase64String(phash),
                PasswordSalt =Convert.ToBase64String(psalt),
                TwoFactorAuthorization = 0,
                PasswordNotifications = 1,
                AuthenticationTime = 25,
            };
        }
        [Fact]
        public void VerifyTotpToken_InvalidTokenShouldReturnNotMatchedResult()
        {




            var testUser = CreateRandomUser();

            string totpTokentoCheck = _userService.GenerateTotpToken(testUser);




            int actual = _userService.VerifyTotpToken(testUser, "zxczxczxczc");



            Assert.Equal((int)ResultsToken.NotMatched, actual);

        }
        [Fact]
        public void VerifyTotpToken_ExpiredTokenShouldReturnExpiredResult()
        {
            var testUser = CreateRandomUser();

            string totpTokentoCheck = _userService.GenerateTotpToken(testUser);
            _unitOfWork.Context.Totp_Users.Add(new Totp_user
            {
                Token = totpTokentoCheck,
                Create_date = DateTime.UtcNow,
                Expire_date = DateTime.UtcNow.AddSeconds(-100),
                User = testUser


            });

            _unitOfWork.Context.SaveChanges();

            var actual = _userService.VerifyTotpToken(testUser, totpTokentoCheck);

            Assert.Equal((int)ResultsToken.Expired, actual);

        }
        [Fact]
        public void VerifyTotpToken_MatchedAndValidTokenShouldReturnMatchedResult()
        {


            var testUser = CreateRandomUser();


            string totpTokentoCheck = _userService.GenerateTotpToken(testUser);


            _unitOfWork.Context.Totp_Users.Add(new Totp_user
            {
                Token = totpTokentoCheck,
                Create_date = DateTime.UtcNow,
                Expire_date = DateTime.UtcNow.AddSeconds(300),
                User = testUser


            });

            _unitOfWork.Context.SaveChanges();

            var actual = _userService.VerifyTotpToken(testUser, totpTokentoCheck);

            Assert.Equal((int)ResultsToken.Matched, actual);
        }
        [Fact]
        public void VerifyTotpToken_MatchedButExpiredTokenShouldReturnExpiredResult()
        {
            var testUser = CreateRandomUser();

            string totpTokentoCheck = _userService.GenerateTotpToken(testUser);
            _unitOfWork.Context.Totp_Users.Add(new Totp_user
            {
                Token = totpTokentoCheck,
                Create_date = DateTime.UtcNow,
                Expire_date = DateTime.UtcNow.AddSeconds(-100),
                User = testUser


            });

            _unitOfWork.Context.SaveChanges();

            var actual = _userService.VerifyTotpToken(testUser, totpTokentoCheck);

            Assert.Equal((int)ResultsToken.Expired, actual);
            var valueInDb = _unitOfWork.Context.Totp_Users.FirstOrDefault(c => c.Token == totpTokentoCheck);
            Assert.NotNull(valueInDb);
            Assert.True(valueInDb.Token == totpTokentoCheck);
        }
        [Fact]
        public void SendTotpToken_EmailMethodShouldBeCalledOnlyOncePerRequest()
        {
            _userService.SendTotpToken(CreateRandomUser());

            mockEmail.Verify(x => x.SendEmailAsync(It.IsAny<Message>()), Times.Once);





        }
        [Fact]
        public void IsTokenActive_TokenIsActive()
        {
            var user = CreateRandomUser();
            var totpTokentoCheck = _userService.GenerateTotpToken(user);
            _unitOfWork.Context.Totp_Users.Add(new Totp_user
            {
                Token = totpTokentoCheck,
                Create_date = DateTime.UtcNow,
                Expire_date = DateTime.UtcNow.AddSeconds(1),
                User = user


            });

            _unitOfWork.Context.SaveChanges();
            var value = _unitOfWork.Users.IsTokenActive(user);

            Assert.True(value);

        }
        [Fact]
        public void IsTokenActive_TokenIsExpired()
        {
            var user = CreateRandomUser();
            var totpTokentoCheck = _userService.GenerateTotpToken(user);
            _unitOfWork.Context.Totp_Users.Add(new Totp_user
            {
                Token = totpTokentoCheck,
                Create_date = DateTime.UtcNow,
                Expire_date = DateTime.UtcNow.AddSeconds(-1),
                User = user


            });

            _unitOfWork.Context.SaveChanges();
            var value = _unitOfWork.Users.IsTokenActive(user);

            Assert.False(value);

        }
        [Fact]
        public void IsTokenActive_TokenTimeIsTheSameAsExpiredData()
        {
            var user = CreateRandomUser();
            var totpTokentoCheck = _userService.GenerateTotpToken(user);
            _unitOfWork.Context.Totp_Users.Add(new Totp_user
            {
                Token = totpTokentoCheck,
                Create_date = DateTime.UtcNow,
                Expire_date = DateTime.UtcNow,
                User = user


            });

            _unitOfWork.Context.SaveChanges();
            var value = _unitOfWork.Users.IsTokenActive(user);

            Assert.False(value);
        }
        [Fact]
        public void GenerateAuthToken_ValidCall()
        {
            
            
            var accessToken = _userService.GenerateAuthToken(CreateRandomUser());
            Assert.NotNull(accessToken);

            RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(
                     source: Convert.FromBase64String(_config["JwtSettings:Asymmetric:PublicKey"]),
                     bytesRead: out int _
                 );

            


            var keyBytes = Encoding.UTF8.GetBytes("!z%C*F-JaNdRgUkXp2s5v8x/A?D(G+Kb");
            var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new RsaSecurityKey(rsa),
                TokenDecryptionKey = symmetricSecurityKey,
                ValidateIssuer = true,
                ValidIssuer = "https://localhost:44324/",
                ValidateAudience = true,
                ValidAudience = "https://localhost:44301/",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero

            };






            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(
            accessToken.JwtToken,
            tokenValidationParameters,
            out SecurityToken securityToken);

            Assert.NotNull(claimsPrincipal);
            Assert.True(claimsPrincipal.Claims.Count() > 0);
            Assert.True(claimsPrincipal.Identity.Name.Equals(CreateRandomUser().Id.ToString()));
           
        }
        [Fact]
        public void ChangeMasterPassword_ValidCallShouldChangePasswordAndThrowsNoErrorIfUserHasNoData()
        {
            var testUser = CreateRandomUser();
            _unitOfWork.Context.Add<User>(testUser);
            _unitOfWork.SaveChanges();
            
            
            bool result = _userService.ChangeMasterPassword("zxczxc", "przemek", testUser.Id.ToString());
            Assert.True(result);
            mockEmail.Verify(a => a.SendEmailAsync(It.IsAny<Message>()));
            
           
            var updatedUser = _unitOfWork.Context.Users.Find(testUser.Id);
            
            bool resultNewPasswordHash = _userService.VerifyPasswordHash("przemek", Convert.FromBase64String(updatedUser.Password), Convert.FromBase64String(updatedUser.PasswordSalt));

            Assert.True(resultNewPasswordHash);

            bool resultOldPasswordHash = _userService.VerifyPasswordHash("zxczxc", Convert.FromBase64String(updatedUser.Password), Convert.FromBase64String(updatedUser.PasswordSalt));

            
            Assert.False(resultOldPasswordHash);

            


        }
        [Fact]
        public void ChangeMasterPassword_ValidCallShouldChangePasswordAndThrowsNoErrorIfUserHasAnyData()
        {
            var testUser = CreateRandomUser();
            _unitOfWork.Context.Add<User>(testUser);
            _unitOfWork.SaveChanges();

            PopulateUserWithData(testUser);


            bool result = _userService.ChangeMasterPassword("zxczxc", "przemek8", testUser.Id.ToString());
            Assert.True(result);
            

            var updatedUser = _unitOfWork.Context.Users.Find(testUser.Id);

            bool resultNewPasswordHash = _userService.VerifyPasswordHash("przemek8", Convert.FromBase64String(updatedUser.Password), Convert.FromBase64String(updatedUser.PasswordSalt));

            Assert.True(resultNewPasswordHash);

            bool resultOldPasswordHash = _userService.VerifyPasswordHash("zxczxc", Convert.FromBase64String(updatedUser.Password), Convert.FromBase64String(updatedUser.PasswordSalt));


            Assert.False(resultOldPasswordHash);





        }

        private void PopulateUserWithData(User user)
        {
            var logins = new List<LoginData>
            {
                new LoginData
                {
                    Login = "login1", Name = "login1", Password = "zxczxc", UserId = user.Id
                },
                new LoginData
                {
                     Login = "login2", Name = "login2", Password = "zxczxc", UserId = user.Id
                }
            };
            var creditCards = new List<CreditCard>
            {
                new CreditCard
                {
                       CardHolderName = "cardholder1", CardNumber = "12345",  User = user
                },
                new CreditCard
                {
                     CardHolderName = "cardholder2", CardNumber = "123145",  User = user
                }
            };
            _unitOfWork.Context.LoginDatas.AddRange(logins);
            _unitOfWork.Context.CreditCards.AddRange(creditCards);
            _unitOfWork.SaveChanges();


        }

        [Fact]
        public void ChangeMasterPassword_ValidCallShouldCallSendEmailMethodOnlyOnce()
        {
            var testUser = CreateRandomUser();
            _unitOfWork.Context.Add<User>(testUser);
            _unitOfWork.SaveChanges();


            bool result = _userService.ChangeMasterPassword("zxczxc", "przemek2", testUser.Id.ToString());
           
            Assert.True(result);
           
            mockEmail.Verify(a => a.SendEmailAsync(It.IsAny<Message>()),Times.Once);

            




        }
        [Fact]
        public void ChangeMasterPassword_ValidCallShouldDecryptAndThenEncryptUserDataWithNewPassword()
        {
            var testUser = CreateRandomUser();
            _unitOfWork.Context.Add<User>(testUser);
            _unitOfWork.SaveChanges();

            PopulateUserWithData(testUser);

            bool result = _userService.ChangeMasterPassword("zxczxc", "przemek8", testUser.Id.ToString());
           
            Assert.True(result);
            
            mockEncryptionService.Verify(a => a.Decrypt("zxczxc", It.IsAny<string>()), Times.Exactly(8));
            
            mockEncryptionService.Verify(a => a.Encrypt("przemek8", It.IsAny<string>()), Times.Exactly(8));





        }
       






       







    }

}
        
 

    


