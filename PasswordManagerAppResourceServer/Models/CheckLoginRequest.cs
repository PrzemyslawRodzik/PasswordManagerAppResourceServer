using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    public class CheckLoginRequest
    {
        public string Website { get; set; }
        public string Login { get; set; }
    }
}
