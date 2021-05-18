using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class OrderConfirmViewModel
    {
        public string DelivetyType { get; set; }
        public string Address { get; set; }
        public int TotalPrice { get; set; }
        public int CardMonth { get; set; }
        public int CardYear { get; set; }
        [Required]
        [DataType(DataType.CreditCard, ErrorMessage = "Wrong card number!")]
        public string CardNumber { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string CCV { get; set; }
    }
}