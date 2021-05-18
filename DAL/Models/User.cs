using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace DAL.Models
{
    public partial class User
    {
        public User()
        {
            Comments = new HashSet<Comment>();
            Orders = new HashSet<Order>();
        }
        [Key]
        public string Username { get; set; }
        public string Email { get; set; }

        [Column(TypeName = "varchar")]
        [Required]
        public string Password { get; set; }

        
        [Column(TypeName = "varchar(5)")]
        public string Role { get; set; }
        
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
