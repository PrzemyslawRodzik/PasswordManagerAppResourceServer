using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerAppResourceServer.Models
{
    [Table("note")]
    public class Note: UserRelationshipModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("Name")]
        public string Name { get; set; }

        [Required]
        [Column("Details")]

        public string Details { get; set; }

       

    }
}
