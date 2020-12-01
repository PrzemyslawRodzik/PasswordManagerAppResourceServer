using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Dtos
{
    public class AddressDto
    {

        public int Id { get; set; }

        [Required]
        
        public string AddressName { get; set; }

        [Required]
        
        public string Street { get; set; }
        [Required]

        public string StreetNumber { get; set; }

        [Required]
        
        public string ZipCode { get; set; }

        [Required]
        
        public string City { get; set; }

       

        public int PersonalInfoId { get; set; }
        public int UserId { get; set; }
        





    }
}
