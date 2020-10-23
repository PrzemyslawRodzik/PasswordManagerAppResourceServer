using PasswordManagerAppResourceServer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Dtos
{
    public class PaypalAccountDto 
    {   
        public int Id { get; set; }

        [Required]
        
        public string Email { get; set; }

        [Required]
        
        public string Password { get; set; }

        [Required]
        
        public int Compromised { get; set; }

        [Required]
        public DateTime ModifiedDate { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
