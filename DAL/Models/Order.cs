using System;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace DAL.Models
{
    public partial class Order
    {

        public int Id { get; set; }
        [ForeignKey("UsernameNavigation")]
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Deliverytype { get; set; }
        
        [Column(TypeName = "varchar")]
        public string Address { get; set; }

        public virtual User UsernameNavigation { get; set; }
    }
}
