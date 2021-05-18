using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class OrderHistory
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        [Column(TypeName = "varchar(9)")]
        public string Status { get; set; }
        public DateTime Time { get; set; }
        public Order Order { get; set; }
    }
}