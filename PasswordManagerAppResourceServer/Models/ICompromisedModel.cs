using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    public interface ICompromisedModel
    {
        public string Name { get; set; }

        public int Compromised { get; set; }



    }
}
