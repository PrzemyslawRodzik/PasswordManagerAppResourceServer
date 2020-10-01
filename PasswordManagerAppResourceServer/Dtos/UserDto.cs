using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Dtos
{
    public class UserDto
    {
        
        
            public int Id { get; set; }
            
            public string Email { get; set; }
          

            public string Password { get; set; }
            
            public string PasswordSalt { get; set; }
            
            public int TwoFactorAuthorization { get; set; }
           
            public int Admin { get; set; }
            
            public int PasswordNotifications { get; set; }
            
            public int AuthenticationTime { get; set; }

            
            public string PrivateKey { get; set; }
            
            public string PublicKey { get; set; }


            


          




        
    }
}
