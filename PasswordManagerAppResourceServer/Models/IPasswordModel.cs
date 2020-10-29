using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    public interface IPasswordModel
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
