using PasswordManagerAppResourceServer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Responses
{
    public class AuthResponse : ApiResponse
    {
        public AccessToken AccessToken { get; set; }

        
        
    }
}
