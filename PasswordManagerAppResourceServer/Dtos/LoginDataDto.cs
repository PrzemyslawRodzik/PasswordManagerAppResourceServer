using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Dtos
{
    public class LoginDataDto
    {
       
        public int Id { get; set; }
        [Required]
        
        public string Name { get; set; }

       
        public string Email { get; set; }
        [Required]
        
        public string Login { get; set; }

        [Required]
        
        public string Password { get; set; }
        
        public string Website { get; set; }
        [Required]
       
        public int Compromised { get; set; }
        [Required]
        
        public DateTime ModifiedDate { get; set; }
        [Required]
        public int UserId { get; set; }

        


    }
}
