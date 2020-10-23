using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Models;

namespace PasswordManagerAppResourceServer.Responses
{
    public class AuthSuccessRegisterResponse
    {
        public AccessToken AccessToken { get; set; }

        public UserDto UserDto { get; set; }
        
    }
}