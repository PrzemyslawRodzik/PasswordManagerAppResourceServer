using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    public class PasswordStatusUpdate
    {
        public int UserId { get; set; }
        public int Compromised { get; set; }
    }
}
