
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PasswordManagerAppResourceServer.Dtos
{   [Table("phone_number")]
    public class PhoneNumberDto
    {
        
        public int Id { get; set; }
        [Required]
        
        public string NickName { get; set; }
        [Required]
        
        public string TelNumber { get; set; }
        [Required]
      
        public string Type { get; set; }

        public int PersonalInfoId { get; set; }
        public int UserId { get; set; }



    }
}
