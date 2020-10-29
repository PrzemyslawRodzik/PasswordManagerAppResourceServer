using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    [Table("address")]
    public class Address: PersonalModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("address_name")]
        public string AddressName { get; set; }

        [Required]
        [Column("street")]
        public string Street { get; set; }

        [Required]
        [Column("zip_code")]
        public string ZipCode { get; set; }

        [Required]
        [Column("city")]
        public string City { get; set; }

        [Required]
        [Column("country")]
        public string Country { get; set; }

        

    }
}
