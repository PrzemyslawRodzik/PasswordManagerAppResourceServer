
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    public class PersonalModel
    {
        [Column("personal_info_id")]
        public int PersonalInfoId { get; set; }
        public PersonalInfo PersonalInfo { get; set; }
    }
}
