using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Dtos
{
    public class PersonalInfoDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SecondName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int UserId { get; set; }
    }
}




