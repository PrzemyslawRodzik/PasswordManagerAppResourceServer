using System.ComponentModel.DataAnnotations;

namespace PasswordManagerAppResourceServer.Models
{
    public class UserLoginRequest
    {
        
        
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}