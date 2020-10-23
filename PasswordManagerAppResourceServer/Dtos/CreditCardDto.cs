
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PasswordManagerAppResourceServer.Dtos
{
    
    public class CreditCardDto
    {
        
        
        
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        [Required]
       
        public string CardHolderName { get; set; }
        [Required]
        
        public string CardNumber { get; set; }
        [Required]
        
        public string SecurityCode { get; set; }
        [Required]
       
        public string ExpirationDate { get; set; }

        public int UserId { get; set; }

      }  

    
}
