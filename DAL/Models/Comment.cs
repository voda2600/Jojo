using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Repositories;
using MongoDB.Driver;

#nullable disable

namespace DAL.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        [ForeignKey("UsernameNavigation")]
        public string Username { get; set; }
        [Required, Column(TypeName = "varchar(25)")]
        public string Product { get; set; }
        public string Content { get; set; }
        public double Mark { get; set; }
        
        public virtual User UsernameNavigation { get; set; }
    }
}
