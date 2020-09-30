


using System.ComponentModel.DataAnnotations;
using PasswordManagerAppResourceServer.Models;

namespace PasswordManagerAppResourceServer.Dtos
{
    
    public class NoteDto
    {
        
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        
        [Required]
        public string Details { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        

       

    }
}