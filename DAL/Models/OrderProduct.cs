using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class OrderProduct
    {
        [Required, ForeignKey("Order")]
        public int OrderId { get; set; }
        [Required, Column(TypeName = "varchar(25)")]
        public string Product { get; set; }
        [Required]
        public int Qty { get; set; }
        
        public Order Order { get; set; }
    }
}