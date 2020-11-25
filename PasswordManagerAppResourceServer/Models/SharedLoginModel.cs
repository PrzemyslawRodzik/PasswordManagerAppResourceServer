using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using PasswordManagerAppResourceServer.Dtos;

namespace PasswordManagerAppResourceServer.Models
{
    
    public class SharedLoginModel
    {   
        
        public LoginDataDto LoginData { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
